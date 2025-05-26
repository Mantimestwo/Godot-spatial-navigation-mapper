using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class NavMapper : Node
{
	[ExportToolButton("Map navigation", Icon = "CanvasItem")]
	public Callable ClickMeButton => Callable.From(MapElements);

	bool mappingQueued = false;
	bool isReadyToMap = false;
	bool debug;
	[Export] Key altAndToHide = Key.H;
	[Export] bool lockVisibilityToggle = false;
	bool visibilityInputLock = false;
	bool emptyWarningPrinted;
	enum AutoUpdateTypes { All, Visualization }
	[Export] AutoUpdateTypes autoUpdate = AutoUpdateTypes.All;
	[Export]
	bool SetBaseGodotFocus
	{
		get { return setBaseGodotFocus; }
		set
		{
			setBaseGodotFocus = value;
			if (isReadyToMap) mappingQueued = true;
		}
	}
	bool setBaseGodotFocus = true;

	[Export]
	bool AllowBaseGodotNextAndPrevious
	{
		get { return allowBaseGodotNextAndPrevious; }
		set
		{
			allowBaseGodotNextAndPrevious = value;
			if (isReadyToMap) mappingQueued = true;
		}
	}
	bool allowBaseGodotNextAndPrevious = false;

	[Export] bool overrideFrameLimit;

	[ExportCategory("Parameters")]
	[Export]
	NavigationGroup[] ElementGroups
	{
		get { return elementGroups; }
		set
		{
			bool isChanged = elementGroups != value;
			elementGroups = value;
			for (int i = 0; i < elementGroups.Length; i++)
			{
				if(elementGroups[i] != null) elementGroups[i].parentMapper = this;
			}
			if (isReadyToMap) mappingQueued = true;
		}
	}
	NavigationGroup[] elementGroups = [];
	
	[Export] 
	int CheckResolution {
		get{ return checkResolution; }
		set{
			bool isChanged = checkResolution != value;
			if (value < 0) value = 0;
			checkResolution = value;
			if (isReadyToMap) mappingQueued = isChanged;
		}
	}
	int checkResolution = 7; 
	
	[Export(PropertyHint.Range, "0,90,0.1,suffix: degrees")] 
	float MaximumTargetAngle {
		get{ return maximumTargetAngle; }
		set{ 
			bool isChanged = maximumTargetAngle != value;
		 	maximumTargetAngle = value;
			if (isReadyToMap) mappingQueued = isChanged;
		}
	}
	float maximumTargetAngle = 30.0f;

	[Export(PropertyHint.Range, "0,90,0.1,suffix: degrees")]
	float MaximumGroupTargetAngle
	{
		get { return maximumGroupTargetAngle; }
		set
		{
			bool isChanged = maximumGroupTargetAngle != value;
			maximumGroupTargetAngle = value;
			if (isReadyToMap) mappingQueued = isChanged;
		}
	}
	float maximumGroupTargetAngle = 30.0f;

	
	
	Control currentSelected;
	Control currentSelectedGroupContainer;
	float currentHighestWeight;
	
	Control pointerParent;
	int currentPointerIndex = 0;
	int preExistingPointerCounts;
	int totalConnectionCount = 0;
	int pointerDifference = 0;
	Control containerParent;
	Control visualizationGrandparent;
	
	Control groupPointerParent;

	[Export]
	Godot.Collections.Array<NavigationReference> NavigationReferences
	{
		get { return navigationReferences; }
		set
		{
			bool isChanged = navigationReferences != value;
			navigationReferences = value;
		}
	}
	Godot.Collections.Array<NavigationReference> navigationReferences = [];
	[Export(PropertyHint.Range, "-1.0,4000.0,2.0,suffix:units")]
	float DistanceCutOff
	{
		get { return distanceCutOff; }
		set
		{
			bool isChanged = distanceCutOff != value;
			distanceCutOff = value;
			if(isReadyToMap) mappingQueued = isChanged;
		}
	}
	float distanceCutOff = -1.0f;

	[ExportGroup("Visuals")]
	[Export(PropertyHint.Range, "0,1.0,0.01,suffix:percent")] 
	public float Opacity{
		get { return opacity; }
		set
		{
			opacity = value;
			UpdateVisualOpacity();
		}
	}
	float opacity = 1.0f;
	[Export] 
	public NavMapVisualOverride ColorOverride
	{
		get { return visualOverride; }
		set
		{
			visualOverride = value;
			CheckSetArrowColors(0);
			CheckSetArrowColors(1);
			CheckSetGroupContainerColorAndTex();
			UpdateGroupLabelSizes();
			UpdateVisualOpacity();

			if (visualOverride != null)
				UpdateGroupPadding(visualOverride.GroupBoxPadding);
			else
				UpdateGroupPadding(6);
		}
	}
	NavMapVisualOverride visualOverride;

	[Export] public Color UpColor
	{
		get { return upColor; }
		set
		{
			upColor = value;
			CheckSetArrowColors(0);
		}
	}
	Color upColor = new(0.89f, 0.34f, 0.18f, 1.00f);
	[Export] public Color DownColor
	{
		get { return downColor; }
		set
		{
			downColor = value;
			CheckSetArrowColors(0);
		}
	}
	Color downColor = new(0.00f, 0.62f, 1.00f, 1.00f);
	[Export] public Color LeftColor
	{
		get { return leftColor; }
		set
		{
			leftColor = value;
			CheckSetArrowColors(0);
		}
	}
	Color leftColor = new(0.67f, 0.85f, 0.36f, 1.00f);
	[Export] public Color RightColor
	{
		get { return rightColor; }
		set
		{
			rightColor = value;
			CheckSetArrowColors(0);
		}
	}
	Color rightColor = new(0.73f, 0.22f, 0.60f, 1.00f);
	
	[Export] public Color GroupContainerColor
	{
		get { return groupContainerColor; }
		set
		{
			groupContainerColor = value;
			CheckSetGroupContainerColorAndTex();
		}
	}
	Color groupContainerColor = new(1f, 0.765f, 0.0f,1f);

	[Export] public Color GroupUpColor
	{
		get { return groupUpColor; }
		set
		{
			groupUpColor = value;
			CheckSetArrowColors(1);
		}
	}
	Color groupUpColor = new(0.96f, 0.15f, 0.08f, 1.00f);
	[Export] public Color GroupDownColor
	{
		get { return groupDownColor; }
		set
		{
			groupDownColor = value;
			CheckSetArrowColors(1);
		}
	}
	Color groupDownColor = new(0.00f, 0.30f, 1.00f, 1.00f);
	[Export] public Color GroupLeftColor
	{
		get { return groupLeftColor; }
		set
		{
			groupLeftColor = value;
			CheckSetArrowColors(1);
		}
	}
	Color groupLeftColor = new(0.39f, 0.93f, 0.59f, 1.00f);
	[Export] public Color GroupRightColor
	{
		get { return groupRightColor; }
		set
		{
			groupRightColor = value;
			CheckSetArrowColors(1);
		}
	}
	Color groupRightColor = new(0.98f, 0.13f, 0.4f, 1.00f);

	[Export]
	int GroupBoxPadding
	{
		get { return groupBoxPadding; }
		set
		{
			value = Math.Clamp(value, -5, 999);
			if (visualOverride == null) UpdateGroupPadding(value);
		}
	}
	int groupBoxPadding = 6;
	[Export]
	public float GroupLabelSize
	{
		get { return groupLabelSize; }
		set
		{
			groupLabelSize = value;
			UpdateGroupLabelSizes();
		}
	}
	float groupLabelSize = 0.5f;

	public string[] pointerLetters = ["U","D","L","R"];
	Vector2[] directions = [Vector2.Left, Vector2.Up, Vector2.Right, Vector2.Down];
	List<float> angles = [];
	List<float> finals = [];
	List<float> distances = [];
	Vector2 finalSelectedPointPosition;
	List<Vector2> pointHitPosition = [];
	List<Vector2> elementSizes = [];
	List<Vector2> elementPositions = [];
	List<Control> pointParents = [];
	List<Vector2> pointPositions = [];
	int currentWholeIndex;
	int currentTotalGroupConnections;
	[Export]
	Texture2D ContainerTex
	{
		get { return containerTex; }
		set
		{
			containerTex = value;
			CheckSetGroupContainerColorAndTex();
		}
	}
	Texture2D containerTex = GD.Load<Texture2D>("res://addons/Spatial navigation mapper/ContainerBorder.png");

	int checkFrame;
	public void QueueMap()
	{
		if (isReadyToMap) { mappingQueued = true; }
	}
	public void QueueOnlyManualUpdate(string msg)
	{
		if (autoUpdate == AutoUpdateTypes.Visualization)
		{
			if (isReadyToMap) { mappingQueued = true; }
		}
	}
	void UpdateVisualOpacity()
	{
		float value = opacity;
		if (visualOverride != null) value = visualOverride.opacity;
		if (visualizationGrandparent != null)
		{
			Color clr = visualizationGrandparent.Modulate;
			visualizationGrandparent.Modulate = new(clr.R, clr.G, clr.B, opacity);
		}

	}
	void UpdateGroupPadding(int value)
	{
		int change = value - groupBoxPadding;
		groupBoxPadding = value;
		Vector2 changeVec = new(change, change);
		if (containerParent != null)
		{
			for (int i = 0; i < containerParent.GetChildCount(); i++)
			{
				NinePatchRect border = containerParent.GetChild<NinePatchRect>(i);

				border.Position -= changeVec;
				border.Size += changeVec * 2.0f;
			}
		}
	}

	void CheckSetArrowColors(int type)
	{
		Control[] parents = [pointerParent, groupPointerParent];
		if (parents[type] != null)
		{
			for (int i = 0; i < elementGroups.Length; i++)
			{
				
				if(type == 0)
				{
					Control groupParent = pointerParent.GetChild<Control>(i);
					for (int e = 0; e < groupParent.GetChildCount(); e++)
						groupParent.GetChild<SpatNavPointer>(e).SetPointerColor(CompileColorsToArray(0));
				}
				else{
					for (int e = 0; e < groupPointerParent.GetChildCount(); e++)
						groupPointerParent.GetChild<SpatNavPointer>(e).SetPointerColor(CompileColorsToArray(1));
				}
				

			}
		}
	}
	void CheckSetGroupContainerColorAndTex()
	{
		if (containerParent != null){
			Color colorToUse = groupContainerColor;
			Texture2D texToUse = containerTex;
			if (visualOverride != null){
				colorToUse = visualOverride.groupContainerColor;
				texToUse = visualOverride.containerTex;
			}
			for (int i = 0; i < elementGroups.Length; i++)
			{
				NinePatchRect child = containerParent.GetChild<NinePatchRect>(i);
				child.Modulate = colorToUse;
				child.Texture = texToUse;
			}
		}

	}
	Color[] CompileColorsToArray(int i)
	{
		Color[] elementColors = [upColor, downColor, leftColor, rightColor];
		if (i == 1) elementColors = [groupUpColor, groupDownColor, groupLeftColor, groupRightColor];


		if (visualOverride != null)
		{
			if (i == 0) elementColors = [visualOverride.upColor, visualOverride.downColor,
													visualOverride.leftColor,visualOverride.rightColor];
			else if (i == 1) elementColors = [visualOverride.groupUpColor, visualOverride.groupDownColor, 
													visualOverride.groupLeftColor, visualOverride.groupRightColor];
		}
			

		return elementColors;
	}
	void UpdateGroupLabelSizes()
	{
		for (int i = 0; i < containerParent.GetChildCount(); i++)
		{
			Label textLabel = containerParent.GetChild<NinePatchRect>(i).GetChild<Label>(0);
			float size = visualOverride == null ? groupLabelSize : visualOverride.groupLabelSize;
			textLabel.Scale = new(size, size);
			textLabel.Position = CalculateLabelOffset();
		}	
	}
	public override void _EnterTree()
	{
		for (int i = 0; i < navigationReferences.Count; i++){
			navigationReferences[i].parentMapper = this;
		}
		for (int i = 0; i < ElementGroups.Length; i++){
			ElementGroups[i].parentMapper = this;
		}

	}
    public override void _ExitTree()
    {
		for (int i = 0; i < navigationReferences.Count; i++){
			navigationReferences[i].parentMapper = null;
		}
		for (int i = 0; i < ElementGroups.Length; i++){
			ElementGroups[i].parentMapper = null;
		}
	}
	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			isReadyToMap = true;
			if (mappingQueued)
			{
				mappingQueued = false;
				MapElements();
			}
			CheckSetVisualizerParents();

		}
	}
	
	public override void _PhysicsProcess(double delta)
    {

		if (!lockVisibilityToggle)
		{
			if (Input.IsPhysicalKeyPressed(altAndToHide) && Input.IsPhysicalKeyPressed(Key.Alt) && !visibilityInputLock)
			{
				visibilityInputLock = true;
				visualizationGrandparent.Visible = !visualizationGrandparent.Visible;
			}
			else if (!Input.IsPhysicalKeyPressed(altAndToHide) && visibilityInputLock) visibilityInputLock = false;
		}
		
	
		if (Engine.IsEditorHint()){
			if(checkFrame < 2 && !overrideFrameLimit) checkFrame++;
			else{
				checkFrame = 0;
				bool empties = AnyGroupsHaveEmpty(false);
				if (empties) return;
				else MoveCheck();
			}

			if (mappingQueued){
				MapElements();
				mappingQueued = false;
			}
		}
	}
    
	void MoveCheck()
	{
		//pull both piles from element containers
		//make big batch for check
		List<Control> fullListOfElements = [];
		for (int i = 0; i < elementGroups.Length; i++) 
		{
			Godot.Collections.Array<NodePath> nodePile = elementGroups[i].ElementsToMap;
			for (int e = 0; e < nodePile.Count; e++)
			{
				Control node = GetNodeOrNull<Control>(nodePile[e]);
				if(node == null){ GD.PrintErr($"Element reference list broken at group {i} index {e}!"); return;}
				fullListOfElements.Add(node);
			}
		}

		//if counts dont match, reset
		int expectedCount = fullListOfElements.Count;
		if (elementPositions.Count != expectedCount || elementSizes.Count != expectedCount)
		{
			elementPositions.Clear();
			elementSizes.Clear();
			for (int i = 0; i < expectedCount; i++)
			{
				elementPositions.Add(fullListOfElements[i].Position);
				elementSizes.Add(fullListOfElements[i].Size);
			}
		}

		//check full pile for changes
		for (int i = 0; i < expectedCount; i++)
		{
			if (elementPositions[i] != fullListOfElements[i].Position || elementSizes[i] != fullListOfElements[i].Size)
			{
				elementPositions[i] = fullListOfElements[i].Position;
				elementSizes[i] = fullListOfElements[i].Size;
				if (isReadyToMap) mappingQueued = true;
			}
		}
	}
	
	void CheckAndResetDirectionResources()
	{
		List<Control> fullListOfElements = [];
		for (int j = 0; j < elementGroups.Length; j++)
		{
			Godot.Collections.Array<NodePath> nodePile = elementGroups[j].ElementsToMap;
			for (int i = 0; i < nodePile.Count; i++)
				fullListOfElements.Add(GetNode<Control>(nodePile[i]));
		}

		bool RedoNavigationResources = navigationReferences.Count != fullListOfElements.Count;

		if (RedoNavigationResources) navigationReferences.Clear();
		int fullIndex = 0;
		for (int i = 0; i < elementGroups.Length; i++)
		{
			for (int j = 0; j < elementGroups[i].ElementsToMap.Count; j++)
			{
				
				if (RedoNavigationResources)
					navigationReferences.Add(new() { containerID = i, parentMapper = this });
				else
					navigationReferences[fullIndex].containerID = i;
					if(navigationReferences[fullIndex].parentMapper == null) navigationReferences[fullIndex].parentMapper = this;
					
				fullIndex++;
			}
		}

	}
	bool AnyGroupsHaveEmpty(bool canForceWarning)
	{
		bool emptyFound = false;
		string type = "";
		int warningParentIndex = -1;
		for (int i = 0; i < elementGroups.Length; i++)
		{

			if (elementGroups[i] == null || elementGroups[i].ElementsToMap.Count <= 0)
			{
				emptyFound = true;
				warningParentIndex = i;
				type = "";
				break;
			}

			else
			{
				for (int e = 0; e < elementGroups[i].ElementsToMap.Count; e++)
				{
					if (elementGroups[i].ElementsToMap[e] == "")
					{
						emptyFound = true;
						warningParentIndex = i;
						type = $", child index {e}";
						break;
					}
				}
			}
		}
		if (emptyFound && (!emptyWarningPrinted || canForceWarning))
		{
			emptyWarningPrinted = true;
			GD.PrintErr($"Empty found in container group index {warningParentIndex}{type}");
		}
		if (!emptyFound && emptyWarningPrinted) emptyWarningPrinted = false;

		return emptyFound;
	}
	void MapElements()
	{

		if (AnyGroupsHaveEmpty(true)) return;
		else
		{
			DebugPrint("Starting navigation bake");

			//Rebuild custom path reference resources with group ID & origin references
			if (autoUpdate == AutoUpdateTypes.All)
				CheckAndResetDirectionResources();

			//Check group & other relevant counts and adjust visualizer group-counts accordingly
			CheckSetVisualizerParents();
			SubContainerToGroupsCountCheck();

			//element to element within group
			switch (autoUpdate)
			{
				case AutoUpdateTypes.All:
					ElementNavigationMapping();
					break;
				case AutoUpdateTypes.Visualization:
					MatchToManualAndAddArrows();
					break;
			}
			//group to group, maps even with update set to only visualization
			GroupNavigationMapping();

			//add self to any empty built in focus reference to work around unwanted, unmapped navigation
			if (setBaseGodotFocus)
				SelfToEmpty();
			SetFocusModeToAll();

			// Since there's currently no custom logic for next/previous
			// I'm simply disabling it for the time being to not break the idea of the groups too much
			DisableNextPrevious();
		}
	}

	void DisableNextPrevious()
	{
		Control element;
		for (int i = 0; i < navigationReferences.Count; i++)
		{
			element = GetNode<Control>(navigationReferences[i].origin);
			if (!allowBaseGodotNextAndPrevious)
			{
				element.FocusNext = ".";
				element.FocusPrevious = ".";
			}
			else
			{
				element.FocusNext = "";
				element.FocusPrevious = "";
			}
		}
	}

	void SetFocusModeToAll()
	{
		Control element;
		for (int i = 0; i < navigationReferences.Count; i++)
		{
			element = GetNode<Control>(navigationReferences[i].origin);
			if (setBaseGodotFocus) element.FocusMode = Control.FocusModeEnum.All;
			else 					element.FocusMode = Control.FocusModeEnum.None;

		}
	}

	void MatchToManualAndAddArrows()
	{
		int totalIndex = 0;
		totalConnectionCount = 0;
		currentWholeIndex = 0;
		for (int i = 0; i < elementGroups.Length; i++)
		{
			for (int e = 0; e < elementGroups[i].ElementsToMap.Count; e++)
			{
				Control node = GetNode<Control>(elementGroups[i].ElementsToMap[e]);
				node.FocusNeighborTop = null;
				node.FocusNeighborBottom = null;
				node.FocusNeighborLeft = null;
				node.FocusNeighborRight = null;
				CheckForOutOfGroupReference(totalIndex, i);
				ReApplyReferenceAndCalculateForAlreadyKnownTarget(node, totalIndex);
				totalIndex++;
			}
			

			Control[] mappable = CreateToBeMappedArray(i);
			PointerGeneration(mappable, pointerParent.GetChild<Control>(i));
			SetGroupVisualizationBox(mappable, i);

			//hitpoints, pointer index counter & connection count
			ClearPointerListsAndValues();
		}
	}

	//Clears out of group references to for adding them back
	void CheckForOutOfGroupReference(int totalIndex, int groupIndex)
	{
		foreach (Vector2 direction in directions)
		{
			Control checkedNode = GetNodeOrNull<Control>(GetNavReferencePath(direction,totalIndex));

			if (checkedNode != null){
				bool grouphasnode = GroupContainsNode(checkedNode, elementGroups[groupIndex].ElementsToMap);
				if (!grouphasnode)
				{
					navigationReferences[totalIndex].SetDirectionReferences(direction, null, true);
				}
			}
		}
	}

	bool GroupContainsNode(Control comparedNode, Godot.Collections.Array<NodePath> pathPile)
	{
		bool isFound = false;
		for (int i = 0; i < pathPile.Count; i++)
		{
			if(comparedNode == GetNodeOrNull(pathPile[i])) {
				isFound = true;
				break;
			}
		}
		return isFound;
	}

	void ReApplyReferenceAndCalculateForAlreadyKnownTarget(Control currentOriginNode, int totalIndex)
	{
		foreach (Vector2 direction in directions)
		{
			Control target = null;
			switch (direction)
			{
				case Vector2(0.0f, -1.0f): target = GetNodeOrNull<Control>(navigationReferences[totalIndex].Up); break;
				case Vector2(0.0f, 1.0f): target = GetNodeOrNull<Control>(navigationReferences[totalIndex].Down); break;
				case Vector2(-1.0f, 0.0f): target = GetNodeOrNull<Control>(navigationReferences[totalIndex].Left); break;
				case Vector2(1.0f, 0.0f): target = GetNodeOrNull<Control>(navigationReferences[totalIndex].Right); break;
			}
			if (target != null && target != currentOriginNode)
			{
				totalConnectionCount++;
				//repeat point checks & calculations ***only*** for known selected target node and calculate best point for arrows
				AddNewPointsToLists(target, direction);
				CalculateComparisonValues(currentOriginNode, direction);
				SelectBestDirectionalCandidate(180f, false, direction);
				pointHitPosition.Add(finalSelectedPointPosition);
				ClearDirectionalCheckListsAndValues();
			}

			if (setBaseGodotFocus)
			{
				NodePath originToSelected = target == null ? null : currentOriginNode.GetPathTo(target);
				switch (direction)
				{
					case Vector2(0.0f, -1.0f): currentOriginNode.FocusNeighborTop = originToSelected; break;
					case Vector2(0.0f, 1.0f): currentOriginNode.FocusNeighborBottom = originToSelected; break;
					case Vector2(-1.0f, 0.0f): currentOriginNode.FocusNeighborLeft = originToSelected; break;
					case Vector2(1.0f, 0.0f): currentOriginNode.FocusNeighborRight = originToSelected; break;
				}
			}
			else
			{
				switch (direction)
				{
					case Vector2(0.0f, -1.0f): currentOriginNode.FocusNeighborTop = "."; break;
					case Vector2(0.0f, 1.0f): currentOriginNode.FocusNeighborBottom = "."; break;
					case Vector2(-1.0f, 0.0f): currentOriginNode.FocusNeighborLeft = "."; break;
					case Vector2(1.0f, 0.0f): currentOriginNode.FocusNeighborRight = "."; break;
				}
			}
		}
	}

	Control[] CreateToBeMappedArray(int groupIndex)
	{
		List<Control> mappable = [];
		foreach (NodePath elementPath in elementGroups[groupIndex].ElementsToMap) mappable.Add(GetNode<Control>(elementPath));
		return [.. mappable];
	}

	Control[] CreateGroupMappedArray()
	{
		List<Control> mappable = [];
		for (int i = 0; i < elementGroups.Length; i++)
		{
			mappable.Add(containerParent.GetChild<Control>(i));
		}
		return [.. mappable];
	}

	void ElementNavigationMapping()
	{
		currentWholeIndex = 0;
		for (int i = 0; i < elementGroups.Length; i++)
		{
			int indexAtIterationStart = currentWholeIndex;

			Control[] mappable = CreateToBeMappedArray(i);
			NavigationMappingLogic(mappable);

			//reset changes made to whole-index check value for pointer generation
			currentWholeIndex = indexAtIterationStart;
			
			PointerGeneration(mappable, pointerParent.GetChild<Control>(i));

			SetGroupVisualizationBox(mappable, i);

			ClearPointerListsAndValues();
		}
	}

	void NavigationMappingLogic(Control[] mappable)
	{
		for (int e = 0; e < mappable.Length; e++)
		{
			int elementIndex = e;
			Control currentOriginNode = mappable[elementIndex];
			navigationReferences[currentWholeIndex].origin = GetPathTo(currentOriginNode);

			foreach (Vector2 direction in directions)
			{
				DebugPrint(currentOriginNode.Name + " " + PrintDirection(direction));

				ListAllViableTargetPoints(currentOriginNode, direction, mappable);
				CalculateComparisonValues(currentOriginNode, direction); 
				SelectBestDirectionalCandidate(maximumTargetAngle, false, direction);
				SetFinalReferences(currentOriginNode, direction, false);

				ClearDirectionalCheckListsAndValues();
				DebugPrint("-------------------------------------------------");
			}
			currentWholeIndex++;
		}
	}

	void GroupNavigationMapping()
	{
		//group to group navigation
		Control[] mappableGroups = CreateGroupMappedArray();
		for (int i = 0; i < mappableGroups.Length; i++)
		{
			Control currentOriginNode = mappableGroups[i];
			foreach (Vector2 direction in directions)
			{
				currentWholeIndex = -1;
				ListAllViableTargetPoints(currentOriginNode, direction, mappableGroups);
				CalculateComparisonValues(currentOriginNode, direction);
				SelectBestDirectionalCandidate(maximumGroupTargetAngle, true, direction);
				SetGroupPointers(direction, currentOriginNode);
				int countInGroup = elementGroups[i].ElementsToMap.Count;
				for (int e = 0; e < countInGroup; e++) 
				{
					//check which reference to start going up from from if index isnt set
					if (currentWholeIndex == -1) {
						for (int p = 0; p < navigationReferences.Count; p++) {
							
							if (MatchPathToPathViaNode(navigationReferences[p].origin, elementGroups[i].ElementsToMap[e])){
								currentWholeIndex = p; 
								break;
							}
						}
					}
					else //else increment
						currentWholeIndex++;
					SetFinalReferences(GetNode<Control>(elementGroups[i].ElementsToMap[e]), direction, true); 
				}
				ClearDirectionalCheckListsAndValues();
			}
		}
		DeleteStragglerGroupPointers();
		ClearPointerListsAndValues();
		currentTotalGroupConnections = 0;
	}

	void SelfToEmpty()
	{
		for (int i = 0; i < elementGroups.Length; i++)
		{
			Control[] mappable = CreateToBeMappedArray(i);
			for (int e = 0; e < mappable.Length; e++)
			{
				foreach (Vector2 direction in directions)
				{
					switch (direction)
					{
						//up
						case Vector2(0.0f, -1.0f):
							if (mappable[e].FocusNeighborTop == null || mappable[e].FocusNeighborTop == "")
								mappable[e].FocusNeighborTop = ".";
							break;

						//down
						case Vector2(0.0f, 1.0f):
							if (mappable[e].FocusNeighborBottom == null || mappable[e].FocusNeighborBottom == "")
								mappable[e].FocusNeighborBottom = ".";
							break;

						//left
						case Vector2(-1.0f, 0.0f):
							if (mappable[e].FocusNeighborLeft == null || mappable[e].FocusNeighborLeft == "")
								mappable[e].FocusNeighborLeft = ".";
							break;

						//right
						case Vector2(1.0f, 0.0f):
							if (mappable[e].FocusNeighborRight == null || mappable[e].FocusNeighborRight == "")
								mappable[e].FocusNeighborRight = ".";
							break;
					}
				}
			}
		}
	}

	void DeleteStragglerGroupPointers()
	{
		int childCount = groupPointerParent.GetChildCount();
		for (int i = 0; i < childCount; i++)
			if (i >= currentTotalGroupConnections) { groupPointerParent.GetChild(i).QueueFree(); }
	}

	bool MatchPathToPathViaNode(NodePath one, NodePath two)
	{	
		return GetNode<Control>(one) == GetNode<Control>(two);
	}
	
	Vector2 CalculateArrowTarget(Vector2 dir, Control node)
	{

        return dir switch
        {
            //up
            Vector2(0.0f, -1.0f) => node.GlobalPosition + new Vector2(node.Size.X / 2.0f, 0f),
            //down
            Vector2(0.0f, 1.0f) => node.GlobalPosition + new Vector2(node.Size.X / 2.0f, node.Size.Y),
            //left
            Vector2(-1.0f, 0.0f) => node.GlobalPosition + new Vector2(0f, node.Size.Y / 2.0f),
            //right
            Vector2(1.0f, 0.0f) => node.GlobalPosition + new Vector2(node.Size.X, node.Size.Y / 2.0f),
			
            _ => node.GlobalPosition + new Vector2(0f, 0f),
        };
    }

	void PointerGeneration(Control[] mappable, Control subParent)
	{
		
		preExistingPointerCounts = subParent.GetChildCount();
		pointerDifference = totalConnectionCount - preExistingPointerCounts;

		for (int e = 0; e < mappable.Length; e++)
		{
			Control currentOriginNode = mappable[e];
			foreach (Vector2 direction in directions)
			{
				Control target = GetNodeOrNull<Control>(GetNavReferencePath(direction, currentWholeIndex));

				if (target != null) 
					SetPointerArrows(target, currentOriginNode, direction, subParent);
			}
			currentWholeIndex++;
		}

		if (pointerDifference < 0) //remove excess pointers after setting required ones
		{
			for (int e = 0; e < subParent.GetChildCount(); e++){
				if (e >= totalConnectionCount) { subParent.GetChild(e).QueueFree(); }
			}
		}
	}

	void SetPointerArrows(Control target, Control currentOriginNode, Vector2 direction, Control subParent)
	{
		if (target == null) return;
		SpatNavPointer pointer;

		if (pointerDifference > 0)
		{
			pointer = new();
			subParent.AddChild(pointer);
			subParent.MoveChild(pointer, currentPointerIndex);
			pointer.Owner = GetTree().EditedSceneRoot;
			pointer.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
			pointerDifference--;
		}
		else { pointer = subParent.GetChild<SpatNavPointer>(currentPointerIndex); }

		if (pointer != null)
		{
			pointer.GlobalPosition = pointHitPosition[currentPointerIndex];
			CalculatePointerDirAndPos(0, direction, currentOriginNode, pointer);
		}
		else { GD.PrintErr("Pointer reference error"); }
		currentPointerIndex++;
	}

	void SetGroupPointers(Vector2 direction, Control currentOriginNode)
	{
		if (currentSelectedGroupContainer == null) return;

		SpatNavPointer pointer;
		if (groupPointerParent.GetChildCount() <= currentTotalGroupConnections)
		{
			pointer = new();
			groupPointerParent.AddChild(pointer);
			groupPointerParent.MoveChild(pointer, currentTotalGroupConnections);
			pointer.Owner = GetTree().EditedSceneRoot;
		}
		else
		{
			pointer = groupPointerParent.GetChild<SpatNavPointer>(currentTotalGroupConnections);
		}
		currentTotalGroupConnections++;

		pointer.GlobalPosition = CalculateArrowTarget(direction * -1f, currentSelectedGroupContainer);
		CalculatePointerDirAndPos(1, direction, currentOriginNode, pointer);
	}

	void CalculatePointerDirAndPos(int type, Vector2 direction, Control currentOriginNode, SpatNavPointer pointer)
	{
		int directionIndex = GetDirectionIndex(direction);

		Vector2 directionVector = CalculateRelativeVector(CalculateArrowTarget(direction , currentOriginNode), pointer.GlobalPosition);
		Vector2 directionNormal = directionVector.Normalized();

		//pointer.GlobalPosition += directionVector; // * 0.4f
		pointer.GlobalPosition -= new Vector2(directionNormal.Y, -directionNormal.X) * 5f;

		pointer.SetPointerParameters(directionIndex, directionNormal, directionVector.Length(), CompileColorsToArray(type), pointerLetters);
	}
	
	void SetGroupVisualizationBox(Control[] mappable, int index)
	{
		NinePatchRect containerVisualizer = containerParent.GetChild<NinePatchRect>(index);

		if(containerVisualizer == null) { GD.PrintErr("ContainerVisualizer check returns a null"); return; }
		else
		{
			Vector2 smallest = new(999999f, 999999f);
			Vector2 largest = new(0f, 0f);
			for (int e = 0; e < mappable.Length; e++)
			{
				Control current = mappable[e];
				if (current.GlobalPosition.X < smallest.X) smallest.X = current.GlobalPosition.X;
				if (current.GlobalPosition.Y < smallest.Y) smallest.Y = current.GlobalPosition.Y;

				if (current.GlobalPosition.X + current.Size.X > largest.X) largest.X = current.GlobalPosition.X + current.Size.X;
				if (current.GlobalPosition.Y + current.Size.Y > largest.Y) largest.Y = current.GlobalPosition.Y + current.Size.Y;
			}

			containerVisualizer.GlobalPosition = smallest - new Vector2(groupBoxPadding, groupBoxPadding);
			containerVisualizer.Size = CalculateRelativeVector(largest, smallest) + new Vector2(groupBoxPadding, groupBoxPadding) * 2f;
			Color colorToUse = groupContainerColor;
			if (visualOverride != null) colorToUse = visualOverride.groupContainerColor;
			containerVisualizer.Modulate = colorToUse;
			containerVisualizer.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;

			Label label = containerVisualizer.GetChild<Label>(0);
			label.Text = $"Group {index}";
		}
	}
	
	void ClearDirectionalCheckListsAndValues()
	{
		pointPositions.Clear();
		pointParents.Clear();
		distances.Clear();
		finals.Clear();
		angles.Clear();
	}
	
	void ClearPointerListsAndValues()
	{
		pointHitPosition.Clear();
		totalConnectionCount = 0;
		currentPointerIndex = 0;
	}

	void ListAllViableTargetPoints(Control currentOriginNode, Vector2 direction, Control[] mappable)
	{
		for (int i = 0; i < mappable.Length; i++)
		{
			if (mappable[i] != currentOriginNode)
			{
				Vector2 currentCenter = CalculateControlCenter(currentOriginNode);
				Vector2 targetCenter = CalculateControlCenter(mappable[i]);

				if (CheckDirectionHit(direction, currentCenter, targetCenter))
					AddNewPointsToLists(mappable[i], direction);
			}
		}
	}

	void AddNewPointsToLists(Control currentTargetNode, Vector2 direction)
	{
		Vector2 y_centering = new(0f, currentTargetNode.Size.Y / 2.0f);
		Vector2 y_additive = new(currentTargetNode.Size.X / (checkResolution - 1f), 0f);

		Vector2 x_centering = new(currentTargetNode.Size.X / 2.0f, 0f);
		Vector2 x_additive = new(0f, currentTargetNode.Size.Y / (checkResolution - 1f));

		for (int k = 0; k < checkResolution; k++)
		{
			pointPositions.Add(CalculatePointPosition(currentTargetNode, k, direction, y_centering, x_centering, y_additive, x_additive));
			pointParents.Add(currentTargetNode);
		}
	}

	Vector2 CalculatePointPosition(Control currentTargetNode, int index, Vector2 direction, Vector2 y_centering, Vector2 x_centering, Vector2 y_additive, Vector2 x_additive)
	{
		bool isVertical = Mathf.Abs(direction.Y) == 1f;

		Vector2 centering = isVertical ? y_centering : x_centering;
		Vector2 additive = isVertical ? y_additive : x_additive;

		return currentTargetNode.GlobalPosition + centering + (additive * index);
	}

	Control GetCorrectElementIndexFromGroup(Control mappable, Vector2 direction)
	{
		Control element = null;
		for (int g = 0; g < elementGroups.Length; g++)
		{
			if (mappable == containerParent.GetChild<Control>(g)) {
				
				switch (direction)
				{
					//up
					case Vector2(0.0f, -1.0f):
						element = GetNode<Control>(elementGroups[g].ElementsToMap[elementGroups[g].BottomEntrySelectionIndex]);
						break;
					//down
					case Vector2(0.0f, 1.0f):
						element = GetNode<Control>(elementGroups[g].ElementsToMap[elementGroups[g].TopEntrySelectionIndex]);
						break;
					//left
					case Vector2(-1.0f, 0.0f):
						element = GetNode<Control>(elementGroups[g].ElementsToMap[elementGroups[g].RightEntrySelectionIndex]);
						break;
					//right
					case Vector2(1.0f, 0.0f):
						element = GetNode<Control>(elementGroups[g].ElementsToMap[elementGroups[g].LeftEntrySelectionIndex]);
						break;
				}
				break;
			}
		}
		return element;
	}

	bool CheckDirectionHit(Vector2 direction, Vector2 current, Vector2 target)
	{
        return direction switch
        {
            //up
            Vector2(0.0f, -1.0f) => current.Y > target.Y,
            //down
            Vector2(0.0f, 1.0f) => current.Y < target.Y,
            //left
            Vector2(-1.0f, 0.0f) => current.X > target.X,
            //right
            Vector2(1.0f, 0.0f) => current.X < target.X,
            _ => false,
        };
    }

	void CalculateComparisonValues(Control currentOriginNode, Vector2 direction)
	{
		for (int i = 0; i < pointPositions.Count; i++)
		{
			if(pointParents[i] != currentOriginNode)
			{
				Vector2 fromCurrentToTarget = pointPositions[i] - CalculateControlCenter(currentOriginNode);
				float dotProduct = direction.Dot(fromCurrentToTarget.Normalized());
				float distance = fromCurrentToTarget.Length();
				float angle = Mathf.RadToDeg(Mathf.Acos(dotProduct));
				//if(currentOriginNode.Name == "Button") { GD.Print($"angle: " + angle); GD.Print(distance); }
				angles.Add(angle);
				finals.Add(dotProduct / distance);
				distances.Add(distance);
			}
			else{
				//GD.Print("Point is on self, skipping");
			}
		}
	}
	
	void SelectBestDirectionalCandidate(float maxAngle, bool forGroup, Vector2 direction)
	{
		currentSelected = null;
		currentSelectedGroupContainer = null;
		currentHighestWeight = 0f;

		for (int i = 0; i < pointPositions.Count; i++)
		{
			if (angles[i] <= maxAngle && (distanceCutOff == -1 || distances[i] < distanceCutOff))
			{
				if (finals[i] > currentHighestWeight || currentSelected == null){
					
					currentHighestWeight = finals[i];
					finalSelectedPointPosition = pointPositions[i];
					if(forGroup) {
						currentSelectedGroupContainer = pointParents[i];
						currentSelected = GetCorrectElementIndexFromGroup(pointParents[i], direction);
					}
					else{
						currentSelected = pointParents[i];
					}
				}
			}
		}
	}
	
	void SetFinalReferences(Control currentOriginNode, Vector2 direction, bool forGroup)
	{
		NodePath originToSelected = currentSelected == null ? null : currentOriginNode.GetPathTo(currentSelected);
		NodePath thisToSelected = currentSelected == null ? null : GetPathTo(currentSelected);
		
		if(thisToSelected != null && thisToSelected != "" && !forGroup) {
			totalConnectionCount++;
			pointHitPosition.Add(finalSelectedPointPosition);
			DebugPrint($"Final selected for {currentOriginNode.Name}: {currentSelected.Name}");
		}
		
		switch (direction)
		{
			//up
			case Vector2(0.0f, -1.0f):
				if ((navigationReferences[currentWholeIndex].Up == null && thisToSelected != null) || !forGroup)
				{
					navigationReferences[currentWholeIndex].SetDirectionReferences(direction, thisToSelected, forGroup);
					if (setBaseGodotFocus)
						currentOriginNode.FocusNeighborTop = originToSelected;
					else currentOriginNode.FocusNeighborTop = ".";
				}
				break;
				
			//down
			case Vector2(0.0f, 1.0f):
				if ((navigationReferences[currentWholeIndex].Down == null && thisToSelected != null) || !forGroup)
				{
					navigationReferences[currentWholeIndex].SetDirectionReferences(direction, thisToSelected, forGroup);
					if (setBaseGodotFocus)
						currentOriginNode.FocusNeighborBottom = originToSelected;
					else currentOriginNode.FocusNeighborBottom = ".";
				}
				break;

			//left
			case Vector2(-1.0f, 0.0f):
				if ((navigationReferences[currentWholeIndex].Left == null && thisToSelected != null) || !forGroup)
				{
					navigationReferences[currentWholeIndex].SetDirectionReferences(direction, thisToSelected, forGroup);
					if (setBaseGodotFocus)
						currentOriginNode.FocusNeighborLeft = originToSelected;
					else currentOriginNode.FocusNeighborLeft = ".";
				}
				break;

			//right
			case Vector2(1.0f, 0.0f):
				if ((navigationReferences[currentWholeIndex].Right == null && thisToSelected != null) || !forGroup)
				{
					navigationReferences[currentWholeIndex].SetDirectionReferences(direction, thisToSelected, forGroup);
					if (setBaseGodotFocus)
						currentOriginNode.FocusNeighborRight = originToSelected;
					else currentOriginNode.FocusNeighborRight = ".";
				}
				break;
		}
	}
	
	NodePath GetNavReferencePath(Vector2 direction, int index)
	{
        return direction switch
        {
            //up
            Vector2(0.0f, -1.0f) => navigationReferences[index].Up,
            //down
            Vector2(0.0f, 1.0f) => navigationReferences[index].Down,
            //left
            Vector2(-1.0f, 0.0f) => navigationReferences[index].Left,
            //right
            Vector2(1.0f, 0.0f) => navigationReferences[index].Right,
            _ => (NodePath)"",
        };
    }

	void CheckSetVisualizerParents()
	{
		visualizationGrandparent = AddMissingParentControl(GetChildOrNull<Control>(0), "Visualization grandparent", 0);

		//If parent exists, the method simply passes it through, otherwise creates new one and assigns needed child position and values
		pointerParent = AddMissingParentControl(visualizationGrandparent.GetChildOrNull<Control>(0), "Pointer parent", 1);
		containerParent = AddMissingParentControl(visualizationGrandparent.GetChildOrNull<Control>(1), "Container parent", 2);
		groupPointerParent = AddMissingParentControl(visualizationGrandparent.GetChildOrNull<Control>(2), "Group pointer parent", 3);
	}
	
	Control AddMissingParentControl(Control parentNode, string desiredName, int type)
	{
		if (parentNode == null || parentNode.Name != desiredName)
		{
			parentNode = new(){
				Name = desiredName,
				GlobalPosition = new(0f, 0f)
			};

			if(type != 0){
				visualizationGrandparent.AddChild(parentNode);
				visualizationGrandparent.MoveChild(parentNode, type-1);
			}
			else{
				GD.Print("Adding new parent for visualization nodes! - It's recommended to *lock* and toggle *group selected nodes* on for 'Visualization grandparent' to make pointers & group-boxes as non-intrusive and non-selectable as possible.");
				AddChild(parentNode);
				MoveChild(parentNode, 0);
			}
			
			parentNode.Owner = GetTree().EditedSceneRoot;
		}
		return parentNode;
	}
	
	int ChildDifference(int count) { 
		return count - elementGroups.Length; 
	}
	
	void SubContainerToGroupsCountCheck()
	{
		Control[] parents = [pointerParent, containerParent];

		int[] childCounts = [parents[0].GetChildCount(), parents[1].GetChildCount()];
		int[] subParentDifferences = [ChildDifference(childCounts[0]), ChildDifference(childCounts[1])];

		//these aren't actually needed for group to group, thus only typeamount of 2, since they are just in one big pile & are checked elsewhere
		int typeAmount = 2;
		for (int i = 0; i < typeAmount; i++)
		{

			int childCount = childCounts[i];
			int differenceToUse = subParentDifferences[i];

			if (differenceToUse < 0)
			{
				for (int e = 0; e < Mathf.Abs(differenceToUse); e++)
				{
					switch (i)
					{
						case 0:
							AddPointerParentSubControls(e, childCount);
							break;
						case 1:
							AddContainerParentSubs(e, childCount);
							break;
						case 2:
							// group-to-group, were they checked here
							break;
					}
				}
			}

			else if (differenceToUse > 0)
			{
				for (int e = 0; e < childCount; e++)
					if (e > differenceToUse) {
						parents[i].GetChild(e).QueueFree();
					}
			}
		}
	}

	void AddPointerParentSubControls(int index, int existingCount)
	{
		Control parentChild = new();
		pointerParent.AddChild(parentChild);
		parentChild.Owner = GetTree().EditedSceneRoot;
		parentChild.Name = $"Pointers {index + existingCount}";
	}

	void AddContainerParentSubs(int index, int existingCount)
	{
		NinePatchRect parentChild = new();
		containerParent.AddChild(parentChild);
		parentChild.Owner = GetTree().EditedSceneRoot;
		parentChild.Texture = containerTex;
		parentChild.Name = $"Container of group {index + existingCount}";
		int containerPadding = 6;
		parentChild.PatchMarginTop = containerPadding;
		parentChild.PatchMarginBottom = containerPadding;
		parentChild.PatchMarginLeft = containerPadding;
		parentChild.PatchMarginRight = containerPadding;

		Label textLabel = new();
		parentChild.AddChild(textLabel);
		textLabel.Text = $"Group {index + existingCount}";
		textLabel.Owner = GetTree().EditedSceneRoot;
		textLabel.Position = CalculateLabelOffset();
		float size = visualOverride == null ? groupLabelSize : visualOverride.groupLabelSize;
		textLabel.Scale = new(size, size);
	}

	Vector2 CalculateLabelOffset()
	{
		float size = visualOverride == null ? groupLabelSize : visualOverride.groupLabelSize;
		return new(0f, -10f - 20f * (size - 0.5f));
	}

	string PrintDirection(Vector2 dir)
	{
        return dir switch
        {
            Vector2(0.0f, -1.0f) => "up",
            Vector2(0.0f, 1.0f) => "down",
            Vector2(-1.0f, 0.0f) => "left",
            Vector2(1.0f, 0.0f) => "right",
            _ => "",
        };
    }
	
	static Vector2 CalculateRelativeVector(Vector2 to, Vector2 from)
	{
		return to - from;
	}

	static float AngleFromPointToPoint(Vector2 from, Vector2 to)
	{
		Vector2 directionVector = to - from;
		return Mathf.RadToDeg(Mathf.Atan2(directionVector.Y, directionVector.X)) + 90f;
	}

	static Vector2 CalculateControlCenter(Control element)
	{
		if(element != null)
		{
			return element.GlobalPosition + element.Size / 2.0f;
		}
		else{
			return Vector2.Zero;
		}
		
	}

	int GetDirectionIndex(Vector2 direction)
	{
        return direction switch
        {
            //up
            Vector2(0.0f, -1.0f) => 0,
            //down
            Vector2(0.0f, 1.0f) => 1,
            //left
            Vector2(-1.0f, 0.0f) => 2,
            //right
            Vector2(1.0f, 0.0f) => 3,
            _ => -1,
        };
	}
	
	void DebugPrint(string log){ if(debug) GD.Print(log); }
}
