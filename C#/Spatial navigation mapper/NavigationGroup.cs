using Godot;
using System;
using System.Data;

[GlobalClass]
[Tool]
public partial class NavigationGroup : Resource
{
    public NavMapper parentMapper;

    [Export]
    public Godot.Collections.Array<NodePath> ElementsToMap
    {
        get { return elementsToMap; }
        set
        {
            elementsToMap = value;
            CheckEntryIndexLimits();
        }
    }
    Godot.Collections.Array<NodePath> elementsToMap = [];
    [ExportGroup("Entry selection indexes")]
    [Export]
    public int TopEntrySelectionIndex
    {
        get { return topEntrySelectionIndex; }
        set { topEntrySelectionIndex = value; CheckEntryIndexLimits(); }
    }
    int topEntrySelectionIndex;
    [Export]
    public int LeftEntrySelectionIndex
    {
        get { return leftEntrySelectionIndex; }
        set { leftEntrySelectionIndex = value; CheckEntryIndexLimits(); }
    }
    int leftEntrySelectionIndex;
    [Export]
    public int RightEntrySelectionIndex
    {
        get { return rightEntrySelectionIndex; }
        set { rightEntrySelectionIndex = value; CheckEntryIndexLimits(); }
    }
    int rightEntrySelectionIndex;
    [Export]
    public int BottomEntrySelectionIndex
    {
        get { return bottomEntrySelectionIndex; }
        set { bottomEntrySelectionIndex = value; CheckEntryIndexLimits(); }
    }
    int bottomEntrySelectionIndex;
    
    void CheckEntryIndexLimits()
    {
        leftEntrySelectionIndex = Math.Clamp(leftEntrySelectionIndex, 0, elementsToMap.Count - 1);
        rightEntrySelectionIndex = Math.Clamp(rightEntrySelectionIndex, 0, elementsToMap.Count - 1);
        bottomEntrySelectionIndex = Math.Clamp(bottomEntrySelectionIndex, 0, elementsToMap.Count - 1);
        topEntrySelectionIndex = Math.Clamp(topEntrySelectionIndex, 0, elementsToMap.Count - 1);
        parentMapper?.QueueMap();
    }
    
}
