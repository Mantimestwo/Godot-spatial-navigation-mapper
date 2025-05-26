@tool
extends Node

@export_tool_button("Map navigation", "CanvasItem") var ClickMeButton = MapElements

var mapping_queued : bool = false
var is_ready_to_map : bool = false
var debug : bool = false
@export var alt_and_hotkey_to_hide : Key = KEY_H
@export var lock_visibility_toggle : bool = false
var visibility_input_lock : bool = false
var empty_warning_printed : bool = false
enum AutoUpdateTypes {All, Visualization}
@export var auto_update : AutoUpdateTypes = AutoUpdateTypes.All
@export var SetBaseGodotFocus : bool = true:
	get: return set_base_godot_focus
	set(value):
		set_base_godot_focus = value
		if is_ready_to_map:
			mapping_queued = true;
var set_base_godot_focus : bool = true
@export var AllowBaseGodotNextAndPrevious : bool = false:
	get: return allow_base_godot_next_and_previous
	set(value):
		allow_base_godot_next_and_previous = value
		if is_ready_to_map:
			mapping_queued = true;
var allow_base_godot_next_and_previous : bool = false
@export var override_frame_limit : bool = false
@export_category("Parameters")
@export var Element_groups : Array[NavigationGroup]:
	get:
		return element_groups
	set(value):
		var is_changed : bool = element_groups != value
		element_groups = value
		for i in element_groups.size():
			if element_groups[i] != null:
				element_groups[i].parent_mapper = self
		if is_ready_to_map:
			mapping_queued = true
var element_groups : Array[NavigationGroup]

@export var CheckResoulution : int = 7:
	get:
		return check_resolution
	set(value):
		var is_changed = check_resolution != value
		value = clamp(value, 0, 9999)
		check_resolution = value
		if is_ready_to_map:
			mapping_queued = is_changed
var check_resolution : int = 7

@export_range(0,90,0.1,"suffix:degrees") var MaximumTargetAngle : float = 30.0:
	get:
		return maximum_target_angle
	set(value):
		var is_changed = maximum_target_angle != value
		maximum_target_angle = value
		if is_ready_to_map:
			mapping_queued = is_changed
var maximum_target_angle : float = 30.0

@export_range(0,90,0.1,"suffix:degrees") var MaximumGroupTargetAngle : float  = 30.0:
	get:
		return maximum_group_target_angle
	set(value):
		var is_changed = maximum_group_target_angle != value
		maximum_group_target_angle = value
		if is_ready_to_map:
			mapping_queued = is_changed
var maximum_group_target_angle : float = 30.0



var current_selected : Control
var current_selected_group_container : Control
var current_highest_weight : float
var pointer_parent : Control
var current_pointer_index : int = 0
var pre_existing_pointer_counts : int
var total_connection_count : int = 0
var pointer_difference : int = 0
var container_parent : Control
var visualization_grandparent : Control
var group_pointer_parent : Control

@export var navigation_references : Array[NavigationReference]

@export_range(-1.0,4000,2.0,"suffix:units") var DistanceCutOff : float = -1.0:
	get:
		return distance_cut_off
	set(value):
		var is_changed = distance_cut_off != value
		distance_cut_off = value
		if is_ready_to_map:
			mapping_queued = is_changed
var distance_cut_off : float = -1.0

@export_group("Visuals")
@export var VisualOverride : NavMapVisualOverride:
	get:
		return visual_override
	set(value):
		visual_override = value
		CheckSetArrowColors(0)
		CheckSetArrowColors(1)
		CheckSetGroupContainerColorAndTex()
		UpdateGroupLabelSizes()
		UpdateVisualOpacity()
		if visual_override != null:
			UpdateGroupPadding(visual_override.group_box_padding)
		else:
			UpdateGroupPadding(6)
var visual_override : NavMapVisualOverride

@export_range(0,100.0,0.1,"suffix:percent") var Opacity : float = 100.0:
	get:
		return opacity
	set(value):
		opacity = value
		UpdateVisualOpacity()
var opacity : float = 100.0

@export var UpColor : Color = Color(0.89, 0.34, 0.18, 1.00):
	get:
		return up_color
	set(value):
		up_color = value
		CheckSetArrowColors(0)
var up_color : Color = Color(0.89, 0.34, 0.18, 1.00)

@export var DownColor : Color = Color(0.00, 0.62, 1.00, 1.00):
	get:
		return down_color
	set(value):
		down_color = value
		CheckSetArrowColors(0)
var down_color : Color = Color(0.00, 0.62, 1.00, 1.00)

@export var LeftColor : Color = Color(0.67, 0.85, 0.36, 1.00):
	get:
		return left_color
	set(value):
		left_color = value
		CheckSetArrowColors(0)
var left_color : Color = Color(0.67, 0.85, 0.36, 1.00)

@export var RightColor : Color = Color(0.73, 0.22, 0.60, 1.00):
	get:
		return right_color
	set(value):
		right_color = value
		CheckSetArrowColors(0)
var right_color : Color = Color(0.73, 0.22, 0.60, 1.00)

@export var GroupContainerColor : Color = Color(1, 0.765, 0.0, 1):
	get:
		return group_container_color
	set(value):
		group_container_color = value
		CheckSetGroupContainerColorAndTex()
