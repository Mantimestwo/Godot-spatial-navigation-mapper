@tool
extends NinePatchRect

class_name SpatNavPointer

var direction_index : int = -1

func _enter_tree():
	if(get_child_count() < 1):
		var pointerLabel : Label = Label.new()
		add_child(pointerLabel)
		pointerLabel.owner = get_tree().edited_scene_root

	texture = load("res://addons/Spatial navigation mapper/arrow.png")
	region_rect = Rect2(0, 0, 21.0, 16.0)
	patch_margin_top = 11.0

func SetPointerParameters(receiving_direction_index : int, direction_vector : Vector2, distance : float, colors : Array[Color], letters : Array[String]):
	rotation_degrees = rad_to_deg(atan2(direction_vector.y, direction_vector.x)) - 90.0
	size = Vector2(texture.get_size().x, distance+20.0)
	direction_index = receiving_direction_index
	modulate = colors[direction_index]

	var dir_label : Label = get_child(0)
	dir_label.text = letters[direction_index]
	dir_label.rotation_degrees = -rotation_degrees
	dir_label.global_position = global_position + (direction_vector*distance/2.0) - Vector2(-direction_vector.y, direction_vector.x)*11.0 - dir_label.size/2.0
	pass

func SetPointerColor(colors : Array[Color]):
	modulate = colors[direction_index]
