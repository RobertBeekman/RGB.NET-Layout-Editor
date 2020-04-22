
# RGB.NET-Layout-Editor

[![Build status](https://build.rbeekman.nl/app/rest/builds/buildType:(id:RgbNetLayoutEditor_Build)/statusIcon.svg)](https://build.rbeekman.nl/viewType.html?buildTypeId=RgbNetLayoutEditor_Build&guest=1)

An editor for [RGB.NET](https://github.com/DarthAffe/RGB.NET) layouts. Not extremely user friendly so feel free to ask for help, I'll add a wiki soon-ish..

## Features

 - Supports the complete RGB.NET device layout specification
 - Uses a root directory to ensure the relative image paths are correctly stored
 - Direct visualisation of the layout, uses RGB.NET under the hood to ensure accurate results
 - Mouse pan & zoom
 - LED dragging (hold shift and move the selected LED, will make your LED use absolute positioning)
 - Custom shape editing*
 

Example layouts can be found at https://github.com/DarthAffe/RGB.NET-Resources  
Executable of latest build: [RGB.NET_Layout_Editor_Build.zip]( https://build.rbeekman.nl/repository/downloadAll/RgbNetLayoutEditor_Build/.lastSuccessful/artifacts.zip?guest=1)  
To build yourself, clone and run  ```Update-Package -reinstall PropertyChanged.Fody``` on the project

*\*To create multiple paths on custom shapes, stop and start shape editing each time you want a new path.*

## Screenshots
![Editing a keyboard](https://i.imgur.com/BYn8HW8.png)
_Editing a keyboard_  

![Editing a mouse](https://i.imgur.com/haDmrQW.png)
_Editing a mouse_  

![Creating custom LED shapes](https://i.imgur.com/iTbvHA2.png)
_Creating a custom shape_  