var group_container_color : Color = Color(1, 0.765, 0.0, 1)

@export var GroupUpColor : Color = Color(0.96, 0.15, 0.08, 1.00):
	get:
		return group_up_color
	set(value):
		group_up_color = value
		CheckSetArrowColors(1)
var group_up_color : Color = Color(0.96, 0.15, 0.08, 1.00)

@export var GroupDownColor : Color = Color(0.00, 0.30, 1.00, 1.00):
	get:
		return group_down_color
	set(value):
		group_down_color = value
		CheckSetArrowColors(1)
var group_down_color : Color = Color(0.00, 0.30, 1.00, 1.00)

@export var GroupLeftColor : Color = Color(0.39, 0.93, 0.59, 1.00):
	get:
		return group_left_color
	set(value):
		group_left_color = value
		CheckSetArrowColors(1)
var group_left_color : Color = Color(0.39, 0.93, 0.59, 1.00)

@export var GroupRightColor : Color = Color(0.98, 0.13, 0.4, 1.00):
	get:
		return group_right_color
	set(value):
		group_right_color = value
		CheckSetArrowColors(1)
var group_right_color : Color = Color(0.98, 0.13, 0.4, 1.00)

@export var GroupBoxPadding : int = 6:
	get:
		return group_box_padding
	set(value):
		value = clamp(value,-5,999)
		if visual_override == null:
			UpdateGroupPadding(value)

var group_box_padding : int = 6

@export var GroupLabelSize : float = 0.5:
	get:
		return group_label_size
	set(value):
		group_label_size = value
		UpdateGroupLabelSizes()
var group_label_size : float = 0.5

var pointer_letters : Array[String] = ["U","D","L","R"]
var directions : Array[Vector2] = [Vector2.LEFT, Vector2.UP, Vector2.RIGHT, Vector2.DOWN]
var angles : Array[float]
var finals : Array[float]
var distances : Array[float]
var final_selected_point_position : Vector2
var point_hit_position : Array[Vector2]
var element_sizes : Array[Vector2]
var element_positions : Array[Vector2]
var point_parents : Array[Control]
var point_positions : Array[Vector2]
var current_whole_index : int
var current_total_group_connections : int

@export var ContainerTex : Texture2D = load("res://addons/Spatial navigation mapper/ContainerBorder.png"):
	get:
		return container_tex
	set(value):
		container_tex = value
		CheckSetGroupContainerColorAndTex()
		pass
var container_tex : Texture2D = load("res://addons/Spatial navigation mapper/ContainerBorder.png")

var check_frame : int

func QueueMap():
	if is_ready_to_map:
		mapping_queued = true

func QueueOnlyManualUpdate(msg : String):
	if auto_update == AutoUpdateTypes.Visualization:
		if is_ready_to_map:
			mapping_queued = true

func UpdateVisualOpacity():
	var value = opacity
	if visual_override != null:
		value = visual_override.opacity
	if visualization_grandparent != null:
		var clr = visualization_grandparent.modulate
		visualization_grandparent.modulate = Color(clr.r,clr.g,clr.b,value/100.0)
	pass

func UpdateGroupPadding(value : int):
	var change : int = value - group_box_padding
	group_box_padding = value
	if container_parent != null:
		for i in container_parent.get_child_count():
			var border = container_parent.get_child(i)
			border.position -= Vector2(change,change)
			border.size += Vector2(change,change)*2.0
	pass

func CheckSetArrowColors(type):
	var parents : Array[Control] = [pointer_parent,group_pointer_parent]
	if parents[type] != null:
		for i in element_groups.size():
			var pointer : SpatNavPointer
			if type == 0:
				var group_parent = pointer_parent.get_child(i)
				for e in group_parent.get_child_count():
					pointer = group_parent.get_child(e)
					pointer.SetPointerColor(CompileColorsToArray(0))
				pass
			else:
				for e in group_pointer_parent.get_child_count():
					pointer = group_pointer_parent.get_child(e)
					pointer.SetPointerColor(CompileColorsToArray(1))
				pass
			pass
		pass
	pass

func CheckSetGroupContainerColorAndTex():
	if container_parent != null:
		var color_to_use = group_container_color
		var tex_to_use = container_tex
		if visual_override != null:
			color_to_use = visual_override.group_container_color
			tex_to_use = visual_override.container_tex

		for i in element_groups.size():
			var child : NinePatchRect = container_parent.get_child(i)
			child.modulate = color_to_use
			child.texture = tex_to_use
			pass
		pass
	pass

func CompileColorsToArray(i):
	var element_colors : Array[Color] = [up_color, down_color, left_color, right_color]
	if (i == 1):
		element_colors = [group_up_color, group_down_color, group_left_color, group_right_color]
	if (visual_override != null):
		if i == 0:
			element_colors = [visual_override.up_color, visual_override.down_color, visual_override.left_color , visual_override.right_color]
		elif (i == 1):
			element_colors = [visual_override.group_up_color, visual_override.group_down_color, visual_override.group_left_color, visual_override.group_right_color]
		pass
	return element_colors

