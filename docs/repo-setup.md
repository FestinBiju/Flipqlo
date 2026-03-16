# GitHub Setup Checklist

1. Create a private GitHub repository.
2. Push this codebase to `main`.
3. Replace `<YOUR-USERNAME>` and `<YOUR-REPO>` placeholders in `README.md`.
4. Push a first version tag (example `v2.0.0`) to trigger release assets.
5. Verify release contains:
   - `Flipqlo-Windows-x64.scr`
   - `Flipqlo-Android.apk`

## Optional hardening

- Move Android release signing from debug key to your proper release keystore.
- Add a SHA256 checksum section in release notes for both downloadable artifacts.
