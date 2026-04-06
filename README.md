<div align="center">

<img src="docs/media/flipqlo.png" alt="Flipqlo Banner" width="100%" />

# Flipqlo Reborn

A dual-platform flip clock screensaver project with a native Windows `.scr` and Android DreamService build in one repo.

[![Windows Build](https://img.shields.io/badge/Windows-.scr-black?style=for-the-badge&logo=windows)](https://github.com/FestinBiju/Flipqlo/releases/latest/download/Flipqlo-Windows-x64.scr)
[![Android Build](https://img.shields.io/badge/Android-APK-3DDC84?style=for-the-badge&logo=android&logoColor=white)](https://github.com/FestinBiju/Flipqlo/releases/latest/download/Flipqlo-Android.apk)
[![Release](https://img.shields.io/github/v/tag/FestinBiju/Flipqlo?sort=semver&style=for-the-badge&cacheSeconds=120)](https://github.com/FestinBiju/Flipqlo/releases)
[![License](https://img.shields.io/github/license/FestinBiju/Flipqlo?style=for-the-badge)](LICENSE)

</div>

## Download Now

- Windows `.scr`: https://github.com/FestinBiju/Flipqlo/releases/latest/download/Flipqlo-Windows-x64.scr
- Android APK: https://github.com/FestinBiju/Flipqlo/releases/latest/download/Flipqlo-Android.apk


## Visuals

| Windows Screensaver | Android DreamService |
|---|---|
| ![Windows Preview](docs/media/windows-preview.png) | ![Android Screenshot](android-src/emulator-settings-screen.png) |

### Demo Video

![Flipqlo Screensaver Demo](docs/media/video.gif)

## Usage Graph

```mermaid
pie title Platform Usage Split (example)
    "Windows .scr" : 72
    "Android DreamService" : 28
```

```mermaid
flowchart LR
    A[Open Repository] --> B{Choose Platform}
    B --> C[Download .scr]
    B --> D[Download APK]
    C --> E[Install on Windows]
    D --> F[Install on Android]
```

## Repository Structure

This monorepo keeps both codebases together but clearly separated by platform:

- `windows-src/` - WPF Windows screensaver source (`.scr` target)
- `android-src/` - Android app + DreamService source
- `shared/` - cross-platform design tokens and shared specs
- `docs/` - screenshots, behavior spec, and docs assets

## Use this on your device
just download the latest version from the Releases window in the right side. You will have options to download for windows and android over there.
### for windows
- just right click on the downloaded .scr file and in the context window, you will have an option to install. click install and you will be good to go.
### for android 
- download the .apk file and install it on your device
- after installation, go to the screen saver settings on your device and choose fliqlo.

## Build Locally

### Windows `.scr`

```bash
# from repo root
bash windows-src/build.sh
```

Windows output:

- `windows-src/Flipqlo/bin/Release/net10.0-windows/win-x64/publish/Flipqlo.scr`

### Android APK

```bash
cd android-src
./gradlew assembleRelease
```

Android output:

- `android-src/app/build/outputs/apk/release/app-release.apk`

## Source Code Entry Points

### Windows

- `windows-src/Flipqlo/Program.cs` - `/s`, `/p`, `/c` screensaver modes
- `windows-src/Flipqlo/ScreensaverWindow.xaml.cs` - full-screen + preview hosting logic
- `windows-src/Flipqlo/Rendering/FlipClockRenderer.cs` - flip digit rendering and animation pipeline
- `windows-src/Flipqlo/Engine/ClockEngine.cs` - clock timing and state transitions

### Android

- `android-src/app/src/main/java/com/flipqlo/screensaver/FlipClockDreamService.kt` - DreamService entry point
- `android-src/app/src/main/java/com/flipqlo/screensaver/FlipClockView.kt` - custom rendering view
- `android-src/app/src/main/java/com/flipqlo/screensaver/ClockEngine.kt` - clock timing and transitions
- `android-src/app/src/main/java/com/flipqlo/screensaver/SettingsActivity.kt` - settings UI

## Open Source

Flipqlo is now a FOSS project under MIT License.

- License: [LICENSE](LICENSE)
- Contributing guide: [CONTRIBUTING.md](CONTRIBUTING.md)
- Code of conduct: [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)
- Security policy: [SECURITY.md](SECURITY.md)
- Support guide: [SUPPORT.md](SUPPORT.md)

If you want to help, start with issues labeled `good first issue` or `help wanted`.

## Maintainer Roadmap

Near-term goals:

1. Improve Android DreamService stability across OEM variants.
2. Add more customization options (font weight, scale, color presets).
3. Expand automated tests for clock transitions and settings persistence.
4. Improve installer/distribution UX for Windows `.scr`.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).

