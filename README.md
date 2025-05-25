# Godot spatial navigation mapper

A godot tool plugin for automatically controlling and mapping out spatial-navigation pathing for interface elements, originally created as a part of my bachelor's thesis during spring 2025.

![presentation1](https://github.com/user-attachments/assets/84d53d6e-716d-4a64-8793-af446c3b6ca1)

# Features

- Real time automatic element-to-element and Group-to-group calculations for focus references
- Manually adjustable pathing post-calculation by toggling on 'visualization' auto-update  and editing 'navigation reference' resources 
- Automated visualization for determined pathing and groups (Including visibility toggle + full customizable color palette with a saveable resource for easy reuse)
- Currently tested with Godot UI elements like buttons, but some other elements may not function perfectly with the current version

## Installation

To access the code and install it within Godot, either clone the repository or download the files and move the unzipped (which ever version of the two you need) ‘Spatial navigation mapper’ folder into the ‘addons’ folder within your choice of a Godot-project. For C#, to make the addon visible and accessible within the project, build or run your game once to compile the code and toggle it on via project > project settings > plugins using the checkbox. For the native GDscript version, you should be able to simply toggle it on once the addon folder is in place.

Once the plugin is active, to add a mapping node add a new child node and search for 'Navigation mapper' within the node selection menu. You can then add UI control nodes to as many 'Navigation groups' as you want, and the tool will handle the rest. Elements within a group will only primarily map to each other, groups will interact with other groups.

Due to current limitations of the Godot editor, the plugin cannot automatically toggle on ‘Group selected nodes’ and ‘Lock node’ to make the generated visualizers non-intrusive. Thus for the best experience, they are recommended to manually be manually toggled on for the visualization parent node, which is automatically generated when any to-map controls are added for the first time.

## License

[MIT](https://choosealicense.com/licenses/mit/)


## FAQ

#### Q: Why?

Godot's current built-in automatic navigation mapping has no debug visualization, nor any way to access the data for the chosen navigation targets, as far as I know at least. I had a potential want and a need for both of those for some custom UI work, hence I chose to develop this tool as a project for my bachelor's thesis.

During the few years in uni, I was also "The UI-guy" for a few game projects I took part in. I'm well aware of how annoying UI can be to work with, thus I want to make it a little less so.

#### Q: Will this be updated any further?

My plan is to at least clean up some of the code and potentially add some features down the line. At the bare minimum, I will rework how the custom navigation resources are listed, as a single array with all elements at once may be extremely annoying to read.

Some other minor additions may be added down the line, depending on solutions I come up with for any encountered need. Of course, if you have a need for some very specific addition, the code is licensed under a MIT license, so you are completely free to edit the code to make your own version of the plugin!


## 🔗 Links
[Twitter](https://x.com/Mantimestwo)
[Youtube](https://www.youtube.com/channel/UCVOaL_jrj6RhFPgmmfbdmog)

