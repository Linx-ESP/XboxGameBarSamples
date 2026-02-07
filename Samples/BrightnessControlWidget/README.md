# Brightness Control Widget for Xbox Game Bar

This Xbox Game Bar widget provides quick access to monitor brightness controls through an HTML-based interface that communicates with [Rusty Twinkle Tray](https://github.com/Linx-ESP/rusty-twinkle-tray).

## Features

- Control multiple monitor brightness levels from Game Bar
- Real-time brightness adjustment with sliders
- Clean, minimal interface that matches Game Bar aesthetic
- WebView-based HTML interface

## Prerequisites

1. **Windows 10/11** with Xbox Game Bar enabled
2. **Rusty Twinkle Tray** backend running on `http://localhost:7842`
3. **Visual Studio 2019 or later** with UWP development tools

## Backend Requirements

This widget requires the Rusty Twinkle Tray backend to be running with an HTTP API server. The backend should expose the following endpoints:

### GET /monitors
Returns a JSON array of monitors with their current brightness levels:
```json
[
  {
    "id": "monitor-1",
    "name": "Display 1",
    "brightness": 75
  }
]
```

### POST /monitor/{id}
Sets the brightness for a specific monitor:
```json
{
  "brightness": 50
}
```

## Building the Widget

1. Open `BrightnessControlWidget.csproj` in Visual Studio
2. Restore NuGet packages
3. Build for your target platform (x86, x64, or ARM64)
4. Deploy the application

## Installation

1. Build the project in Visual Studio
2. Deploy to your local machine
3. Start the Rusty Twinkle Tray backend
4. Open Xbox Game Bar (Win+G)
5. Find "Monitor Brightness" in the widget menu

## Widget Configuration

The widget is configured in `Package.appxmanifest`:

- **Display Name**: Monitor Brightness
- **Description**: Quick monitor brightness control
- **Window Size**: 
  - Default: 350x300 pixels
  - Min: 250x150 pixels
  - Max: 400x400 pixels
- **Features**: 
  - Home menu visible
  - Pinning supported
  - Resizable

## Customization

### Modifying the UI

Edit `widget.html` to customize:
- Colors and styling (CSS)
- API endpoint (`API_BASE` constant)
- UI layout and controls

### Adjusting Widget Size

Edit `Package.appxmanifest` to change:
- Initial window dimensions
- Minimum/maximum sizes
- Resize behavior

## Troubleshooting

### Widget shows "Backend not running"

- Ensure Rusty Twinkle Tray is running on port 7842
- Check that the API server is accessible at `http://localhost:7842`
- Verify firewall settings allow local connections

### Widget doesn't appear in Game Bar

- Ensure the app is properly installed/deployed
- Check that Game Bar is enabled in Windows Settings
- Try restarting Game Bar

### Brightness changes don't work

- Verify the backend API is responding correctly
- Check browser console in WebView for errors
- Ensure monitor supports DDC/CI brightness control

## Technical Details

### Architecture

- **Framework**: UWP (Universal Windows Platform)
- **Language**: C# with XAML
- **WebView**: Hosts HTML/JavaScript interface
- **Game Bar SDK**: Microsoft.Gaming.XboxGameBar 7.2.240903001

### File Structure

```
BrightnessControlWidget/
├── App.xaml / App.xaml.cs          # Application entry point
├── Widget1.xaml / Widget1.xaml.cs  # Main widget with WebView
├── MainPage.xaml / MainPage.xaml.cs # Standard launch page
├── widget.html                      # HTML interface
├── Package.appxmanifest            # App manifest and Game Bar config
├── BrightnessControlWidget.csproj  # Project file
└── Assets/                         # App icons and images
```

## License

This sample is provided as-is under the MIT License.

## Related Projects

- [Rusty Twinkle Tray](https://github.com/Linx-ESP/rusty-twinkle-tray) - Backend brightness control service
- [Xbox Game Bar SDK Samples](https://github.com/microsoft/XboxGameBarSamples) - More widget examples