func UpdateGroupLabelSizes():
	for i in container_parent.get_child_count():
		var text_label = container_parent.get_child(i).get_child(0)
		var size : float = group_label_size
		if visual_override != null:
			size = visual_override.group_label_size
		text_label.scale = Vector2(size,size)
		text_label.position = CalculateLabelOffset()
	pass

func _enter_tree():
	for i in navigation_references.size():
		navigation_references[i].parent_mapper = self
		pass
	for i in element_groups.size():
		element_groups[i].parent_mapper = self
		pass
	pass

func _exit_tree():
	for i in navigation_references.size():
		navigation_references[i].parent_mapper = null
		pass
	for i in element_groups.size():
		element_groups[i].parent_mapper = null
		pass
	pass

func _ready():
	if (Engine.is_editor_hint()):
		is_ready_to_map = true
		if mapping_queued:
			mapping_queued = false
			MapElements()
		CheckSetVisualizerParents()
		pass
	else:
		#GDscript version seems to lose reference to vis-grandparent (and other similar ones) at runtime unless its exported (which is unnecessary) so this is a bandaid
		if visualization_grandparent == null:
			visualization_grandparent = get_node_or_null("Visualization grandparent")
	pass


func _physics_process(delta):
	if !lock_visibility_toggle:
		if Input.is_physical_key_pressed(alt_and_hotkey_to_hide) and Input.is_physical_key_pressed(KEY_ALT) and !visibility_input_lock:
			visibility_input_lock = true
			visualization_grandparent.visible = !visualization_grandparent.visible
		elif (!Input.is_physical_key_pressed(alt_and_hotkey_to_hide) && visibility_input_lock):
			visibility_input_lock = false

	if (Engine.is_editor_hint()):
		if (check_frame < 2 and !override_frame_limit):
			check_frame += 1
		else:
			check_frame = 0
			var empties = AnyGroupsHaveEmpty(false)
			if empties:
				return
			else:
				MoveCheck()
		if mapping_queued:
			MapElements()
			mapping_queued = false
	pass

func MoveCheck():
	# pull both piles from element containers
	# make big batch for check
	var full_list_of_elements : Array[Control]
	for i in element_groups.size():
		var node_pile : Array[NodePath] = element_groups[i].elements_to_map
		for e in node_pile.size():
			var node = get_node_or_null(node_pile[e])
			if node == null:
				return
			else:
				full_list_of_elements.append(node)

	# if counts dont match, reset
	var expected_count : int = full_list_of_elements.size()
	if element_positions.size() != expected_count or element_sizes.size() != expected_count:
		element_positions.clear()
		element_sizes.clear()
		for i in expected_count:
			element_positions.append(full_list_of_elements[i].position)
			element_sizes.append(full_list_of_elements[i].size)

	# check full pile for changes
	for i in expected_count:
		if element_positions[i] != full_list_of_elements[i].position or element_sizes[i] != full_list_of_elements[i].size:
			element_positions[i] = full_list_of_elements[i].position
			element_sizes[i] = full_list_of_elements[i].size
			mapping_queued = true

func CheckAndResetDirectionResources():
	var full_list_of_elements : Array[Control]
	for i in element_groups.size():
		var node_pile : Array[NodePath] = element_groups[i].elements_to_map
		for  j in node_pile.size():
			full_list_of_elements.append(get_node(node_pile[j]))

	var redo_navigation_resources : bool = (navigation_references.size() != full_list_of_elements.size())
	DebugPrint("redo navigation resources? : " + str(redo_navigation_resources))
	if redo_navigation_resources:
		navigation_references.clear()
	var fullIndex : int = 0
	var group_count = element_groups.size()
	for i in group_count:
		for j in element_groups[i].elements_to_map.size():
			if redo_navigation_resources:
				var ref : NavigationReference = NavigationReference.new()
				ref.container_id = i
				navigation_references.append(ref)
				pass
			else:
				navigation_references[fullIndex].container_id = i
			fullIndex += 1


func AnyGroupsHaveEmpty(can_force_warning):
	var empty_found : bool = false
	var type_str : String = ""
	var warning_parent_index : int = -1
	for i in element_groups.size():
		if element_groups[i] == null or element_groups[i].elements_to_map.size() <= 0:
			empty_found = true
			warning_parent_index = i
			type_str = ""
			break
		else:
			for e in element_groups[i].elements_to_map.size():
				if (element_groups[i].elements_to_map[e] == NodePath("")):
					empty_found = true
					warning_parent_index = i
					type_str = ", child index " + str(e)
	if (empty_found and (!empty_warning_printed or can_force_warning)):
		empty_warning_printed = true
		printerr("Empty found in container group index " + str(warning_parent_index) + type_str)
	if (!empty_found and empty_warning_printed):
		empty_warning_printed = false
	return empty_found

