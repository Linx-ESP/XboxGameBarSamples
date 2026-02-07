# Quick Start Guide - Brightness Control Widget

This guide will help you get the Brightness Control Widget running quickly.

## Prerequisites Checklist

- [ ] Windows 10 version 1903 or later (or Windows 11)
- [ ] Xbox Game Bar enabled (Windows Settings > Gaming > Xbox Game Bar)
- [ ] Visual Studio 2019 or later with "Universal Windows Platform development" workload
- [ ] Rusty Twinkle Tray installed and configured

## Step 1: Build the Widget

1. Open Visual Studio
2. Open `BrightnessControlWidget.csproj`
3. Select your platform (x64 recommended for most systems)
4. Build > Build Solution (Ctrl+Shift+B)
5. If you get package restore errors, right-click the solution and select "Restore NuGet Packages"

## Step 2: Deploy the Widget

### For Development/Testing:
1. Right-click the project in Solution Explorer
2. Select "Deploy"
3. Wait for deployment to complete

### For Distribution:
1. Build > Create App Packages
2. Follow the wizard to create an APPX package
3. Install the package on target machines

## Step 3: Set Up Rusty Twinkle Tray Backend

### Add Required Dependencies

Edit `Cargo.toml` in your Rusty Twinkle Tray project:

```toml
[dependencies]
tiny_http = "0.12"
serde = { version = "1.0", features = ["derive"] }
serde_json = "1.0"
```

### Implement the API

See `BACKEND_INTEGRATION.md` for complete implementation details. The key steps are:

1. Add the API server code to `main.rs`
2. Start the server in a background thread
3. Implement the `/monitors` and `/monitor/{id}` endpoints

### Build and Run the Backend

```bash
cd rusty-twinkle-tray
cargo build --release
cargo run --release
```

The server should start on `http://localhost:7842`

## Step 4: Test the Widget

1. Press **Win+G** to open Xbox Game Bar
2. Click the widgets menu (three horizontal lines icon)
3. Look for "Monitor Brightness" in the widget list
4. Click to open the widget

### Expected Behavior:

**If Backend is Running:**
- Widget displays sliders for each detected monitor
- Moving a slider updates brightness in real-time
- Percentage value updates next to each slider

**If Backend is NOT Running:**
- Widget displays: "⚠️ Backend not running. Please start Rusty Twinkle Tray."

## Troubleshooting

### Widget doesn't appear in Game Bar

1. Check if the app is installed:
   ```powershell
   Get-AppxPackage | Select-String "BrightnessControlWidget"
   ```

2. If not found, redeploy from Visual Studio

3. Restart Game Bar:
   - Close Game Bar completely
   - Press Win+G to reopen

### Widget shows "Backend not running" error

1. Verify Rusty Twinkle Tray is running
2. Test the API manually:
   ```bash
   curl http://localhost:7842/monitors
   ```
3. Check Windows Firewall isn't blocking local connections
4. Ensure port 7842 is not in use by another application

### Brightness doesn't change when moving slider

1. Check Rusty Twinkle Tray console for errors
2. Verify your monitors support DDC/CI brightness control
3. Try the API directly:
   ```bash
   curl -X POST http://localhost:7842/monitor/your-monitor-id \
     -H "Content-Type: application/json" \
     -d '{"brightness": 50}'
   ```

### WebView shows blank page

1. Check for Windows updates (WebView2 dependency)
2. Verify `widget.html` is included in the build (check .csproj)
3. Look for errors in Visual Studio Output window during deployment

## Development Tips

### Testing HTML Changes

1. Edit `widget.html`
2. Rebuild the project
3. Redeploy to see changes
4. No need to restart Game Bar for HTML changes

### Debugging the Widget

1. Add `Debugger.Launch()` in Widget1.xaml.cs constructor
2. Rebuild and deploy
3. Visual Studio will attach when widget opens

### Changing the API URL

Edit `widget.html` line 41:
```javascript
const API_BASE = 'http://localhost:YOUR_PORT';
```

## Common Customizations

### Change Widget Size

Edit `Package.appxmanifest` lines 55-62:
```xml
<Size>
  <Height>300</Height>
  <Width>350</Width>
  ...
</Size>
```

### Customize UI Appearance

Edit `widget.html` CSS section (lines 6-35) to change:
- Colors
- Fonts
- Spacing
- Layout

### Add Additional Features

Modify `widget.html` to add:
- Preset brightness levels (buttons for 0%, 50%, 100%)
- Monitor profiles
- Keyboard shortcuts
- Animations

## Next Steps

- Read `README.md` for detailed documentation
- See `BACKEND_INTEGRATION.md` for Rust backend details
- Check Xbox Game Bar SDK documentation: https://docs.microsoft.com/gaming/game-bar
- Report issues on GitHub

## Support

For issues with:
- **The Widget**: Check this repository's issues
- **Rusty Twinkle Tray**: Check the Rusty Twinkle Tray repository
- **Game Bar SDK**: See official Microsoft documentation
