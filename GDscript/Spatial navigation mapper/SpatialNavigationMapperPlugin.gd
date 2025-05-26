@tool
extends EditorPlugin


func _enter_tree():
	# Initialization of the plugin goes here.
	add_custom_type("Navigation mapper", "Node", load("res://addons/Spatial navigation mapper/NavMapper.gd"),load("res://addons/Spatial navigation mapper/NavMapperIcon.png"))
	add_custom_type("Nav-pointer", "NinePatchRect", load("res://addons/Spatial navigation mapper/SpatNavPointer.gd"),load("res://addons/Spatial navigation mapper/PointerIcon.png"))
	pass


func _exit_tree():
	# Clean-up of the plugin goes here.
	remove_custom_type("Navigation mapper")
	remove_custom_type("Nav-pointer")
	pass