func MapElements():
	if AnyGroupsHaveEmpty(true) == true:
		return
	else:
		# Rebuild custom path reference resources with group ID & origin references
		if auto_update == AutoUpdateTypes.All:
			CheckAndResetDirectionResources()
		# Check group & other relevant counts and adjust visualizer group-counts accordingly
		CheckSetVisualizerParents()
		SubContainerToGroupsCountCheck()

		# element to element within group
		if auto_update == AutoUpdateTypes.All:
			ElementNavigationMapping()
			pass
		elif auto_update == AutoUpdateTypes.Visualization:
			MatchToManualAndAddArrows()
			pass

		# group to group, maps even with update set to only visualization
		GroupNavigationMapping()

		# add self to any empty built in focus reference to work around unwanted, unmapped navigation
		if set_base_godot_focus:
			SelfToEmpty()
		SetFocusModeToAll()

		# Since there's currently no custom logic for next/previous
		# I'm simply disabling it for the time being to not break the idea of the groups too much
		DisableNextPrevious()

func DisableNextPrevious():
	var element : Control
	for i in navigation_references.size():
		element = get_node(navigation_references[i].origin)
		if !allow_base_godot_next_and_previous:
			element.focus_next = NodePath(".")
			element.focus_previous = NodePath(".")
		else:
			element.focus_next = NodePath("")
			element.focus_previous = NodePath("")
func SetFocusModeToAll():
	var element : Control
	for i in navigation_references.size():
		element = get_node(navigation_references[i].origin)
		if set_base_godot_focus:
			element.focus_mode = Control.FOCUS_ALL
		else:
			element.focus_mode = Control.FOCUS_NONE

func MatchToManualAndAddArrows():
	var total_index : int = 0
	total_connection_count = 0
	current_whole_index = 0
	for i in element_groups.size():
		for e in element_groups[i].elements_to_map.size():
			var node : Control = get_node(element_groups[i].elements_to_map[e])
			node.focus_neighbor_top = NodePath("")
			node.focus_neighbor_bottom = NodePath("")
			node.focus_neighbor_left = NodePath("")
			node.focus_neighbor_right = NodePath("")
			CheckForOutOfGroupReference(total_index, i)
			ReApplyReferenceAndCalculateForAlreadyKnownTarget(node, total_index)
			total_index += 1
			pass

		var mappable : Array[Control] = CreateToBeMappedArray(i)
		PointerGeneration(mappable, pointer_parent.get_child(i))
		SetGroupVisualizationBox(mappable, i)

		# hitpoints, pointer index counter & connection count
		ClearPointerListsAndValues()
		pass
	pass

func CheckForOutOfGroupReference(total_index, group_index):
	for direction in directions:

		var checked_node : Control = get_node_or_null(GetNavReferencePath(direction,total_index))

		if checked_node != null:
			var group_has_node = GroupContainsNode(checked_node, element_groups[group_index].elements_to_map)
			if !group_has_node:
				navigation_references[total_index].SetDirectionReferences(direction, NodePath(""), true)


func GroupContainsNode(compared_node, path_pile):
	var is_found : bool = false
	for i in path_pile.size():
		if compared_node == get_node_or_null(path_pile[i]):
			is_found = true
			break
	return is_found

func ReApplyReferenceAndCalculateForAlreadyKnownTarget(current_origin_node : Control, total_index):
	for direction in directions:
		var target : Control = null
		match direction:
			Vector2(0.0, -1.0):
				target = get_node_or_null(navigation_references[total_index].up)

			Vector2(0.0, 1.0):
				target = get_node_or_null(navigation_references[total_index].down)

			Vector2(-1.0, 0.0):
				target = get_node_or_null(navigation_references[total_index].left)

			Vector2(1.0, 0.0):
				target = get_node_or_null(navigation_references[total_index].right)

		if target != null and target != current_origin_node:
			total_connection_count += 1
			#repeat point checks & calculations ***only*** for known selected target node and calculate best point for arrows
			AddNewPointsToLists(target, direction)
			CalculateComparisonValues(current_origin_node, direction)
			SelectBestDirectionalCandidate(180.0, false, direction)
			point_hit_position.append(final_selected_point_position)
			ClearDirectionalCheckListsAndValues()

		if set_base_godot_focus:
			var origin_to_selected : NodePath
			if target == null:
				origin_to_selected = NodePath("")
			else:
				origin_to_selected = current_origin_node.get_path_to(target)
			match direction:
				Vector2(0.0, -1.0):
					current_origin_node.focus_neighbor_top = origin_to_selected
				Vector2(0.0, 1.0):
					current_origin_node.focus_neighbor_bottom = origin_to_selected
				Vector2(-1.0, 0.0):
					current_origin_node.focus_neighbor_left = origin_to_selected
				Vector2(1.0, 0.0):
					current_origin_node.focus_neighbor_right = origin_to_selected
		else:
			match direction:
				Vector2(0.0, -1.0):
					current_origin_node.focus_neighbor_top = NodePath(".")
				Vector2(0.0, 1.0):
					current_origin_node.focus_neighbor_bottom = NodePath(".")
				Vector2(-1.0, 0.0):
					current_origin_node.focus_neighbor_left = NodePath(".")
				Vector2(1.0, 0.0):
					current_origin_node.focus_neighbor_right = NodePath(".")

func CreateToBeMappedArray(group_index):
	var mappable : Array[Control]
	for element_path : NodePath in element_groups[group_index].elements_to_map:
		mappable.append(get_node(element_path))
	return mappable

func CreateGroupMappedArray():
	var mappable : Array[Control]
	for i in element_groups.size():
		mappable.append(container_parent.get_child(i))
	return mappable

