using Godot;
using System;

[GlobalClass]
[Tool]
public partial class NavigationGroup : Resource
{
    [Export] public Godot.Collections.Array<NodePath> elementsToMap = [];
    public Vector2 boxPosition;
    public Vector2 boxSize;
}
