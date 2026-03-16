# Contributing to Flipqlo

Thanks for helping improve Flipqlo.

## Ways to Contribute

- Report bugs
- Suggest features
- Improve docs
- Submit code fixes and enhancements
- Test releases on more devices and monitor setups

## Development Setup

### Windows

1. Install .NET SDK 10.
2. Build from repo root:

```bash
bash windows-src/build.sh
```

### Android

1. Install Java 17 and Android SDK.
2. Build from repo root:

```bash
cd android-src
./gradlew assembleRelease
```

## Branch and PR Process

1. Fork the repo.
2. Create a branch: `feature/short-description`.
3. Keep changes focused and small.
4. Open a Pull Request with:
   - What changed
   - Why it changed
   - Screenshots or short clips for UI/animation changes
   - Test notes for Windows and Android

## Coding Guidelines

- Keep naming clear and consistent.
- Avoid large refactors in bug-fix PRs.
- Update docs when behavior changes.
- Do not commit build outputs (`bin`, `obj`, `build`, `apk`, `scr`).

## Good First Issues

Look for issues labeled:

- `good first issue`
- `help wanted`
- `documentation`

## Questions

Open a GitHub Discussion or an issue if you are unsure where to start.
