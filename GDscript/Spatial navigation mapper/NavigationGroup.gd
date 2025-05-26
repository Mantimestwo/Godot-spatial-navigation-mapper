@tool
extends Resource
class_name NavigationGroup

var parent_mapper : Node

@export var ElementsToMap : Array[NodePath]:
	get:
		return elements_to_map
	set(value):
		elements_to_map = value
		CheckEntryIndexLimits()
var elements_to_map : Array[NodePath]

@export_group("Entry selection indexes")
@export var TopEntrySelectionIndex : int:
	get:
		return top_entry_selection_index
	set(value):
		top_entry_selection_index = value
		CheckEntryIndexLimits()
var top_entry_selection_index : int
@export var LeftEntrySelectionIndex : int:
	get:
		return left_entry_selection_index
	set(value):
		left_entry_selection_index = value
		CheckEntryIndexLimits()
var left_entry_selection_index : int
@export var RightEntrySelectionIndex : int:
	get:
		return right_entry_selection_index
	set(value):
		right_entry_selection_index = value
		CheckEntryIndexLimits()
var right_entry_selection_index : int
@export var BottomEntrySelectionIndex : int:
	get:
		return bottom_entry_selection_index
	set(value):
		bottom_entry_selection_index = value
		CheckEntryIndexLimits()
var bottom_entry_selection_index : int

func CheckEntryIndexLimits():
	top_entry_selection_index = clamp(top_entry_selection_index, 0, elements_to_map.size()-1)
	left_entry_selection_index = clamp(left_entry_selection_index, 0, elements_to_map.size()-1)
	right_entry_selection_index = clamp(right_entry_selection_index, 0, elements_to_map.size()-1)
	bottom_entry_selection_index = clamp(bottom_entry_selection_index, 0, elements_to_map.size()-1)
	if parent_mapper != null:
		parent_mapper.QueueMap()
	pass
