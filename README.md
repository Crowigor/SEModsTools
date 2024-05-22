# SEModsTools

Visual Studio 2022 Space Engineers Mods Tools

[![Support](https://img.shields.io/github/issues-raw/Crowigor/SEModsTools?style=for-the-badge&logoWidth=20&label=support)](https://github.com/Crowigor/SEModsTools/issues)
[![Download](https://img.shields.io/github/release/Crowigor/SEModsTools.svg?style=for-the-badge&colorA=555&colorB=1e87f0&label=download)](https://github.com/Crowigor/SEModsTools/releases/latest)

## Main Features

Tools consist of two parts: a template with settings for new projects using Setup Wizard, and the tools themselves.

### Tool Features

#### Push Files

Push files into the local mod folder.
Only push files added to the project and only of certain extensions (can be changed in Properties).
This allows you to store the project and the mod module in different locations and prevents service files from being
included in the mod and on Steam upon publication.
There are 3 upload options:

1. Push - standard file push with overwrite.
2. Force Push - before pushing, the mod folder is deleted, then files are uploaded.
3. Automatic Upload - if enabled in properties, a watcher tracks file changes (specifically changes, meaning the file
   must be saved), and if a file has been changed, it uploads it to the mod folder.

### Creating a New Project

1. After installing the extension, select SEModTemplate from the project creation menu.
   ![create_project_list](/Screenshots/create_project_list.jpg)
2. Fill out the form as usual and click `Create`.
   ![create_project_defualt](/Screenshots/create_project_defualt.jpg)
3. In the window that appears, fill out the form.
   ![create_project_window](/Screenshots/create_project_window.jpg)
    * Mod Name - The name of your mod, will be used in the path when uploading files, and also when creating the basic
      file structure.
    * Namespace - The main namespace of the project.
    * Game Bin Path - The path to the Bin64 folder of Space Engineers,
      usually `_PATH_TO_STEM_\Steam\steamapps\common\SpaceEngineers\Bin64` (there is a Browse for convenience). Used to
      connect the game's source files.
    * Mods Folder - The folder where local SpaceEngineers mods are stored `%AppData%\SpaceEngineers\Mods`. It can remain
      unchanged if everything is working as standard.
    * Allowed Extensions - Allowed file extensions for uploading, separated by commas (extension should start with a
      dot).
    * Automatic Upload - If enabled, a Watcher will monitor file changes in the project and upload as needed.
4. After filling out the form, click `Apply`.

If everything is successful, you will see the initialization of the project in the Output SEModsTools. (All tool actions
will also be displayed in this window.)
![project](/Screenshots/project.jpg)

### Changing properties / Connecting to an existing project

1. In Solution Manager, in the project's context menu (right-click on the project, not the solution), select
   SEModsTools > Properties.
   ![project_menu](/Screenshots/project_menu.jpg)
2. In the window that appears, change/fill in the required fields.
   ![project_properties](/Screenshots/project_properties.jpg)
3. After filling out, click `Apply`.

If you are connecting SEModsTools to an existing project that did not use it before, restart VS after saving.