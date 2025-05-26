using Godot;
using System;

[Tool]
public partial class SpatNavPointer : NinePatchRect
{
    public int directionIndex = -1;

    public override void _EnterTree()
    {
        if(GetChildCount() < 1){
            Label pointerLabel = new();
            AddChild(pointerLabel);
            pointerLabel.Owner = GetTree().EditedSceneRoot;
        }
        
        Texture = GD.Load<Texture2D>("res://addons/Spatial navigation mapper/arrow.png");
        RegionRect = new(0f, 0f, 21f, 16f);
        PatchMarginTop = 11;
    }

    public void SetPointerParameters(int receivingDirIndex, Vector2 directionVector, float distance, Color[] colors, string[] letters)
    {
        RotationDegrees = Mathf.RadToDeg(Mathf.Atan2(directionVector.Y, directionVector.X)) - 90f;
        Size = new(Texture.GetSize().X, distance+20.0f);
        directionIndex = receivingDirIndex;
        Modulate = colors[directionIndex];

        Label dirLabel = GetChild<Label>(0);

        dirLabel.Text = letters[directionIndex];
        dirLabel.RotationDegrees = -RotationDegrees;
        dirLabel.GlobalPosition = GlobalPosition + (directionVector * distance / 2.0f) - new Vector2(-directionVector.Y, directionVector.X) * 11f - dirLabel.Size / 2.0f;
    }

    public void SetPointerColor(Color[] colors)
    {
        Modulate = colors[directionIndex];
    }
}
