# Replanetizer

This project is a Level Editor for Ratchet &amp; Clank games on the PS3.

# Technology

The project is a WinForms project written in C#. Visual Studio is used for development.
All commits pushed to the master branch should be working builds of the application.
Any ongoing work MUST be pushed to a separate branch. e.g. While you're developing new features.

# Usage guide

Extract a .psarc file from a game using seperate tools (PSArcTool.exe for example).
Use File -> Open, and select a `engine.ps3` file inside a level folder.

You can move around the world with keyboard and mouse:

 - WASD to move camera inside the world. Hold shift to speed up movement
 - Left click items to select them
 - Rotate the camera by holding the right mouse button down and moving your mouse

## Running on non-Windows platforms

Not all parts of Replanetizer work on non-Windows platform, notably some of the UI functions do not.

To build and run Replanetizer, you need both Mono and .NET core installed.

To get started, clone this repository, and execute the following commands:

 - `dotnet build`
 - `cd Replanetizer`
 - `mono bin/Debug/net472/RatchetEdit.exe`

When we get all dependencies and WinForms running on .NET core, the dependency on Mono can hopefully be dropped.


# Legal stuff

Replanetizer is Copyright (C) 2018-2020, The Replanetizer Contributors.

Replanetizer is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version. 
Please see the [LICENSE.md](LICENSE.md) file for more details.

Replanetizer includes icons from the materials icon pack located at https://material.io/tools/icons/?style=baseline, 
which is protected under the apache 2.0 license availible at https://www.apache.org/licenses/LICENSE-2.0.html

