using Godot;
using System;

[GlobalClass]
[Tool]
public partial class NavigationReference : Resource
{
    [Export] public int containerID;
    [Export] public NodePath origin;
    [Export] public NodePath left;
    [Export] public NodePath up;
    [Export] public NodePath right;
    [Export] public NodePath down;
    [Export] public NodePath next;
    [Export] public NodePath previous;
}
