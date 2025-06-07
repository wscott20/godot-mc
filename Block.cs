using Godot;
using System;

public partial class Block : StaticBody3D
{
	string name;
	public string BlockName
	{
		get => name;
		set
		{
			name = value;
			GetNode<MeshInstance3D>("Mesh").MaterialOverride = new StandardMaterial3D{
				AlbedoTexture = ResourceLoader.Load<Texture2D>($"res://textures/blocks/{name}.png"),
				TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest};
			blastResistance = name switch
			{
				"bedrock" => 18e6f,
				"cobblestone" => 6f,
				"stone" => 6f,
				"dirt" => .5f,
				"grass" => .6f,
				"log" => 2f,
				"tnt" => 0.5f,
				_ => 1f,
			};
		}
	}
	public float blastResistance;
}
