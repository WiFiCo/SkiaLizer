# SkiaLizer Release Guide

## Automated Release System

This project uses GitHub Actions to automatically build and release executables when you create version tags.

## How to Create a Release

### Using GitHub Desktop:

1. **Make your changes and commit them as usual**
2. **Push to GitHub** using GitHub Desktop
3. **Create a release tag:**
   - Go to your repository on GitHub.com
   - Click "Releases" (on the right side)
   - Click "Create a new release"
   - In "Choose a tag", type a version like `v1.0.0`, `v1.1.0`, etc.
   - Click "Create new tag: v1.0.0 on publish"
   - Add a release title like "SkiaLizer v1.0.0"
   - Optionally add release notes describing what's new
   - Click "Publish release"

### Using Command Line (Alternative):

```bash
git tag v1.0.0
git push origin v1.0.0
```

## What Happens Automatically

When you create a version tag (like `v1.0.0`), GitHub Actions will:

1. ✅ **Build** both 32-bit and 64-bit Windows executables
2. ✅ **Package** them as self-contained applications (users don't need .NET installed)
3. ✅ **Create** professional zip files for distribution
4. ✅ **Publish** a GitHub release with:
   - Download links for both versions
   - Installation instructions
   - Feature highlights
   - Professional release notes

## Expected Output

Your releases will automatically include:
- `SkiaLizer-v1.0.0-win-x64.zip` (64-bit Windows)
- `SkiaLizer-v1.0.0-win-x86.zip` (32-bit Windows)

Each zip contains a single `SkiaLizer.exe` that users can run immediately.

## Build Testing

Every time you push changes to the main branch, GitHub Actions will also:
- Test that the project builds correctly
- Verify the executable is created successfully
- Check the file size

You can see the build status in the "Actions" tab of your GitHub repository.

## Version Numbering

Use semantic versioning:
- `v1.0.0` - Major release (breaking changes)
- `v1.1.0` - Minor release (new features)
- `v1.0.1` - Patch release (bug fixes)

## Troubleshooting

If a release fails to build:
1. Check the "Actions" tab in your GitHub repository
2. Look at the failed workflow logs
3. Fix any build errors and create a new tag

## File Sizes

Self-contained executables will be approximately 80-120MB because they include the .NET runtime. This ensures users can run SkiaLizer without installing anything separately.
