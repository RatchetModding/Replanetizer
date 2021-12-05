<!-- LOGO -->
<h1>
<p align="center">
  <img src="media/ReplanetizerIcon500px.ico" alt="Logo" width="110" height="110" title="Logo made by Nooga.">
  <p align="center" style="font-weight: bold">Replanetizer</p>
</h1>
  <p align="center">
    Level Editor for the Ratchet and Clank Collection on PS3
    <br />
    </p>
</p>
<p align="center">
  <a href="#features">Features</a> •
  <a href="#usage">Usage</a> •
  <a href="#building">Building</a> •
  <a href="#technology">Technology</a> •
  <a href="#licensing">Licensing</a> •
  <a href="CONTRIBUTING.md">Contributing</a>
</p>

<p align="center">
  <img src="media/preview.gif" alt="Preview">
</p>

# Features

Replanetizer is still in an early state of development. Its feature set entails:

 - Visualization
   - Levelobjects
   - Collision
   - Splines
   - Triggers
 - Editing
   - Transformation of objects
   - Manipulation of variables
 - Export
   - Textures (`.png`)
   - Rigged models (`.dae`, `.obj`, `.iqe`)
   - Level as a model (`.obj`)
   - Collision (`.obj`)

Support for Ratchet Deadlocked is available but low priority.

# Usage

Extract a `.psarc` file from a game using seperate tools (PSArcTool.exe for example).
Use File -> Open, and select a `engine.ps3` file inside a level folder. It is recommended to keep the folder structure present in the `.psarc` file.

To apply changes to the game, you need to either repack the files into a `.psarc` file or use an incomplete `.psarc` file. Any file missing in a `.psarc` file will be loaded from the correspondent location outside the `.psarc` file.

You can move around the world with keyboard and mouse:

 - WASD to move camera inside the world. Hold shift to speed up movement
 - Left click items to select them
 - Rotate the camera by holding the right mouse button down and moving your mouse

# Building

Dependencies:

 - .NET 5 tooling (dotnet host, runtime, sdk and targeting pack)
 - For Linux (and maybe other Unixes):
   - Zenity (or KDialog if you prefer KDE dialogs)
   - Basic OpenGL dependencies (most of these will be installed if you run any form of GUI).
 - Refer to the READMEs of our NuGet       dependencies for more information if you get stuck.

To get started, clone this repository, and execute the following command:

 - `dotnet run -p Replanzetier`

Alternatively, open Replanetizer.sln in your favourite IDE, and use its tooling instead.

# Technology

The project is written in C#, and uses OpenTK4 for rendering. C# bindings for Dear ImGui are used for the UI.

We currently target .NET 5 (net5.0-windows for the Windows builds, net5.0 on anything else).

OpenGL 3.3 is used for the rendering.

# Licensing

Replanetizer is Copyright (C) 2018-2022, The Replanetizer Contributors.

Replanetizer is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
Please see the [LICENSE.md](LICENSE.md) file for more details.

