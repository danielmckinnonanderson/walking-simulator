using Godot;
using System;
using static Godot.GD;

public partial class World : Node3D
{
	public void _OnKillFloorEntered()
	{
		Print("Entered kill floor");
		Exit();
	}

	// Called when the player contacts the kill floor.
	private void Exit()
	{
		GetTree().Quit();
	}
}
