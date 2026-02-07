# Rust Backend Integration for Brightness Control Widget

This document describes the Rust backend changes needed in the [Rusty Twinkle Tray](https://github.com/Linx-ESP/rusty-twinkle-tray) project to support the Xbox Game Bar Brightness Control Widget.

## Overview

The widget requires an HTTP API server running on `http://localhost:7842` that exposes endpoints for querying monitors and controlling brightness.

## Required Dependencies

Add these to your `Cargo.toml`:

```toml
[dependencies]
tiny_http = "0.12"
serde = { version = "1.0", features = ["derive"] }
serde_json = "1.0"
```

## API Implementation

Add the following code to your `main.rs` (or appropriate module):

### 1. Start API Server in Background Thread

```rust
use tiny_http::{Server, Response, Header, Method};
use std::thread;
use std::sync::{Arc, Mutex};
use serde::{Serialize, Deserialize};

// Add to your run() function or main entry point
fn run() -> Result<()> {
    // ... existing code ...
    
    // Start API server in background thread
    let config_clone = config.clone();
    let controller_sender = wnd_sender.clone();
    thread::spawn(move || {
        start_api_server(config_clone, controller_sender);
    });
    
    // ... rest of existing code ...
}
```

### 2. API Server Implementation

```rust
#[derive(Serialize, Deserialize)]
struct MonitorInfo {
    id: String,
    name: String,
    brightness: u8,
}

#[derive(Deserialize)]
struct BrightnessUpdate {
    brightness: u8,
}

fn start_api_server(
    config: Arc<Mutex<Config>>, 
    sender: Sender<CustomEvent>
) {
    let server = Server::http("127.0.0.1:7842").unwrap();
    
    for request in server.incoming_requests() {
        // Enable CORS for local requests
        let cors_header = Header::from_bytes(
            &b"Access-Control-Allow-Origin"[..], 
            &b"*"[..]
        ).unwrap();
        
        let response = match (request.method(), request.url()) {
            // GET /monitors - Return list of all monitors with current brightness
            (Method::Get, "/monitors") => {
                let monitors = get_all_monitors(); // Your existing function
                let monitors_json: Vec<MonitorInfo> = monitors.iter().map(|m| {
                    MonitorInfo {
                        id: m.id.clone(),
                        name: m.name.clone(),
                        brightness: m.brightness,
                    }
                }).collect();
                
                Response::from_string(serde_json::to_string(&monitors_json).unwrap())
                    .with_header(Header::from_bytes(
                        &b"Content-Type"[..], 
                        &b"application/json"[..]
                    ).unwrap())
                    .with_header(cors_header)
            }
            
            // POST /monitor/{id} - Set brightness for specific monitor
            (Method::Post, url) if url.starts_with("/monitor/") => {
                let monitor_id = &url[9..]; // Extract ID after "/monitor/"
                
                // Read request body
                let mut content = String::new();
                request.as_reader().read_to_string(&mut content).unwrap();
                
                // Parse brightness value
                if let Ok(update) = serde_json::from_str::<BrightnessUpdate>(&content) {
                    // Send brightness change event to your existing controller
                    sender.send(CustomEvent::BrightnessChanged { 
                        monitor_id: monitor_id.to_string(),
                        value: update.brightness
                    }).unwrap();
                    
                    Response::from_string("OK")
                        .with_header(cors_header)
                } else {
                    Response::from_string("Invalid request").with_status_code(400)
                        .with_header(cors_header)
                }
            }
            
            // OPTIONS requests for CORS preflight
            (Method::Options, _) => {
                Response::from_string("")
                    .with_header(cors_header)
                    .with_header(Header::from_bytes(
                        &b"Access-Control-Allow-Methods"[..],
                        &b"GET, POST, OPTIONS"[..]
                    ).unwrap())
                    .with_header(Header::from_bytes(
                        &b"Access-Control-Allow-Headers"[..],
                        &b"Content-Type"[..]
                    ).unwrap())
            }
            
            // 404 for unknown routes
            _ => Response::from_string("Not Found").with_status_code(404)
                .with_header(cors_header)
        };
        
        request.respond(response).unwrap();
    }
}
```

### 3. Custom Event Definition

If you don't already have a `CustomEvent` type, add this:

```rust
pub enum CustomEvent {
    BrightnessChanged {
        monitor_id: String,
        value: u8,
    },
    // ... other events ...
}
```

### 4. Monitor Information Structure

Make sure your monitor structure includes the necessary fields:

```rust
pub struct Monitor {
    pub id: String,
    pub name: String,
    pub brightness: u8,
    // ... other fields ...
}
```

## Integration Notes

1. **Thread Safety**: The API server runs in a separate thread, so use `Arc<Mutex<T>>` for shared state
2. **CORS**: CORS headers are included to allow the WebView to make requests
3. **Error Handling**: Add proper error handling for production use
4. **Logging**: Consider adding logging for debugging API requests
5. **Security**: The API only listens on localhost (127.0.0.1) for security

## Testing the API

You can test the API endpoints using curl:

```bash
# Get all monitors
curl http://localhost:7842/monitors

# Set brightness for a monitor
curl -X POST http://localhost:7842/monitor/monitor-1 \
  -H "Content-Type: application/json" \
  -d '{"brightness": 75}'
```

## Troubleshooting

### Port Already in Use
If port 7842 is already in use, you can change it in both:
- Rust backend: `Server::http("127.0.0.1:PORT")`
- Widget HTML: `const API_BASE = 'http://localhost:PORT'`

### CORS Errors
Make sure CORS headers are properly set on all responses

### Widget Can't Connect
- Verify the Rust backend is running
- Check Windows Firewall settings
- Ensure no antivirus is blocking local connections

## Security Considerations

1. **Localhost Only**: The server only binds to 127.0.0.1 (localhost)
2. **No Authentication**: This is a local-only service, so no auth is implemented
3. **Input Validation**: Validate brightness values (0-100 range)
4. **Rate Limiting**: Consider adding rate limiting for production

## Future Enhancements

- Add WebSocket support for real-time brightness updates
- Support for monitor profiles
- Night mode scheduling
- Multi-monitor presets
