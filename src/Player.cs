#nullable enable

using System;
using Godot;
using static Constants;


using Vector3 = Godot.Vector3;


public partial class Player : CharacterBody3D
{
	float gravity = (float) ProjectSettings.GetSetting("physics/3d/default_gravity");

    // Player settings
	[Export] float Speed = 5.0f;

	// Camera settings
	[Export] float CameraSensHori       =  0.032f;
	[Export] float CameraSensVert       =  0.032f;
	[Export] float CameraLimitUpper     =  1.00f; // radians
	[Export] float CameraLimitLower     = -0.20f; // radians
	[Export] bool  CameraFlipInputHori  =  true;
	[Export] bool  CameraFlipInputVert  =  true;

	// Node refs
	private Camera3D  camera;
	private Node3D    cameraPivotHori;
	private Node3D    cameraPivotVert;
	private Node3D    modelRoot;


	public override void _Ready()
	{
		camera          = GetNode<Camera3D>("CameraYPivot/CameraXPivot/Camera3D");
		cameraPivotHori = GetNode<Node3D>("CameraYPivot");
		cameraPivotVert = GetNode<Node3D>("CameraYPivot/CameraXPivot");
		modelRoot       = GetNode<Node3D>("Model");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Debug only - Exit if player presses 'back' button or 'ESC'
		if (Input.IsActionJustPressed(InputMapKeys.InGameDebugExit))
			GetTree().Quit();

		UpdateCameraPosition();

		if (!IsOnFloor())
			ApplyGravity(delta);
		
		ApplyMovementInput(delta);
		
		MoveAndSlide();
	}

	public void UpdateCameraPosition()
	{
		// Get input vector from r-stick
		var input = new Vector3
		{
			X = Input.GetAxis(InputMapKeys.InGameCameraLeft, InputMapKeys.InGameCameraRight),
			Y = Input.GetAxis(InputMapKeys.InGameCameraLower, InputMapKeys.InGameCameraHigher)
		};

		// Reverse inputs if settings to flip are true
		input.X = CameraFlipInputHori ? -input.X : input.X;
		input.Y = CameraFlipInputHori ? -input.Y : input.Y;
		
		// Moving up or down on r-stick rotates the X axis pivot around the player
		cameraPivotVert.RotateX(input.Y * CameraSensVert);
		// Moving left or right on r-stick rotates the Y axis pivot around the player (laterally)
		cameraPivotHori.RotateY(input.X * CameraSensHori);

		// Update rotation
		cameraPivotVert.Rotation = new Vector3 {
			// Clamp rotation based on limits
			X = Mathf.Clamp(cameraPivotVert.Rotation.X, CameraLimitLower, CameraLimitUpper),
			// Wrap horizontal rotation when angle above 360 or below 0
			Y = Mathf.Wrap(cameraPivotVert.Rotation.Y, 0.0f, 360.01f)
		};
	}

	public void ApplyGravity(double delta)
	{
		// Update velocity with effects of gravity over time
		Velocity = new Vector3 {
			Y = Velocity.Y - (gravity * (float) delta),
			X = Velocity.X,
			Z = Velocity.Z
		};
	}

	public void ApplyMovementInput(double _delta)
	{
		// Get input vector from left stick
		var input = Vector3.Zero;
		var leftStickInput = Input.GetVector(
            InputMapKeys.InGameMoveLeft,
            InputMapKeys.InGameMoveRight, 
            InputMapKeys.InGameMoveForward,
            InputMapKeys.InGameMoveBackward 
        );
		input.X = leftStickInput.X;
		input.Z = leftStickInput.Y;

		if (input == Vector3.Zero) {
			// No input so don't do anything
			Velocity = new Vector3 {
				X = 0.0f,
				Z = 0.0f,
				Y = Velocity.Y
			};
			return;
		}
		
		// Adjust inputs by basis
		Vector3 movementDir = TransformInputByCameraBasis(input);

		// Update velocity
		Velocity = new Vector3 {
			X = movementDir.X * Speed,
			Y = Velocity.Y,
			Z = movementDir.Z * Speed,
		};
	}

	private Vector3 TransformInputByCameraBasis(Vector3 rawInput)
	{
		return rawInput.Rotated(Vector3.Up, cameraPivotHori.Rotation.Y).Normalized();
	}
}
