# Godot spatial navigation mapper
A  godot tool plugin for automatically controlling and mapping out spatial-navigation pathing for interface elements, originally created as a part of my bachelor's thesis during spring 2025
![image](https://github.com/user-attachments/assets/65da3830-34fe-46b1-b5a0-0f2060df54c9)

To install and use the thesis-release of the plugin, at least a Godot version 4.4 .Net installation is required to allow any support for C# code.

To access the code and install it within Godot, either clone the repository or download the files and move the unzipped C# ‘pathmapper’ folder into the ‘addons’ folder within your choice of a Godot-project. To make the addon visible and accessible within the project, build or run your game once to compile the code andtoggle it on via project > project settings > plugins using the checkbox.

Once the plugin is active, to add a mapping node add a new child node and search for 'Navigation mapper' within the node selection menu. You can then add UI control nodes to as many 'Navigation groups' as you want, and the tool will handle the rest. Elements within a group will only primarily map to each other, and then groups will interact with other groups.

Due to current limitations of the Godot editor, the plugin cannot automatically toggle on ‘Group selected nodes’ and ‘Lock node’ to make the generated visualizers non-intrusive. Thus for the best experience, they are recommended to manually be manually toggled on for the visualization parent node, which is automatically generated when any to-map controls are added for the first time.
