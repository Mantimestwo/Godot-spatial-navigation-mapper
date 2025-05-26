# Automatic spatial navigation mapper for Godot

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

Due to current limitations of the Godot editor, the plugin cannot automatically toggle on ‘Group selected nodes’ and ‘Lock node’ to make the generated visualizers non-intrusive. Thus for the best experience, they are recommended to manually be toggled on for the visualization grandparent node, which is automatically generated.

![image](https://github.com/user-attachments/assets/d792a817-1077-4600-adc7-9d4ac50f6782)


## License

This project is licensed under an MIT license.

See LICENSE for further detail.


## FAQ

#### Q: Why?

The current built-in automatic navigation mapping in Godot has no debug visualization, nor any way to access the data for the chosen navigation targets, as far as I know at least. I had a potential want and a need for both of those for some custom UI work, hence I chose to develop this tool as a project for my bachelor's thesis.

During my few years in uni, I was also "The UI-guy" for a few game projects I was a part of. I'm well aware of how annoying UI can be to work with, thus I want to make it a little less so.

#### Q: Will this be updated any further?

My plan is to at least clean up some of the code and potentially add some features down the line. At the bare minimum, I will rework how the custom navigation resources are listed, as a single array with all elements at once may be extremely annoying to read.

Some other minor additions may be added down the line, depending on solutions I come up with for any encountered need. Of course, if you have a need for some very specific addition, the code is licensed under an MIT license, so you are completely free to edit the code to make your own version of the plugin, within the rights detailed within the license document!

#### Q: Why is the code basically held together with duct tape and string?

Because it is :)

## 🔗 Links
[Twitter](https://x.com/Mantimestwo)
[Youtube](https://www.youtube.com/channel/UCVOaL_jrj6RhFPgmmfbdmog)

## Details on Navmapper node

Info about the navmapper node and variables within (in order)

- ***Alt and (key) to hide***: input key for visibility toggle hotkey combination (Alt + [key])

- ***Lock visibility toggle***: Lock for whether it's possible to use visibility hotkey

- ***Auto-update***:
	- ***All***: Calculate automatic navigation and set visualization
	- ***Visualization***: Only update visualization, used for manually updating pathing via navigation reference resources
(Note: if new elements are added, "All" auto-update is required to generate and properly re-create navigation resources, groups and other)

- ***Set base godot focus***: (add set self to all to stop all navigation)
	- Toggles whether the tool sets built-in Godot focus references and toggles whether built in focus is in use at all. (If needed for any other custom implementations where data is read from navigation references for custom focus tools etc)
   
- ***Allow base godot next and previous***: 
  - Controls whether next/previous focus references are filled with self or left empty
  - Off by default, as I have no custom logic and dont want these to break any UI calculated by the tool

- ***Override frame focus***: Smoother visual update, but computes a mapping update every tick in _physics_process() (60hz) instead of every other tick (30hz)

- ***Element groups***: The collection of groups of elements to be mapped
	- ***Elements to map***: elements for a group
	- ***Entry selection indexes***: for controlling which index in the array gets selected, when moving from another group to the corresponding one (Right input = left entry, Up input = bottom entry)
		- defaults to first element in group

- ***Check resolution***: Amount of "detection" points alongside the width and height of all elements for added accuracy
	- (higher amount means more calculations, hence a count over something like 15 would probably be unnecessary)

- ***Maximum target angle***: Maximum allowed angle deviation compared to input direction
- ***Maximum group target angle***: Ditto, but for group-to-group detection

- ***Navigation references***: Custom resources for bringing all focus references to one place
	- only manually editable in "Visualization" auto-update mode
	- In-group navigation is fully editable
	- Can only move to other groups in the calculated group-to-group directions, controlled by entry indexes found in the element group resources
	- A Null-target sets in-built focus to self, so no navigation can occur in the direction

- ***Distance cut-off***: -1 = no limit, but you can input any maximum distance where results get discarded up till 4000 units (pixels), if more is necessary, you can go into the code and increase the limit

- ***Visuals***:
  - Override: Saveable custom Godot resource with all settings below
    - Can easily be set in any new Nav-mapper instance for transferring customization
  - Overall opacity
  - Colors for each direction:
    - In-group
    - Group-to-group
  - Group container color
  - Group container padding (size)
  - Group label size
  - Group container texture (Currently hard-coded margin of 6 pixels on each edge, but you can use any custom texture)

