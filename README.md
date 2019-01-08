# Replanetizer

This project is a Level Editor for Ratchet &amp; Clank games on the PS3.

# Technology

The project is a WPF project written in C#. Visual Studio is used for development.
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
