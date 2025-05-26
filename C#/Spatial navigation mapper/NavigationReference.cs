using Godot;
using System;

[GlobalClass]
[Tool]
public partial class NavigationReference : Resource
{
    public NavMapper parentMapper;

    [Export] public int containerID;
    [Export] public NodePath origin;
    [Export]
    public NodePath Left
    {
        get { return left; }
        set
        {
            left = value;
            parentMapper?.QueueOnlyManualUpdate("left");
        }
    }
    NodePath left;

    [Export]
    public NodePath Up
    {
        get { return up; }
        set
        {
            up = value;
            parentMapper?.QueueOnlyManualUpdate("up");
        }
    }
    NodePath up;
   
    [Export]
    public NodePath Right
    {
        get { return right; }
        set
        {
            right = value;
            parentMapper?.QueueOnlyManualUpdate("right");
        }
    }
    NodePath right;

    [Export]
    public NodePath Down
    {
        get { return down; }
        set
        {
            down = value;
            parentMapper?.QueueOnlyManualUpdate("down");
        }
    }
    NodePath down;

    public void SetDirectionReferences(Vector2 direction, NodePath pathToTarget, bool GroupReference)
    {
        switch (direction)
        {
            //up
            case Vector2(0.0f, -1.0f):
                up = pathToTarget;
                break;

            //down
            case Vector2(0.0f, 1.0f):
                down = pathToTarget;
                break;

            //left
            case Vector2(-1.0f, 0.0f):
                left = pathToTarget;
                break;

            //right
            case Vector2(1.0f, 0.0f):
                right = pathToTarget;
                break;
        }
    }

}
