# UI Mockup Reference

A self-contained Unity Editor package for keeping UI mockups visible while you build a scene or Canvas. It has no runtime scripts or third-party dependencies.

## Install in another project

Copy this entire `com.mockupreference.editor` directory into that project's `Packages` directory. Unity 2022.3 or newer will discover it as an embedded package. Keep the `.meta` files with the package.

## Use

Open **Tools > UI Mockup Reference** and dock the window beside the Scene or Game view.

- Assign a Texture2D or Sprite to **Image** to add it.
- Multi-select images in the Project window and click **Add Selection** to add them together.
- Assign a Project folder to **Folder** and click **Load Folder** to replace the list with images in that folder and its subfolders.
- Use **Previous** and **Next** to browse, **Fit** or **100%** to set the scale, and the zoom slider for a custom scale.
- **Ping** locates the current image in the Project window. **Clear** removes the current list.

The image list is remembered separately for each Unity project on the same computer.
