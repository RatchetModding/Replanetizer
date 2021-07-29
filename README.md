# Replanetizer

Replanetizer is a Level Editor for Ratchet &amp; Clank games on the PS3.

# Technology

The project is written in C#, and uses OpenTK4 for rendering. C# bindings for Dear ImGui are used for the UI.
We currently target .NET 5 (net5.0-windows for the Windows builds, net5.0 on anything else)

All commits pushed to the master branch should be working builds of the application.
Any ongoing work MUST be pushed to a separate branch. e.g. While you're developing new features.

# Usage guide

Extract a .psarc file from a game using seperate tools (PSArcTool.exe for example).
Use File -> Open, and select a `engine.ps3` file inside a level folder.

You can move around the world with keyboard and mouse:

 - WASD to move camera inside the world. Hold shift to speed up movement
 - Left click items to select them
 - Rotate the camera by holding the right mouse button down and moving your mouse

## Running the project from code instead of pre-builts.

Dependencies:

 - .NET 5 tooling (dotnet host, runtime, sdk and targeting pack)
 - Zenity (or KDialog if you prefer KDE dialogs)
 - Basic OpenGL dependencies (most of these will be installed if you run any form of GUI) 
   Refer to the READMEs of our NuGet dependencies for more information if you get stuck.


To get started, clone this repository, and execute the following command:

 - `dotnet run -p Replanzetier`


# Legal stuff

Replanetizer is Copyright (C) 2018-2020, The Replanetizer Contributors.

Replanetizer is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version. 
Please see the [LICENSE.md](LICENSE.md) file for more details.

Replanetizer includes icons from the materials icon pack located at https://material.io/tools/icons/?style=baseline, 
which is protected under the apache 2.0 license availible at https://www.apache.org/licenses/LICENSE-2.0.html

