using Godot;
using System;

[Tool]
[GlobalClass]
public partial class NavMapVisualOverride : Resource
{
    [Export(PropertyHint.Range, "0,1.0,0.01,suffix:percent")] public float opacity = 1.0f;
    [Export] public Color upColor = new(0.89f, 0.34f, 0.18f, 1.00f);
    [Export] public Color downColor = new(0.00f, 0.62f, 1.00f, 1.00f);
    [Export] public Color leftColor = new(0.67f, 0.85f, 0.36f, 1.00f);
    [Export] public Color rightColor = new(0.73f, 0.22f, 0.60f, 1.00f);
    [Export] public Color groupContainerColor = new(1f, 0.765f, 0.0f, 1f);
    [Export] public Color groupUpColor = new(0.96f, 0.15f, 0.08f, 1.00f);
    [Export] public Color groupDownColor = new(0.00f, 0.30f, 1.00f, 1.00f);
    [Export] public Color groupLeftColor = new(0.39f, 0.93f, 0.59f, 1.00f);
    [Export] public Color groupRightColor = new(0.98f, 0.13f, 0.4f, 1.00f);
    [Export] public int GroupBoxPadding{
        get { return groupBoxPadding; }
        set
        {
            value = Math.Clamp(value, -5, 999);

            int change = value - groupBoxPadding;
            groupBoxPadding = value;
        }
    }
    int groupBoxPadding = 6;
    [Export] public float groupLabelSize = 0.5f;
    [Export] public Texture2D containerTex = GD.Load<Texture2D>("res://addons/Spatial navigation mapper/ContainerBorder.png");
}
