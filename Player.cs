using Godot;
using System;

public enum Gamemode { survival, creative };
public partial class Player : CharacterBody3D
{
	Vector2 mouseMove;
	Root root;
	Camera3D cam;
	[Export] public float Speed = 5.0f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public Gamemode gamemode = Gamemode.creative;
	public string[] inventory;
	float currTime = 0;
	float timeSinceSpacePressed = -1;
	bool spacePressed = false;
	bool flying = false;
	public void PickItem(DroppedItem item)
	{
		string name = item.ItemName;
		//item.QueueFree();
		for (int i = 0; i < inventory.Length; i++)
			if (inventory[i] == "") inventory[i] = name;
		root.PlaySound("pickItem", Position);
	}
	public override void _Ready()
	{
		inventory = new string[9];
		root = GetTree().CurrentScene as Root;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		mouseMove = new Vector2();
		cam = GetNode<Camera3D>("Cam");
	}
	public override void _Process(double delta)
	{
		base._Process(delta);
		float dt = (float)delta;
		currTime += dt;
		mouseMove = Input.GetLastMouseVelocity() / 10000f;
		if (Input.IsKeyPressed(Key.Escape))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GetTree().Quit();
		}
		if (Input.IsKeyPressed(Key.Space))
		{
			if (currTime - timeSinceSpacePressed <= .4f && !spacePressed && gamemode == Gamemode.creative)
				flying = !flying;
			timeSinceSpacePressed = currTime;
			spacePressed = true;
		}
		else spacePressed = false;
	}
	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		var ori = cam.Rotation;
		ori.Y -= mouseMove.X;
		ori.X -= mouseMove.Y;
		ori.X = Mathf.Clamp(ori.X, -Mathf.DegToRad(89.9f), Mathf.DegToRad(89.9f));
		Vector3 vel = Velocity;
		if (!IsOnFloor() && !flying) vel += GetGravity() * dt;
		else if (flying)
		{
			vel.Y = 0;
			if (Input.IsKeyPressed(Key.Space)) vel.Y = Speed;
			if (Input.IsKeyPressed(Key.Tab)) vel.Y = -Speed;
		}
		else if (IsOnFloor() && vel.Y < 0 && gamemode == Gamemode.survival)
		{
			//take damage
			vel.Y = 0;
		}
		if (Input.IsKeyPressed(Key.Space) && IsOnFloor())
			vel.Y = JumpVelocity;
		var fwd = -cam.GlobalTransform.Basis.Z;
		var right = cam.GlobalTransform.Basis.X;
		fwd.Y = right.Y = 0;
		fwd = fwd.Normalized();
		right = right.Normalized();
		if (Input.IsKeyPressed(Key.W))
		{
			var v = fwd * Speed;
			vel.X = v.X;
			vel.Z = v.Z;
		}
		if (Input.IsKeyPressed(Key.S))
		{
			var v = -fwd * Speed;
			vel.X = v.X;
			vel.Z = v.Z;
		}
		if (Input.IsKeyPressed(Key.A))
		{
			var v = -right * Speed;
			vel.X = v.X;
			vel.Z = v.Z;
		}
		if (Input.IsKeyPressed(Key.D))
		{
			var v = right * Speed;
			vel.X = v.X;
			vel.Z = v.Z;
		}
		if (!Input.IsKeyPressed(Key.W) && !Input.IsKeyPressed(Key.S) &&
			!Input.IsKeyPressed(Key.A) && !Input.IsKeyPressed(Key.D))
			vel.X = vel.Z = 0;
		Velocity = vel;
		MoveAndSlide();
		cam.Rotation = ori;
	}
	public override void _Input(InputEvent ev)
	{
		if (ev is InputEventKey k && k.Pressed && k.Keycode == Key.G)
		{
			gamemode = gamemode == Gamemode.survival ? Gamemode.creative : Gamemode.survival;
			GD.Print(gamemode == Gamemode.creative ? "creative" : "survival");
		}
	}
}
