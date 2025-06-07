using Godot;
using System;

public partial class ItemPhysics : CharacterBody3D
{
	public override void _PhysicsProcess(double delta)
	{
		if (!IsOnFloor()) Velocity += GetGravity() * (float)delta;
		MoveAndSlide();
	}
}
