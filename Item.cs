using Godot;
using System;

public partial class Item : Sprite2D
{
	string name;
	public string BlockName {
		get => name;
		set {
			name = value;
			Texture = ResourceLoader.Load<Texture2D>($"res://textures/items/{value}.png");
			if (Texture == null)
				GD.PrintErr($"Item texture not found: res://textures/items/{value}.png");
		}
	}
}
