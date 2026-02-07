# Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Windows 10/11                            │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                  Xbox Game Bar (Win+G)                    │  │
│  │                                                           │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │   Brightness Control Widget (UWP App)             │  │  │
│  │  │                                                    │  │  │
│  │  │   ┌────────────────────────────────────────────┐  │  │  │
│  │  │   │  WebView2 Control                          │  │  │  │
│  │  │   │                                            │  │  │  │
│  │  │   │  ┌──────────────────────────────────────┐ │  │  │  │
│  │  │   │  │  widget.html                         │ │  │  │  │
│  │  │   │  │  • HTML/CSS/JavaScript               │ │  │  │  │
│  │  │   │  │  • Brightness sliders                │ │  │  │  │
│  │  │   │  │  • Real-time updates                 │ │  │  │  │
│  │  │   │  │  • Error handling UI                 │ │  │  │  │
│  │  │   │  └──────────────────────────────────────┘ │  │  │  │
│  │  │   └────────────────────────────────────────────┘  │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│                              │ HTTP Requests                    │
│                              │ (localhost:7842)                 │
│                              ▼                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │           Rusty Twinkle Tray Backend (Rust)              │  │
│  │                                                           │  │
│  │   ┌───────────────────────────────────────────────────┐  │  │
│  │   │  HTTP API Server (tiny_http)                      │  │  │
│  │   │  • GET  /monitors    → List monitors             │  │  │
│  │   │  • POST /monitor/:id → Set brightness            │  │  │
│  │   └───────────────────────────────────────────────────┘  │  │
│  │                              │                            │  │
│  │                              │ DDC/CI Commands            │  │
│  │                              ▼                            │  │
│  │   ┌───────────────────────────────────────────────────┐  │  │
│  │   │  Monitor Control                                  │  │  │
│  │   │  • Detect monitors                                │  │  │
│  │   │  • Read current brightness                        │  │  │
│  │   │  • Set brightness via DDC/CI                      │  │  │
│  │   └───────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
└──────────────────────────────┼──────────────────────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │  Physical Monitors   │
                    │  (via DDC/CI)        │
                    └──────────────────────┘
```

## Component Details

### 1. Brightness Control Widget (UWP)

**Technology:** C# / XAML / UWP
**Package:** BrightnessControlWidget.appx

**Key Files:**
- `App.xaml.cs` - Application entry point, handles Game Bar activation
- `Widget1.xaml` - Main widget UI with WebView container
- `widget.html` - HTML interface loaded in WebView
- `Package.appxmanifest` - Defines Game Bar extension

**Responsibilities:**
- Register as Game Bar widget
- Host WebView for HTML UI
- Handle widget lifecycle (open/close/resize)
- Provide container for web content

### 2. HTML Interface

**Technology:** HTML5 / CSS3 / JavaScript (ES6+)
**File:** `widget.html`

**Features:**
- Dynamic monitor list generation
- Range sliders for brightness control
- Real-time percentage display
- Error handling UI
- Async API communication

**API Integration:**
```javascript
// Fetch monitors
GET http://localhost:7842/monitors
→ [{ id, name, brightness }, ...]