func ElementNavigationMapping():
	current_whole_index = 0
	for i in element_groups.size():
		var index_at_iteration_start : int = current_whole_index

		var mappable : Array[Control] = CreateToBeMappedArray(i)
		NavigationMappingLogic(mappable, i)

		#Reset changes made to whole-index check value for pointer generation
		current_whole_index = index_at_iteration_start

		PointerGeneration(mappable, pointer_parent.get_child(i))
		SetGroupVisualizationBox(mappable, i)
		ClearPointerListsAndValues()
		pass
	pass

func NavigationMappingLogic(mappable : Array[Control], group_id : int):
	for e in mappable.size():
		var current_origin_node : Control = mappable[e]
		navigation_references[current_whole_index].origin = get_path_to(current_origin_node)

		for direction in directions:
			DebugPrint("------------------------------------")
			DebugPrint("Group " + str(group_id)+ " / id " +str(e) + " / " + PrintDirection(direction))
			ListAllViableTargetPoints(current_origin_node, direction, mappable)
			CalculateComparisonValues(current_origin_node, direction)
			SelectBestDirectionalCandidate(maximum_target_angle, false, direction)
			SetFinalReferences(current_origin_node, direction, false)
			ClearDirectionalCheckListsAndValues()
			pass

		current_whole_index += 1

func GroupNavigationMapping():
	var mappable_groups : Array[Control] = CreateGroupMappedArray()
	for i in mappable_groups.size():
		var current_origin_node : Control = mappable_groups[i]
		for direction in directions:
			current_whole_index = -1
			ListAllViableTargetPoints(current_origin_node, direction, mappable_groups)
			CalculateComparisonValues(current_origin_node, direction)
			SelectBestDirectionalCandidate(maximum_group_target_angle, true, direction)
			SetGroupPointers(direction, current_origin_node)
			var count_in_group : int = element_groups[i].elements_to_map.size()
			for e in count_in_group:
				if current_whole_index == -1:
					for p in navigation_references.size():
						if (MatchPathToPathViaNode(navigation_references[p].origin, element_groups[i].elements_to_map[e])):
							current_whole_index = p
							break
				else:
					current_whole_index += 1
				SetFinalReferences(get_node(element_groups[i].elements_to_map[e]), direction, true)

			ClearDirectionalCheckListsAndValues()

	DeleteStragglerGroupPointers()
	current_total_group_connections = 0
	ClearPointerListsAndValues()


func SelfToEmpty():
	for i in element_groups.size():
		var mappable : Array[Control] = CreateToBeMappedArray(i)
		for e in mappable.size():
			for direction in directions:
				match direction:
					Vector2(0.0, -1.0):
						if (mappable[e].focus_neighbor_top == NodePath("")):
							mappable[e].focus_neighbor_top = "."

					Vector2(0.0, 1.0):
						if (mappable[e].focus_neighbor_bottom == NodePath("")):
							mappable[e].focus_neighbor_bottom = "."

					Vector2(-1.0, 0.0):
						if (mappable[e].focus_neighbor_left == NodePath("")):
							mappable[e].focus_neighbor_left = "."

					Vector2(1.0, 0.0):
						if (mappable[e].focus_neighbor_right == NodePath("")):
							mappable[e].focus_neighbor_right = "."


func DeleteStragglerGroupPointers():
	var child_count : int = group_pointer_parent.get_child_count()
	for i in child_count:
		if i >= current_total_group_connections:
			group_pointer_parent.get_child(i).queue_free()

func MatchPathToPathViaNode(one, two):
	return get_node(one) == get_node(two)

func CalculateArrowTarget(direction : Vector2, node : Control):
	var arrow_target

	match direction:
		Vector2(0.0, -1.0):
			arrow_target = node.global_position + Vector2(node.size.x / 2.0, 0.0)
		Vector2(0.0, 1.0):
			arrow_target = node.global_position + Vector2(node.size.x / 2.0, node.size.y)
		Vector2(-1.0, 0.0):
			arrow_target = node.global_position + Vector2(0.0, node.size.y / 2.0)
		Vector2(1.0, 0.0):
			arrow_target = node.global_position + Vector2(node.size.x, node.size.y / 2.0)

	return arrow_target

func PointerGeneration(mappable : Array[Control], sub_parent : Control):
	pre_existing_pointer_counts = sub_parent.get_child_count()
	pointer_difference = total_connection_count - pre_existing_pointer_counts

	for e in mappable.size():
		var current_origin_node = mappable[e]
		for direction in directions:
			var target : Control = get_node_or_null(GetNavReferencePath(direction, current_whole_index))
			if target != null:
				SetPointerArrows(target,current_origin_node, direction,sub_parent)
				pass
		current_whole_index += 1

	#excess pointer removal
	if pointer_difference < 0:
		for e in sub_parent.get_child_count():
			if e >= total_connection_count:
				sub_parent.get_child(e).queue_free()

