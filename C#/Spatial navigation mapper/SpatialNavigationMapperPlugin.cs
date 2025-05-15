#if TOOLS
using Godot;
using System;

[Tool]
public partial class SpatialNavigationMapperPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		AddCustomType("Navigation mapper", "Node", GD.Load<CSharpScript>("res://addons/Spatial navigation mapper/NavMapper.cs"), 
            GD.Load<Texture2D>("res://addons/Spatial navigation mapper/NavMapperIcon.png"));
		AddCustomType("Nav-Pointer", "NinePatchRect", GD.Load<CSharpScript>("res://addons/Spatial navigation mapper/SpatNavPointer.cs"),
			GD.Load<Texture2D>("res://addons/Spatial navigation mapper/PointerIcon.png")); 
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveCustomType("Navigation mapper");
		RemoveCustomType("Nav-Pointer");
	}
}
#endif
