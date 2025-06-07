using Godot;
using System;

public partial class Raycast : RayCast3D
{
	MeshInstance3D outline;
	Root root;
	Player player;
	Hotbar hotbar;
	public int currentItem = 0;
	static readonly float clickCd = .0f;//.35f;
	float rcTimer = 0, lcTimer = 0;
	public override void _EnterTree()
	{
		outline = GetTree().CurrentScene.GetNode<MeshInstance3D>("Outline");
		root = GetTree().CurrentScene as Root;
		player = root.GetNode<Player>("Player");
		hotbar = GetTree().CurrentScene.GetNode<Hotbar>("UI/Hotbar");
	}
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (IsColliding())
		{
			var collider = GetCollider();
			var norm = GetCollisionNormal();
			outline.GlobalPosition = (collider as StaticBody3D).GlobalPosition;
			var playerPos = GlobalPosition.Round() - Vector3.Up * 1.5f;
			if (Input.IsMouseButtonPressed(MouseButton.Right))
			{
				rcTimer -= (float)delta;
				if (rcTimer <= 0)
				{
					rcTimer = clickCd;
					var pos = outline.GlobalPosition + norm;
					string block = hotbar.GetItem(currentItem);
					if (
						pos != playerPos &&
						pos != playerPos + Vector3.Up &&
						block != ""
					)
					{
						var newBlock = root.SpawnBlock(pos, block, true);
						if (block == "log")
						{
							string normStr = $"{norm.X} {norm.Y} {norm.Z}";
							switch (normStr)
							{
								case "0 1 0":
								case "0 -1 0":
									newBlock.Rotation = Vector3.Zero;
									break;
								case "1 0 0":
								case "-1 0 0":
									newBlock.Rotation = new Vector3(0, 0, Mathf.Pi / 2);
									break;
								case "0 0 1":
								case "0 0 -1":
									newBlock.Rotation = new Vector3(Mathf.Pi / 2, 0, 0);
									break;
							}
						}
					}
				}
			}
			else rcTimer = 0;
			if (Input.IsMouseButtonPressed(MouseButton.Left))
			{
				lcTimer -= (float)delta;
				if (lcTimer <= 0)
				{
					root.BreakBlock(collider as Block, true, player.gamemode == Gamemode.survival);
					lcTimer = clickCd;
				}
			}
			else lcTimer = 0;
			outline.Visible = true;
		}
		else
		{
			outline.Visible = false;
			rcTimer = lcTimer = clickCd;
		}
	}
}