// Update brightness
POST http://localhost:7842/monitor/{id}
Body: { brightness: 0-100 }
```

### 3. Rusty Twinkle Tray Backend

**Technology:** Rust
**Dependencies:** tiny_http, serde, serde_json

**Components:**
- HTTP API server (port 7842)
- Monitor detection/enumeration
- DDC/CI communication layer
- Brightness control logic

**API Endpoints:**
- `GET /monitors` - Returns JSON array of all monitors
- `POST /monitor/:id` - Updates brightness for specific monitor
- `OPTIONS /*` - CORS preflight support

### 4. Monitor Hardware

**Communication:** DDC/CI (Display Data Channel Command Interface)
**Protocol:** I²C over VGA/DVI/HDMI/DisplayPort

**Requirements:**
- Monitor must support DDC/CI
- DDC/CI must be enabled in monitor OSD
- Proper cable connection (some cables don't support DDC/CI)

## Data Flow

### Loading Monitors (Startup)

```
1. User opens Game Bar (Win+G)
2. Game Bar launches BrightnessControlWidget
3. Widget loads widget.html in WebView
4. JavaScript calls GET /monitors
5. Rust backend queries system for monitors via DDC/CI
6. Backend returns JSON array of monitors
7. JavaScript renders slider for each monitor
8. UI displays to user
```

### Adjusting Brightness (User Action)

```
1. User moves slider
2. JavaScript oninput event fires
3. JavaScript calls POST /monitor/{id}
4. Request body contains new brightness value
5. Rust backend receives request
6. Backend sends DDC/CI command to monitor
7. Monitor hardware updates brightness
8. JavaScript updates percentage display
```

### Error Handling

```
If backend unreachable:
1. fetch() throws exception
2. JavaScript catch block executes
3. Error message displayed to user
4. User sees: "⚠️ Backend not running..."
```

## Security Considerations

### Network Security
- ✅ Server binds to localhost only (127.0.0.1)
- ✅ No external network access
- ✅ No authentication needed (local-only)
- ⚠️ Any local process can access API

### UWP Permissions
- `internetClient` capability required for HTTP requests
- Limited to localhost requests
- Sandboxed UWP environment

### Monitor Access
- DDC/CI requires no special permissions
- Low-level I²C communication handled by Windows
- No kernel-mode drivers needed

## Performance Characteristics

### Response Times
- Monitor detection: ~100-500ms
- Brightness query: ~50-100ms per monitor
- Brightness update: ~50-100ms
- HTTP overhead: <10ms (localhost)

### Resource Usage
- Widget memory: ~20-50MB (includes WebView2)
- Backend memory: ~5-10MB
- CPU usage: <1% idle, <5% during adjustments
- Network: 0 (localhost only)

## Scalability

### Multiple Monitors
- Backend enumerates all monitors
- Widget creates slider for each
- Updates are independent
- No limit on monitor count

### Multiple Widgets
- Each widget instance is independent
- All share same backend
- Backend handles concurrent requests
- No widget-to-widget communication

## Extension Points

### Adding Features to Widget

1. **Presets** - Add buttons for 0%, 50%, 100%
2. **Profiles** - Save/load monitor configurations
3. **Schedules** - Time-based brightness adjustment
4. **Curves** - Custom brightness response curves
5. **Sync** - Link multiple monitors together

### Backend Enhancements

1. **WebSocket** - Real-time brightness updates
2. **Profiles API** - Save/load configurations
3. **Statistics** - Track brightness usage patterns
4. **Auto-brightness** - Ambient light sensor integration
5. **Remote Control** - Network API for automation

## Troubleshooting Flow

```
Widget doesn't appear
    ├─> Check if app installed → Redeploy from VS
    ├─> Restart Game Bar → Close and reopen with Win+G
    └─> Check Windows version → Requires 1903 or later

"Backend not running" error
    ├─> Check if Rust app running → cargo run
    ├─> Test API manually → curl localhost:7842/monitors
    ├─> Check firewall → Allow local connections
    └─> Check port 7842 → No other process using it

Brightness doesn't change
    ├─> Monitor support → Check DDC/CI in OSD
    ├─> Cable support → Use quality cable
    ├─> Backend errors → Check Rust console output
    └─> Test directly → Use monitor buttons to verify
```

## Development Workflow

### Making Changes

1. **HTML/CSS Changes:**
   - Edit `widget.html`
   - Rebuild widget project
   - Redeploy widget
   - Reopen in Game Bar

2. **C# Changes:**
   - Edit `.xaml.cs` files
   - Rebuild project
   - Redeploy widget
   - Restart Game Bar

3. **Backend Changes:**
   - Edit Rust code
   - `cargo build`
   - Restart Rust process
   - Test with widget

### Testing Strategy

1. **Unit Tests** - Test individual components
2. **Integration Tests** - Test API endpoints
3. **Manual Tests** - Test in Game Bar
4. **Hardware Tests** - Test with real monitors

## Future Improvements

### Short Term
- [ ] Add brightness transition animations
- [ ] Remember last brightness values
- [ ] Add keyboard shortcuts
- [ ] Implement monitor auto-detection

### Long Term
- [ ] Support for color temperature
- [ ] Multi-profile system
- [ ] Ambient light sensor integration
- [ ] Remote API for automation
- [ ] Statistics and analytics
- [ ] Cloud sync for profiles