func SetPointerArrows(target : Control, current_origin_node : Control, direction : Vector2, sub_parent : Control):
	if target == null:
		printerr(str(PrintDirection(direction)) + " has no target for some reason in trying to set a pointer, canceling")
		return
	var pointer : SpatNavPointer
	if pointer_difference > 0:
		pointer = SpatNavPointer.new()
		sub_parent.add_child(pointer)
		sub_parent.move_child(pointer, current_pointer_index)
		pointer.owner = get_tree().edited_scene_root
		pointer.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
		pointer_difference -= 1
	else:
		pointer = sub_parent.get_child(current_pointer_index)

	if pointer != null:
		pointer.global_position = point_hit_position[current_pointer_index]
		CalculatePointerDirAndPos(0, direction, current_origin_node, pointer)
	else:
		printerr("Pointer references error")

	current_pointer_index += 1
	pass

func SetGroupPointers(direction : Vector2, current_origin_node : Control):
	if current_selected_group_container == null:
		return

	var pointer : SpatNavPointer
	if group_pointer_parent.get_child_count() <= current_total_group_connections:
		pointer = SpatNavPointer.new()
		group_pointer_parent.add_child(pointer)
		group_pointer_parent.move_child(pointer, current_total_group_connections)
		pointer.owner = get_tree().edited_scene_root
		pointer.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	else:
		pointer = group_pointer_parent.get_child(current_total_group_connections)
	current_total_group_connections += 1

	pointer.global_position = CalculateArrowTarget(direction * -1.0, current_selected_group_container)
	CalculatePointerDirAndPos(1, direction, current_origin_node, pointer)
	pass

func CalculatePointerDirAndPos(type : int, direction : Vector2, current_origin_node : Control, pointer : SpatNavPointer):

	var direction_index : int = GetDirectionIndex(direction)

	var direction_vector : Vector2 = CalculateRelativeVector(CalculateArrowTarget(direction, current_origin_node), pointer.global_position)
	var direction_normal : Vector2 = direction_vector.normalized()

	pointer.global_position -= Vector2(direction_normal.y, -direction_normal.x) * 5.0
	pointer.SetPointerParameters(direction_index, direction_normal, direction_vector.length(), CompileColorsToArray(type),pointer_letters)

func SetGroupVisualizationBox(mappable : Array[Control], index : int):
	var container_visualizer : NinePatchRect = container_parent.get_child(index)
	if container_visualizer == null:
		printerr("container visualizer check returns a null")
		return
	else:
		var smallest : Vector2 = Vector2(9999999.0,9999999.0)
		var largest : Vector2 = Vector2(0.0,0.0)
		for e in mappable.size():
			var current : Control = mappable[e]
			if (current.global_position.x < smallest.x):
				smallest.x = current.global_position.x
			if (current.global_position.y < smallest.y):
				smallest.y = current.global_position.y

			if (current.global_position.x + current.size.x > largest.x):
				largest.x = current.global_position.x + current.size.x
			if (current.global_position.y + current.size.y > largest.y):
				largest.y = current.global_position.y + current.size.y

		container_visualizer.global_position = smallest - Vector2(group_box_padding, group_box_padding)
		container_visualizer.size = CalculateRelativeVector(largest, smallest) + Vector2(group_box_padding, group_box_padding) * 2.0
		var color_to_use : Color = group_container_color
		if visual_override != null:
			color_to_use = visual_override.group_container_color
		container_visualizer.modulate = color_to_use
		var label : Label = container_visualizer.get_child(0)
		label.text = "Group " + str(index)

func ClearDirectionalCheckListsAndValues():
	point_positions.clear()
	point_parents.clear()
	distances.clear()
	finals.clear()
	angles.clear()

func ClearPointerListsAndValues():
	point_hit_position.clear()
	total_connection_count = 0
	current_pointer_index = 0

func ListAllViableTargetPoints(current_origin_node : Control, direction : Vector2, mappable : Array[Control]):
	for i in mappable.size():
		if mappable[i] != current_origin_node:
			var current_center : Vector2 = CalculateControlCenter(current_origin_node)
			var target_center : Vector2 = CalculateControlCenter(mappable[i])
			if (CheckDirectionHit(direction, current_center, target_center)):
				AddNewPointsToLists(mappable[i], direction)
	DebugPrint("total points found: " + str(point_parents.size()))

func AddNewPointsToLists(current_target_node : Control, direction : Vector2):

	var y_centering : Vector2 = Vector2(0.0, current_target_node.size.y / 2.0)
	var y_additive : Vector2 = Vector2(current_target_node.size.x / (check_resolution - 1.0), 0.0)

	var x_centering : Vector2 = Vector2(current_target_node.size.x / 2.0, 0.0)
	var x_additive : Vector2 = Vector2(0.0, current_target_node.size.y / (check_resolution - 1.0))

	for k in check_resolution:
		point_positions.append(CalculatePointPosition(current_target_node, k, direction, y_centering, x_centering, y_additive, x_additive))
		point_parents.append(current_target_node)
		pass

func CalculatePointPosition(current_target_node : Control, index, direction : Vector2, y_centering, x_centering, y_additive, x_additive):
	var is_vertical : bool = abs(direction.y) == 1
	var centering : Vector2
	var additive : Vector2
	if is_vertical:
		centering = y_centering
		additive = y_additive
	else:
		centering = x_centering
		additive = x_additive
	return current_target_node.global_position + centering + (additive * index)

