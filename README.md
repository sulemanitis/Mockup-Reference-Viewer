# UI Mockup Reference

A lightweight, dockable Unity Editor window for keeping UI mockups visible while you build a scene or Canvas.

- View `Texture2D` and `Sprite` references without leaving the Editor.
- Add one image or batch-add selected images from the Project window.
- Load images from a Project folder and its subfolders.
- Browse images, fit them to the window, view them at 100%, or adjust zoom.
- Ping the current image in the Project window.
- Remember the image list separately for each Unity project on the same computer.

This is an Editor-only tool. It adds no runtime scripts and has no third-party dependencies.

## Requirements

- Unity **2022.3 or newer**.
- Images imported into the Unity project.

The minimum version is declared in `package.json`. Compatibility with other Unity versions should be verified in the target Editor.

## Installation

### Option 1: Add from Git URL

1. Open **Window > Package Management > Package Manager** (or open Package Manager from the **Window** menu, depending on your Unity version).
2. Click **+** and choose **Add package from git URL...**.
3. Enter:

   `https://github.com/sulemanitis/Mockup-Reference-Viewer.git`

4. Click **Add**.

This installs the package from the repository's default branch. For a reproducible setup, use a tagged release once one is published.

### Option 2: Install as an embedded package

1. Download or clone this repository.
2. Copy the package contents, including `package.json`, `Editor/`, and their `.meta` files, into your project's `Packages/com.mockupreference.editor` directory.
3. Let Unity refresh the project.

Keep the `.meta` files with the package.

## Usage

1. Open **Tools > UI Mockup Reference**.
2. Dock the window beside the Scene or Game view.
3. Add references using any of the following methods:
   - Assign a `Texture2D` or `Sprite` to **Image**.
   - Select multiple images in the Project window and click **Add Selection**.
   - Assign a Project folder to **Folder** and click **Load Folder** to replace the list with images found in that folder and its subfolders.
4. Use **Previous** and **Next** to browse the list.
5. Use **Fit**, **100%**, or the zoom slider to adjust the displayed scale.
6. Click **Ping** to locate the current image in the Project window, or **Clear** to remove the current list.

The image list is remembered separately for each Unity project on the same computer.

## Troubleshooting

- **The menu item is missing:** Confirm the package is installed and that Unity has finished compiling scripts in the Editor folder. Check the Console for compilation errors.
- **The package is not detected:** Confirm `package.json` is at the package root and that the package is under `Packages/` when using the embedded-package method.
- **An image is missing from the list:** Confirm it is an image asset imported into the project and try adding it directly through **Image** or the Project selection workflow.

## Bug reports and feature requests

Please open an issue in the [GitHub repository](https://github.com/sulemanitis/Mockup-Reference-Viewer/issues). Include your Unity version, reproduction steps, and any relevant Console errors.

## Version history

See [CHANGELOG.md](CHANGELOG.md) for documented changes.

## License

This project is distributed under the [MIT License](LICENSE).
