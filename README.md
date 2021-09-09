# FileDeploy
Best IDE ever

FileDeploy helps developers to write faster by creating template txts and files, which are later used to be processed then it will save the result to the ClipBoard.
FileDeploy allows user to create their own template and parameters to enable best customization.

To Use FileDeploy:

1. Find the 'Template' Folder in FileDeploy.exe
2. Add a new folder as a Template. The name of this folder will be used as Template Name, and display on the UI
3. Add '__copy.txt' to the new folder. Everything written in this file will be copied to the ClipBoard later
(If you want to use parameters)
4. Create '__main.lua' to the new folder.
5. This file should contain a global lua table called Params, all parameters listed here will be display on the UI.
6. This file can also contain a global lua function called Main, every parameters
