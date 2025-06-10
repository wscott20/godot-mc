using Godot;
using System;

public partial class HealthBar : Node2D
{
	Root root;
	Player player;
	Sprite2D[] hearts = new Sprite2D[10];
	int health = 20;
	Texture2D empty, half, full;
	public int Health
	{
		get => health;
		set
		{
			health = Mathf.Clamp(value, 0, 20);
			for (int i = 0; i < 10; i++)
			{
				int h = i * 2;
				if (health <= h) hearts[i].Texture = empty;
				else if (health == h + 1) hearts[i].Texture = half;
				else if (health >= h + 2) hearts[i].Texture = full;
			}
		}
	}
	public override void _EnterTree()
	{
		root = GetTree().CurrentScene as Root;
		player = root.GetNode<Player>("Player");
		for (int i = 0; i < 10; i++) hearts[i] = GetNode<Sprite2D>($"{i}");
		empty = ResourceLoader.Load<Texture2D>("res://textures/emptyHeart.png");
		half = ResourceLoader.Load<Texture2D>("res://textures/halfHeart.png");
		full = ResourceLoader.Load<Texture2D>("res://textures/heart.png");
	}
}
