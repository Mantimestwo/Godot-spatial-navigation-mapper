@tool
extends Resource
class_name NavMapVisualOverride

@export_range(0.0,100.0,0.1,"suffix:percent") var opacity : float = 100.0
@export var up_color : Color = Color(0.89, 0.34, 0.18, 1.00)
@export var down_color : Color = Color(0.00, 0.62, 1.00, 1.00)
@export var left_color : Color = Color(0.67, 0.85, 0.36, 1.00)
@export var right_color : Color = Color(0.73, 0.22, 0.60, 1.00)

@export var group_container_color : Color = Color(1, 0.765, 0.0, 1)

@export var group_up_color : Color = Color(0.96, 0.15, 0.08, 1.00)
@export var group_down_color : Color = Color(0.00, 0.30, 1.00, 1.00)
@export var group_left_color : Color = Color(0.39, 0.93, 0.59, 1.00)
@export var group_right_color : Color = Color(0.98, 0.13, 0.4, 1.00)
@export var GroupBoxPadding : int = 6:
	get:
		return group_box_padding
	set(value):
		value = clamp(value,-5,999)
		group_box_padding = value
var group_box_padding : int = 6
@export var group_label_size : float = 0.5
@export var container_tex : Texture2D = load("res://addons/Spatial navigation mapper/ContainerBorder.png")
