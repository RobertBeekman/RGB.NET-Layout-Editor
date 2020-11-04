
# RGB.NET-Layout-Editor

[![Build Status](https://dev.azure.com/artemis-rgb/Artemis/_apis/build/status/RGB.NET%20Layout%20Editor%20build?branchName=master)](https://dev.azure.com/artemis-rgb/Artemis/_build/latest?definitionId=5&branchName=master)

An editor for [RGB.NET](https://github.com/DarthAffe/RGB.NET) layouts.  
Please note that this was created to save some time creating layouts before I started working on [Artemis 2](https://github.com/Artemis-RGB/Artemis). It's far from perfect and not extremely user friendly, but it's better than editing XML by hand.

I'm happy to fix bugs though and add small improvements, feel free to create an issue or reach out on the [Artemis Discord](https://discord.gg/S3MVaC9).

## Features
 - Supports the complete RGB.NET device layout specification
 - Uses a root directory to ensure the relative image paths are correctly stored
 - Direct visualisation of the layout, uses RGB.NET under the hood to ensure accurate results
 - Mouse pan & zoom
 - LED dragging (hold shift and move the selected LED, will make your LED use absolute positioning)
 - Custom shape editing*
 

Example layouts can be found at https://github.com/DarthAffe/RGB.NET-Resources  

To build yourself, clone and run  ```Update-Package -reinstall PropertyChanged.Fody``` on the project

*\*To create multiple paths on custom shapes, stop and start shape editing each time you want a new path.*

## Screenshots
![Editing a keyboard](https://i.imgur.com/BYn8HW8.png)
_Editing a keyboard_  

![Editing a mouse](https://i.imgur.com/haDmrQW.png)
_Editing a mouse_  

![Creating custom LED shapes](https://i.imgur.com/iTbvHA2.png)
_Creating a custom shape_  
