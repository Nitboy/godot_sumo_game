using Godot;
using System;

public partial class Main : Node
{
	private RigidBody2D eastWrestler;
	private RigidBody2D westWrestler;
	private Node dohyo;
	private CanvasLayer hud;
	private Marker2D markerEast;
	private Marker2D markerWest;

	private Area2D dohyoArea;

	private float acceleration = 800f;
	private float maxSpeed = 450f;
	private float friction = 300f;

	private Texture2D blueTexture;
	private Texture2D redTexture;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		dohyo = GetNode("Dohyo"); // Dohyo is instanced as Sprite2D
		hud = GetNode<CanvasLayer>("Hud");
		hud.Hide();
		markerEast = dohyo.GetNode<Marker2D>("MarkerEast");
		markerWest = dohyo.GetNode<Marker2D>("MarkerWest");

		var wrestlerScene = GD.Load<PackedScene>("res://rikishi_rigid.tscn");

		blueTexture = GD.Load<Texture2D>("res://Assets/sumoS2.png");
		redTexture = GD.Load<Texture2D>("res://Assets/sumoS1.png");

		eastWrestler = wrestlerScene.Instantiate<RigidBody2D>();
		westWrestler = wrestlerScene.Instantiate<RigidBody2D>();

		eastWrestler.Position = markerEast.GlobalPosition;
		westWrestler.Position = markerWest.GlobalPosition;

		AddChild(eastWrestler);
		AddChild(westWrestler);

		// Set blue texture for east wrestler
		var eastSprite = eastWrestler.GetNode<Sprite2D>("Sprite2D");
		eastSprite.Texture = blueTexture;

		// Set red texture for west wrestler
		var westSprite = westWrestler.GetNode<Sprite2D>("Sprite2D");
		westSprite.Texture = redTexture;

		// Get the DohyoArea node
		dohyoArea = dohyo.GetNode<Area2D>("DohyoArea");
		// Connect the body_exited signal
		dohyoArea.BodyExited += OnBodyExited;

		// Connect RestartButton
		var restartButton = hud.GetNode<Button>("ResultPanel/RestartButton");
		restartButton.Pressed += OnRestartButtonPressed;
	}

	private void OnBodyExited(Node body)
	{
		var resultLabel = hud.GetNode<Label>("ResultPanel/ResultLabel");
		var resultPanel = hud.GetNode<Panel>("ResultPanel");
		var stylebox = new StyleBoxFlat();

		if (body == eastWrestler)
		{
			GD.Print("East wrestler lost!");			
			resultLabel.Text = "WEST WINS!";       
			
			stylebox.BgColor = new Color(0.2f, 0.4f, 1); // Blue for west
		}
		else if (body == westWrestler)
		{
			GD.Print("West wrestler lost!");
			resultLabel.Text = "EAST WINS!";    
			stylebox.BgColor = new Color(1, 0.2f, 0.2f); // Red for east
		}
		
		// Set high linear damping for both wrestlers after game over
		eastWrestler.LinearDamp = 50f;
		westWrestler.LinearDamp = 50f;
		
		resultPanel.AddThemeStyleboxOverride("panel", stylebox);
		hud.Show();
	}

	private void OnRestartButtonPressed()
	{
		// Reload the current scene
		GetTree().ReloadCurrentScene();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		// East wrestler: WASD
		Vector2 eastInput = Vector2.Zero;
		if (Input.IsActionPressed("move_up")) eastInput.Y -= 1;
		if (Input.IsActionPressed("move_down")) eastInput.Y += 1;
		if (Input.IsActionPressed("move_left")) eastInput.X -= 1;
		if (Input.IsActionPressed("move_right")) eastInput.X += 1;

		if (eastInput != Vector2.Zero)
		{
			eastInput = eastInput.Normalized();
			if (eastWrestler.LinearVelocity.Length() < maxSpeed)
			{
				eastWrestler.ApplyCentralImpulse(eastInput * acceleration * (float)delta);
			}
		}			

		// West wrestler: Arrow keys
		Vector2 westInput = Vector2.Zero;
		if (Input.IsActionPressed("ui_up")) westInput.Y -= 1;
		if (Input.IsActionPressed("ui_down")) westInput.Y += 1;
		if (Input.IsActionPressed("ui_left")) westInput.X -= 1;
		if (Input.IsActionPressed("ui_right")) westInput.X += 1;

		if (westInput != Vector2.Zero)
		{
			westInput = westInput.Normalized();
			if (westWrestler.LinearVelocity.Length() < maxSpeed)
			{
				westWrestler.ApplyCentralImpulse(westInput * acceleration * (float)delta);
			}
		}
	}
}