func GetCorrectElementIndexFromGroup(mappable : Control, direction : Vector2):
	var element = null
	for g in element_groups.size():
		if mappable == container_parent.get_child(g):
			match direction:
				Vector2(0.0, -1.0):
					element = get_node(element_groups[g].elements_to_map[element_groups[g].bottom_entry_selection_index])
					pass
				Vector2(0.0, 1.0):
					element = get_node(element_groups[g].elements_to_map[element_groups[g].top_entry_selection_index])
					pass
				Vector2(-1.0, 0.0):
					element = get_node(element_groups[g].elements_to_map[element_groups[g].right_entry_selection_index])
					pass
				Vector2(1.0, 0.0):
					element = get_node(element_groups[g].elements_to_map[element_groups[g].left_entry_selection_index])
					pass
	return element

func CheckDirectionHit(direction : Vector2, current : Vector2, target : Vector2):
	var isHit : bool = false
	match direction:
		Vector2(0.0, -1.0):
			isHit = current.y > target.y
		Vector2(0.0, 1.0):
			isHit = current.y < target.y
		Vector2(-1.0, 0.0):
			isHit = current.x > target.x
		Vector2(1.0, 0.0):
			isHit = current.x < target.x
	return isHit

func CalculateComparisonValues(current_origin_node : Control, direction : Vector2):
	for i in point_positions.size():
		if point_parents[i] != current_origin_node:
			var from_current_to_target = point_positions[i] - CalculateControlCenter(current_origin_node)
			var dot_product = direction.dot(from_current_to_target.normalized())
			var distance = from_current_to_target.length()
			var angle = rad_to_deg(acos(dot_product))
			if current_origin_node.name == "Button":
				DebugPrint("Angle: " + str(angle))
				DebugPrint(distance)
			angles.append(angle)
			finals.append(dot_product/distance)
			distances.append(distance)

func SelectBestDirectionalCandidate(max_angle : float, for_group : bool, direction : Vector2):
	current_selected = null
	current_selected_group_container = null
	current_highest_weight = 0.0

	for i in point_positions.size():
		if (angles[i] <= max_angle && (distance_cut_off == -1.0 or distances[i] < distance_cut_off)):
			if (finals[i] > current_highest_weight or current_selected == null):
				current_highest_weight = finals[i]
				final_selected_point_position = point_positions[i]
				if for_group:
					current_selected_group_container = point_parents[i]
					current_selected = GetCorrectElementIndexFromGroup(point_parents[i], direction)
				else:
					current_selected = point_parents[i]
	if current_selected != null:
		DebugPrint("selected: " + current_selected.name)


func SetFinalReferences(current_origin_node : Control, direction : Vector2, for_group : bool):
	var origin_to_selected : NodePath
	var this_to_selected : NodePath
	var has_selection = false

	if current_selected != null:
		origin_to_selected = current_origin_node.get_path_to(current_selected)
		this_to_selected = get_path_to(current_selected)
		has_selection = true
		total_connection_count += 1
		point_hit_position.append(final_selected_point_position)
	else:
		DebugPrint("No final target selected")
		origin_to_selected = NodePath("")
		this_to_selected = NodePath("")
		pass

	match direction:
		Vector2(0.0, -1.0):
			if (navigation_references[current_whole_index].up == NodePath("") && has_selection) or !for_group:
				navigation_references[current_whole_index].SetDirectionReferences(direction, this_to_selected, for_group)
				if set_base_godot_focus:
					current_origin_node.focus_neighbor_top = origin_to_selected
				else:
					current_origin_node.focus_neighbor_top = NodePath(".")

		Vector2(0.0, 1.0):
			if (navigation_references[current_whole_index].down == NodePath("") && has_selection) or !for_group:
				navigation_references[current_whole_index].SetDirectionReferences(direction, this_to_selected, for_group)
				if set_base_godot_focus:
					current_origin_node.focus_neighbor_bottom = origin_to_selected
				else:
					current_origin_node.focus_neighbor_bottom = NodePath(".")

		Vector2(-1.0, 0.0):
			if (navigation_references[current_whole_index].left == NodePath("") && has_selection) or !for_group:
				navigation_references[current_whole_index].SetDirectionReferences(direction, this_to_selected, for_group)
				if set_base_godot_focus:
					current_origin_node.focus_neighbor_left = origin_to_selected
				else:
					current_origin_node.focus_neighbor_left = NodePath(".")

		Vector2(1.0, 0.0):
			if (navigation_references[current_whole_index].right == NodePath("") && has_selection) or !for_group:
				navigation_references[current_whole_index].SetDirectionReferences(direction, this_to_selected, for_group)
				if set_base_godot_focus:
					current_origin_node.focus_neighbor_right = origin_to_selected
				else:
					current_origin_node.focus_neighbor_right = NodePath(".")


func GetNavReferencePath(direction : Vector2, index : int):
	var path_to_return : NodePath

	match direction:
		Vector2(0.0, -1.0):
			path_to_return = navigation_references[index].up
		Vector2(0.0, 1.0):
			path_to_return = navigation_references[index].down
		Vector2(-1.0, 0.0):
			path_to_return = navigation_references[index].left
		Vector2(1.0, 0.0):
			path_to_return = navigation_references[index].right

	return path_to_return

