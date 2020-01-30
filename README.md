# Replanetizer

This project is a Level Editor for Ratchet &amp; Clank games on the PS3.

# Technology

The project is a WinForms project written in C#. Visual Studio is used for development.
All commits pushed to the master branch should be working builds of the application.
Any ongoing work MUST be pushed to a separate branch. e.g. While you're developing new features.

# Task management

All tasks are available at asana. Contact simenfjellstad or stiantoften to be added to the board.

# Git...
No commits should be pushed directly to master.

You should create your own branch for any new feature you create.

## CLI commands for creating a branch:
(Assuming remote = origin)

1. git branch `branch-name`
2. git checkout `branch-name`
3. git add -A
4. git commit -am "My commit message"
5. git push origin `branch-name`

## Merging branches:
All branch merges should be done in their respective branches and **NOT master**  
(Assuming remote = origin)

1. git pull origin master
2. Using a text editor, resolve the conflicts marked by a "<<<<<<<" line
3. Add and commit fixed files.
4. Push merge commit to `branch-name`
5. git checkout master
6. git merge `branch-name`


# Legal stuff

Replanetizer is Copyright (C) 2018-2020, The Replanetizer Contributors.

Replanetizer is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version. 
Please see the [LICENSE.md](LICENSE.md) file for more details.

Replanetizer includes icons from the materials icon pack located at https://material.io/tools/icons/?style=baseline, 
which is protected under the apache 2.0 license availible at https://www.apache.org/licenses/LICENSE-2.0.html

