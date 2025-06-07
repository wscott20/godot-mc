using Godot;
using System;
using System.Linq;
//using System.Runtime.CompilerServices;

public partial class Hotbar : Sprite2D
{
	public string[] items = ["", "", "", "", "", "", "", "", ""];
	PackedScene itemScene;
	Item[] itemArray = new Item[9];
	public override void _EnterTree()
	{
		base._EnterTree();
		itemScene = ResourceLoader.Load<PackedScene>("res://item.tscn");
		//}
		//public override void _Ready()
		//{
		foreach (Item item in GetChildren().Cast<Item>())
		{
			int n = 0;
			switch (item.Position.X)
			{
				case -76: n = 0; break;
				case -57: n = 1; break;
				case -38: n = 2; break;
				case -19: n = 3; break;
				case 0: n = 4; break;
				case 19: n = 5; break;
				case 38: n = 6; break;
				case 57: n = 7; break;
				case 76: n = 8; break;
				default: break;
			}
			items[n] = item.BlockName;
			itemArray[n] = item;
		}
	}
	public void SetItem(int slot, string name)
	{
		if (slot < 0 || slot > 8) return;
		if (items[slot] != "")
			GD.Print(items[slot] + " replaced with " + name);
			//itemArray[slot].QueueFree();
		var item = itemScene.Instantiate<Item>();
		itemArray[slot] = item;
		item.BlockName = name;
		AddChild(item);
		item.Position = new Vector2(-76 + slot * 19, 0);
		items[slot] = name;
	}
	public string GetItem(int slot)
	{
		if (slot < 0 || slot > 8) return "";
		return items[slot];
	}
}