func CheckSetVisualizerParents():
	#These are just to make this a bit more tidy
	#C# version uses GetChildOrNull with indexes... why doesn't that method exist in gdscript?? :(
	var names : Array[String] = ["Visualization grandparent","Pointer parent","Container parent","Group pointer parent"]

	visualization_grandparent = AddMissingParentControl(get_node_or_null(names[0]), names[0], 0)
	DebugPrint("gp: " + str(visualization_grandparent))
	#If parent exists, the method simply passes it through, otherwise creates new one and assigns needed child position and values
	pointer_parent = AddMissingParentControl(visualization_grandparent.get_node_or_null(names[1]), names[1], 1)
	container_parent = AddMissingParentControl(visualization_grandparent.get_node_or_null(names[2]), names[2], 2)
	group_pointer_parent = AddMissingParentControl(visualization_grandparent.get_node_or_null(names[3]), names[3], 3)

func AddMissingParentControl(parent_node : Control, desired_name : String, type : int):
	if (parent_node == null) or (parent_node.name != desired_name):
		parent_node = Control.new()
		parent_node.name = desired_name
		parent_node.global_position = Vector2.ZERO
		if type != 0:
			visualization_grandparent.add_child(parent_node)
			visualization_grandparent.move_child(parent_node, type-1)
		else:
			print("Adding new parent for visualization nodes! - It's recommended to *lock* and toggle *group selected nodes* on for 'Visualization grandparent' to make pointers & group-boxes as non-intrusive and non-selectable as possible.")
			add_child(parent_node)
			move_child(parent_node, 0)

		parent_node.owner = get_tree().edited_scene_root
	return parent_node

func ChildDifference(count : int):
	return count - element_groups.size()

func SubContainerToGroupsCountCheck():
	var parents : Array[Control] = [pointer_parent, container_parent]
	var child_counts : Array[int] = [parents[0].get_child_count(), parents[1].get_child_count()]
	var sub_parent_differences : Array[int] = [ChildDifference(child_counts[0]), ChildDifference(child_counts[1])]

	# these aren't actually needed for group to group, thus only typeamount of 2, since they are just in one big pile & are checked elsewhere
	var type_amount = 2
	for i in type_amount:
		var child_count = child_counts[i]
		var difference_to_use = sub_parent_differences[i]
		if difference_to_use < 0:
			for e in abs(difference_to_use):
				match i:
					0:
						AddPointerParentSubControls(e, child_count)
					1:
						AddContainerParentSubs(e, child_count)
					2:
						# group-to-group, were they needed to be checked here
						pass

		elif difference_to_use > 0:
			for e in child_count:
				if e>difference_to_use:
					parents[i].get_child(e).queue_free()

func AddPointerParentSubControls(index : int, existing_count : int):
	var parent_child : Control = Control.new()
	pointer_parent.add_child(parent_child)
	parent_child.owner = get_tree().edited_scene_root
	parent_child.name = "Pointers " + str(index + existing_count)
	pass

func AddContainerParentSubs(index : int, existing_count : int):
	var parent_child : NinePatchRect = NinePatchRect.new()
	container_parent.add_child(parent_child)
	parent_child.owner = get_tree().edited_scene_root
	parent_child.texture = container_tex
	parent_child.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	parent_child.name = "Container of group " + str(index + existing_count)
	var padding = 6
	parent_child.patch_margin_bottom = padding
	parent_child.patch_margin_top = padding
	parent_child.patch_margin_left = padding
	parent_child.patch_margin_right = padding

	var text_label : Label = Label.new()
	parent_child.add_child(text_label)
	text_label.text = "Group " + str(index+existing_count)
	var size : float = group_label_size
	if visual_override != null:
		size = visual_override.group_label_size
	text_label.scale = Vector2(size,size)
	text_label.position = CalculateLabelOffset()
	text_label.owner = get_tree().edited_scene_root

func CalculateLabelOffset():
	var size : float = group_label_size
	if visual_override != null:
		size = visual_override.group_label_size
	return Vector2(0.0, -10.0 - 20.0 * (size - 0.5))

func CalculateRelativeVector(to : Vector2, from : Vector2):
	return to - from

func AngleFromPointToPoint(to : Vector2, from : Vector2):
	var direction_vector = to - from
	return rad_to_deg(atan2(direction_vector.y, direction_vector.x)) + 90.0

func CalculateControlCenter(element : Control):
	if element != null:
		return element.global_position + element.size / 2.0
	else:
		return Vector2.ZERO

func GetDirectionIndex(direction : Vector2):
	var id
	match direction:
		Vector2(0.0, -1.0):
			id = 0
		Vector2(0.0, 1.0):
			id = 1
		Vector2(-1.0, 0.0):
			id = 2
		Vector2(1.0, 0.0):
			id = 3
	return id

func DebugPrint(logMessage):
	if debug:
		print(logMessage)
	pass

func PrintDirection(direction : Vector2):
	var dirstring : String
	match direction:
		Vector2(0.0, -1.0):
			dirstring = "up"
		Vector2(0.0, 1.0):
			dirstring = "down"
		Vector2(-1.0, 0.0):
			dirstring = "left"
		Vector2(1.0, 0.0):
			dirstring = "right"
	return dirstring
