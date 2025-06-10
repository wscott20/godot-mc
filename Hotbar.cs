using Godot;
using System;
using System.Linq;
//using System.Runtime.CompilerServices;
public struct HotbarItem(string n, int a)
{
	public int amount = a;
	public string name = n;
};
public partial class Hotbar : Sprite2D
{
	public string[] items = new string[9];
	PackedScene itemScene;
	Item[] itemArray = new Item[9];
	Label[] itemNums = new Label[9];
	public override void _EnterTree()
	{
		base._EnterTree();
		itemScene = ResourceLoader.Load<PackedScene>("res://item.tscn");
		for (int i = 0; i < 9; i++)
		{
			items[i] = "";
			itemNums[i] = GetNode<Label>($"{i}");
		}
	}
	public void SetItem(int slot, string name, int amount)
	{
		if (slot < 0 || slot > 8) return;
		if (name == "" && items[slot] != "")
		{
			itemArray[slot].QueueFree();
			items[slot] = "";
		}
		else if (items[slot] == name)
		{
			if (amount <= 0)
			{
				itemArray[slot].QueueFree();
				items[slot] = "";
				itemNums[slot].Text = "";
			}
			else itemNums[slot].Text = amount.ToString();
		}
		else
		{
			int i = Array.IndexOf(items, name);
			if (i == -1)
			{
				var item = itemScene.Instantiate<Item>();
				itemArray[slot] = item;
				item.BlockName = name;
				AddChild(item);
				item.Position = new Vector2(-76 + slot * 19, 0);
				items[slot] = name;
				itemArray[slot] = item;
				itemNums[slot].Text = amount.ToString();
			}
			else
			{
				if (amount == 0)
				{
					itemArray[slot].QueueFree();
					items[slot] = "";
					itemNums[slot].Text = "";
				}
				else itemNums[slot].Text = amount.ToString();
			}
		}
		for (int i = 0; i < 9; i++)
		{
			itemNums[i].Visible = itemNums[i].Text != "1";
			itemNums[i].ZIndex = 1;
		}
	}
	public HotbarItem? GetItem(int slot)
	{
		if (slot < 0 || slot > 8) return null;
		if (int.TryParse(itemNums[slot].Text, out int n)) return new(items[slot], n);
		else return null;
	}
}
