using Godot;
using System;

public partial class DroppedItem : Node3D
{
	Sprite3D item;
	Root root;
	Camera3D cam;
	Player player;
	Area3D collider;
	CharacterBody3D rb;
	string name;
	public string pendingItemName;
	public string ItemName
	{
		get => name;
		set
		{
			name = value;
			if (item != null)
			{
				item.Texture = ResourceLoader.Load<Texture2D>($"res://textures/items/{name}.png");
				item.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
			}
			else pendingItemName = name;
		}
	}
	public override void _EnterTree()
	{
		root = GetTree().CurrentScene as Root;
		player = root.GetNode<Player>("Player");
		rb = GetNode<CharacterBody3D>("Body");
		cam = player.GetNode<Camera3D>("Cam");
		collider = GetNode<Area3D>("Collider");
		item = collider.GetNode<Sprite3D>("Item");
		if (pendingItemName != null)
		{
			ItemName = pendingItemName;
			pendingItemName = null;
		}
		collider.BodyEntered += OnBodyEntered;
	}
	public void OnBodyEntered(Node3D node)
	{
		if (node is Player p)
		{
			Pick(p);
		}
	}
	public void Pick(Player p)
	{
		p.PickItem(this);
		QueueFree();
	}
	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector3 pos = item.Position;
		pos.Y = Mathf.Sin(dt);
		item.Position = pos;
		if (Input.IsKeyPressed(Key.P)) root.PlaySound("pickItem", player.Position);
		var dir = cam.GlobalPosition - item.GlobalPosition;
		dir.Y = 0;
		dir = dir.Normalized();
		var r = item.Rotation;
		r.Y = Mathf.Atan2(dir.X, dir.Z);
		item.Rotation = r;
		collider.Position = rb.Position + Vector3.Up * .01f;
	}
}
