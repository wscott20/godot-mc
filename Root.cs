using Godot;
using System;
using System.Collections.Generic;

public partial class Root : Node3D
{
	PackedScene block, dropItem;
	AudioStream[] stone, dirt, grass, wood, explosion;
	Raycast raycast;
	Player player;
	string[] startingItems = ["cobblestone", "stone", "dirt", "leaves", "log", "tnt", "coalOre", "ironOre", "goldOre"];
	Sprite2D selectedItem;
	Node3D audioNodes, droppedItems;
	static int Rng(int min, int max) => GD.RandRange(min, max);
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Input.IsKeyPressed(Key.I)) PickAllItems();
	}
	public override void _EnterTree()
	{
		base._EnterTree();
		player = GetNode<Player>("Player");
		audioNodes = GetNode<Node3D>("AudioNodes");
	}
	public override void _Ready()
	{
		selectedItem = GetNode<Sprite2D>("UI/Node2D/SelectedItem");
		selectedItem.Position = new Vector2(-152, 0);
		raycast = GetNode<Raycast>("Player/Cam/Raycast");
		GD.Print(GetViewport().GetVisibleRect().Size);
		block = ResourceLoader.Load<PackedScene>("res://block.tscn");
		dropItem = ResourceLoader.Load<PackedScene>("res://dropped_item.tscn");
		stone = new AudioStream[4];
		dirt = new AudioStream[4];
		grass = new AudioStream[4];
		wood = new AudioStream[4];
		explosion = new AudioStream[4];
		for (int i = 0; i < 4; i++)
		{
			stone[i] = ResourceLoader.Load<AudioStream>($"res://sounds/stone/{i + 1}.ogg");
			dirt[i] = ResourceLoader.Load<AudioStream>($"res://sounds/dirt/{i + 1}.ogg");
			grass[i] = ResourceLoader.Load<AudioStream>($"res://sounds/grass/{i + 1}.ogg");
			wood[i] = ResourceLoader.Load<AudioStream>($"res://sounds/wood/{i + 1}.ogg");
			explosion[i] = ResourceLoader.Load<AudioStream>($"res://sounds/explosion/{i + 1}.ogg");
		}
		var hotbar = GetNode<Hotbar>("UI/Hotbar");
		for (int i = 0; i < startingItems.Length; i++)
			hotbar.SetItem(i, startingItems[i]);
		List<Block> blocks = new List<Block>();
		for (int x = -8; x <= 8; x++)
		{
			for (int z = -8; z <= 8; z++)
			{
				for (int y = 0; y < 16; y++)
				{
					string block;
					if (y == 0) block = "bedrock";
					//else if (y < 4) block = "cobblestone";
					else if (y < 11) block = "stone";
					else if (y < 15) block = "dirt";
					else block = "grass";
					blocks.Add(SpawnBlock(new Vector3(x, y, z), block));
				}
			}
		}
		SpawnTree(new(0, 16, 1));
		for (int i = 0; i < blocks.Count; i++)
		{
			var b = blocks[i];
			if (b.BlockName == "stone" && Rng(1, 100) <= 3)
			{
				blocks[i].BlockName = new[] { "coalOre", "ironOre", "goldOre" }[Rng(0, 2)];
			}
		}
		droppedItems = GetNode<Node3D>("DroppedItems");
	}
	public void DropItem(string name, Vector3 pos)
	{
		var item = dropItem.Instantiate<DroppedItem>();
		droppedItems.AddChild(item);
		item.ItemName = name;
		item.GlobalPosition = pos;
	}
	public void SpawnTree(Vector3 basePos)
	{
		var blocks = new Dictionary<Vector3, string>(){
			{new(0, 0, 0),"log"},
			{new(0, 1, 0),"log"},
			{new(0, 2, 0),"log"},
			{new(0, 3, 0),"log"},
		};
		for (int x = -2; x <= 2; x++)
			for (int z = -2; z <= 2; z++)
				blocks[new(x, 4, z)] = "leaves";
		for (int x = -2; x <= 2; x++)
			for (int z = -2; z <= 2; z++)
				if (Math.Abs(x) + Math.Abs(z) < 3) blocks[new(x, 5, z)] = "leaves";
		for (int x = -1; x <= 1; x++)
			for (int z = -1; z <= 1; z++)
				if (Math.Abs(x) + Math.Abs(z) < 2) blocks[new(x, 6, z)] = "leaves";
		blocks[new(0, 7, 0)] = "leaves";
		blocks[new(1, 7, 0)] = "leaves";
		blocks[new(-1, 7, 0)] = "leaves";
		blocks[new(0, 7, 1)] = "leaves";
		blocks[new(0, 7, -1)] = "leaves";
		foreach (var k in blocks.Keys)
		{
			var v = blocks[k];
			SpawnBlock(k + basePos, v);
		}

	}
	public Block SpawnBlock(Vector3 position, string name, bool playSound = false)
	{
		Block newBlock = block.Instantiate<Block>();
		AddChild(newBlock);
		newBlock.GlobalPosition = position;
		newBlock.BlockName = name;
		if (playSound) PlaySound(name, position);
		return newBlock;
	}
	public async void BreakBlock(Block block, bool playSound = false, bool drop = false)
	{
		if (block.BlockName == "tnt")
		{
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			Explode(block.GlobalPosition, 5, 4);
			block.QueueFree();
			if (playSound) PlaySound("explode", block.GlobalPosition);
			return;
		}
		if (drop) DropItem(block.BlockName, block.GlobalPosition);
		block.QueueFree();
		if (playSound) PlaySound(block.BlockName, block.GlobalPosition);
	}
	public async void Explode(Vector3 pos, float rds, float blastPower)
	{
		var sphereShape = new SphereShape3D { Radius = rds };
		var transform = new Transform3D(Basis.Identity, pos);
		var prms = new PhysicsShapeQueryParameters3D
		{
			Shape = sphereShape,
			Transform = transform,
			CollideWithAreas = false,
			CollideWithBodies = true
		};
		var results = GetWorld3D().DirectSpaceState.IntersectShape(prms, 600);
		foreach (var res in results)
		{
			var col = res["collider"].As<Node3D>();
			if (col is Block b && b.Position != pos)
			{
				float dist = b.GlobalPosition.DistanceTo(pos);
				float stepLen = 0.3f;
				int attenSteps = Mathf.FloorToInt(dist / stepLen);
				float requiredResistance = ((1.3f * blastPower) - (0.75f * attenSteps * stepLen)) / stepLen - 0.3f;
				if (b.blastResistance < requiredResistance)
				{
					if (b.BlockName == "tnt") await ToSignal(GetTree().CreateTimer(.1), "timeout");
					BreakBlock(b, b.BlockName == "tnt", player.gamemode == Gamemode.survival);
				}
			}
		}
	}
	public void PlaySound(string name, Vector3 pos)
	{
		var audio = new AudioStreamPlayer3D();
		AddChild(audio);
		audio.GlobalPosition = pos;
		int n = Rng(0, 3);
		try
		{
			switch (name)
			{
				case "bedrock":
				case "cobblestone":
				case "stone":
				case "coalOre":
				case "ironOre":
				case "goldOre":
					audio.Stream = stone[n];
					break;
				case "dirt":
					audio.Stream = dirt[n];
					break;
				case "grass":
				case "tnt":
				case "leaves":
					audio.Stream = grass[n];
					break;
				case "log":
				case "planks":
					audio.Stream = wood[n];
					break;
				case "explode":
					audio.Stream = new[] { wood, stone, dirt, grass }[Rng(0, 3)][n];
					break;
				case "pickItem":
					audio.Stream = ResourceLoader.Load<AudioStream>("res://sounds/itemPickup.ogg");
					//audio.PitchScale = new[] { 1 / (float)Rng(0, 4), Rng(1, 2) }[Rng(0, 1)];
					break;
				default: break;
			}
		}
		catch (Exception e)
		{
			GD.PrintErr(e);
			GD.Print(n);
		}
		audio.Play();
		audio.Finished += () => audio.QueueFree();
	}
	public override void _Input(InputEvent ev)
	{
		base._Input(ev);
		if (ev is InputEventMouseButton button && button.Pressed)
		{
			if (button.ButtonIndex == MouseButton.WheelDown)
			{
				raycast.currentItem = (raycast.currentItem + 1) % 9;
				selectedItem.Position = new Vector2(-152 + raycast.currentItem * 38, 0);
			}
			if (button.ButtonIndex == MouseButton.WheelUp)
			{
				raycast.currentItem = (raycast.currentItem - 1 + 9) % 9;
				selectedItem.Position = new Vector2(-152 + raycast.currentItem * 38, 0);
			}
		}
		//if (ev is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.I)
			//PickAllItems();
	}
	//`async
	void PickAllItems()
	{
		foreach (var item in droppedItems.GetChildren())
		{
			//await ToSignal(GetTree().CreateTimer(.01f), "timeout");
			(item as DroppedItem).Pick(player);
		}

	}
}
