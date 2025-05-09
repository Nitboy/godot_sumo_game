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
	private Marker2D centerMarker; // Center marker for visualization

	private Area2D dohyoArea;
	private WrestlerBot westBot; // Bot controlling west wrestler

	private float acceleration = 800f;
	private float maxSpeed = 550f;
	private float friction = 300f;

	private Texture2D blueTexture;
	private Texture2D redTexture;
	
	private Vector2 dohyoCenter;
	
	// Input method for west wrestler
	private enum WestInputMethod
	{
		Arrows,
		Numpad,
		Controller,
		Bot
	}
	
	private WestInputMethod currentWestInputMethod = WestInputMethod.Numpad;
	
	// Current bot strategy label
	private Label botStrategyLabel;
	
	// Game state tracking
	private bool gameOver = false;
	private bool canResetMatch = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		dohyo = GetNode("Dohyo"); // Dohyo is instanced as Sprite2D
		hud = GetNode<CanvasLayer>("Hud");
		hud.Hide();
		markerEast = dohyo.GetNode<Marker2D>("MarkerEast");
		markerWest = dohyo.GetNode<Marker2D>("MarkerWest");
		
		// Calculate dohyo center as the midpoint between the east and west markers
		dohyoCenter = (markerEast.GlobalPosition + markerWest.GlobalPosition) / 2;
		
		// Create a visual marker for the center (for debugging)
		centerMarker = new Marker2D();
		centerMarker.GlobalPosition = dohyoCenter;
		AddChild(centerMarker);
		
		// Add a visible ColorRect to the center marker
		var centerVisual = new ColorRect();
		centerVisual.Color = new Color(1, 0, 0, 0.5f); // Semi-transparent red
		centerVisual.Size = new Vector2(20, 20); // 20x20 pixels
		centerVisual.Position = new Vector2(-10, -10); // Center the rect
		centerMarker.AddChild(centerVisual);

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
		
		// Setup the WrestlerBot for west wrestler
		westBot = new WrestlerBot();
		AddChild(westBot);
		westBot.Initialize(westWrestler, eastWrestler, dohyoCenter);
		
		// Calculate and set ring radius (distance from center to markers)
		float eastRadius = (markerEast.GlobalPosition - dohyoCenter).Length();
		float westRadius = (markerWest.GlobalPosition - dohyoCenter).Length();
		float avgRadius = (eastRadius + westRadius) / 2;
		westBot.SetRingRadius(avgRadius);
		
		// Debug log radius
		GD.Print("Ring Radius: ", avgRadius);
		
		// Create bot strategy label
		CreateBotStrategyLabel();
		
		// Debug log center position
		GD.Print("Dohyo Center Position: ", dohyoCenter);
	}
	
	private void CreateBotStrategyLabel()
	{
		botStrategyLabel = new Label();
		botStrategyLabel.Position = new Vector2(10, 10);
		botStrategyLabel.Text = "Bot Strategy: Chaser (1)";
		botStrategyLabel.Visible = false;
		
		// Add to canvas layer for UI
		var canvas = new CanvasLayer();
		canvas.Layer = 10; // Above other UI
		AddChild(canvas);
		canvas.AddChild(botStrategyLabel);
	}

	private void OnBodyExited(Node body)
	{
		if (gameOver) return; // Prevent multiple triggers
		
		gameOver = true;
		canResetMatch = true;
		
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
		ResetMatch();
	}
	
	// Reset wrestlers without reloading the whole scene
	private void ResetMatch()
	{
		// Hide the result panel
		hud.Hide();
		
		// Reset positions
		eastWrestler.Position = markerEast.GlobalPosition;
		westWrestler.Position = markerWest.GlobalPosition;
		
		// Reset velocities
		eastWrestler.LinearVelocity = Vector2.Zero;
		westWrestler.LinearVelocity = Vector2.Zero;
		
		// Reset dampening
		eastWrestler.LinearDamp = 0;
		westWrestler.LinearDamp = 0;
		
		// Reset rotation
		eastWrestler.Rotation = 0;
		westWrestler.Rotation = 0;
		
		// Reset angular velocity
		eastWrestler.AngularVelocity = 0;
		westWrestler.AngularVelocity = 0;
		
		// Reset forces using Godot methods
		eastWrestler.ConstantForce = Vector2.Zero;
		westWrestler.ConstantForce = Vector2.Zero;
		
		// Reset bot state
		westBot.ResetState();
		
		// Reset game state
		gameOver = false;
		canResetMatch = false;
		
		GD.Print("Match reset - wrestlers repositioned and bot state reset");
	}
	
	// Full game reset by reloading the scene
	private void ResetGame()
	{
		GetTree().ReloadCurrentScene();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Check for reset input
		if (canResetMatch && (Input.IsKeyPressed(Key.Space) || Input.IsKeyPressed(Key.Enter)))
		{
			ResetMatch();
		}
		
		// Switch input methods with function keys
		if (Input.IsKeyPressed(Key.F1))
			currentWestInputMethod = WestInputMethod.Arrows;
		else if (Input.IsKeyPressed(Key.F2))
			currentWestInputMethod = WestInputMethod.Numpad;
		else if (Input.IsKeyPressed(Key.F3))
			currentWestInputMethod = WestInputMethod.Controller;
		else if (Input.IsKeyPressed(Key.F4))
			currentWestInputMethod = WestInputMethod.Bot;
			
		// Show or hide bot strategy label
		botStrategyLabel.Visible = (currentWestInputMethod == WestInputMethod.Bot);
		
		// Switch bot strategies with number keys when in Bot input mode
		if (currentWestInputMethod == WestInputMethod.Bot)
		{
			if (Input.IsKeyPressed(Key.Key1) || Input.IsKeyPressed(Key.Kp1))
			{
				westBot.SetStrategy(WrestlerBot.BotStrategy.Chaser);
				botStrategyLabel.Text = "Bot Strategy: Chaser (1)";
			}
			else if (Input.IsKeyPressed(Key.Key2) || Input.IsKeyPressed(Key.Kp2))
			{
				westBot.SetStrategy(WrestlerBot.BotStrategy.Circler);
				botStrategyLabel.Text = "Bot Strategy: Circler (2)";
			}
			else if (Input.IsKeyPressed(Key.Key3) || Input.IsKeyPressed(Key.Kp3))
			{
				westBot.SetStrategy(WrestlerBot.BotStrategy.Controller);
				botStrategyLabel.Text = "Bot Strategy: Controller (3)";
			}
		}
		
		// Update bot timers
		westBot.UpdateTimers(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (gameOver) return;
		
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

		// West wrestler: Get input based on selected method
		Vector2 westInput = GetWestWrestlerInput();

		if (westInput != Vector2.Zero)
		{
			westInput = westInput.Normalized();
			if (westWrestler.LinearVelocity.Length() < maxSpeed)
			{
				westWrestler.ApplyCentralImpulse(westInput * acceleration * (float)delta);
			}
		}
	}
	
	private Vector2 GetWestWrestlerInput()
	{
		switch (currentWestInputMethod)
		{
			case WestInputMethod.Arrows:
				return GetArrowKeysInput();
			case WestInputMethod.Numpad:
				return GetNumpadInput();
			case WestInputMethod.Controller:
				return GetControllerInput();
			case WestInputMethod.Bot:
				return westBot.GetBotInput();
			default:
				return Vector2.Zero;
		}
	}
	
	private Vector2 GetArrowKeysInput()
	{
		Vector2 input = Vector2.Zero;
		if (Input.IsActionPressed("ui_up")) input.Y -= 1;
		if (Input.IsActionPressed("ui_down")) input.Y += 1;
		if (Input.IsActionPressed("ui_left")) input.X -= 1;
		if (Input.IsActionPressed("ui_right")) input.X += 1;
		return input;
	}
	
	private Vector2 GetNumpadInput()
	{
		Vector2 input = Vector2.Zero;
		// Main directions
		if (Input.IsKeyPressed(Key.Key8) || Input.IsKeyPressed(Key.Kp8)) input.Y -= 1; // Up
		if (Input.IsKeyPressed(Key.Key2) || Input.IsKeyPressed(Key.Kp2)) input.Y += 1; // Down
		if (Input.IsKeyPressed(Key.Key4) || Input.IsKeyPressed(Key.Kp4)) input.X -= 1; // Left
		if (Input.IsKeyPressed(Key.Key6) || Input.IsKeyPressed(Key.Kp6)) input.X += 1; // Right
		
		// Diagonals
		if (Input.IsKeyPressed(Key.Key7) || Input.IsKeyPressed(Key.Kp7)) { input.X -= 1; input.Y -= 1; } // Up-Left
		if (Input.IsKeyPressed(Key.Key9) || Input.IsKeyPressed(Key.Kp9)) { input.X += 1; input.Y -= 1; } // Up-Right
		if (Input.IsKeyPressed(Key.Key1) || Input.IsKeyPressed(Key.Kp1)) { input.X -= 1; input.Y += 1; } // Down-Left
		if (Input.IsKeyPressed(Key.Key3) || Input.IsKeyPressed(Key.Kp3)) { input.X += 1; input.Y += 1; } // Down-Right
		
		return input;
	}
	
	private Vector2 GetControllerInput()
	{
		Vector2 input = Vector2.Zero;
		
		// Left stick for main movement
		input.X = Input.GetJoyAxis(0, JoyAxis.LeftX);
		input.Y = Input.GetJoyAxis(0, JoyAxis.LeftY);
		
		// Apply deadzone
		if (Mathf.Abs(input.X) < 0.2f)
			input.X = 0;
		if (Mathf.Abs(input.Y) < 0.2f)
			input.Y = 0;
			
		// D-pad support
		if (Input.IsJoyButtonPressed(0, JoyButton.DpadUp))
			input.Y = -1;
		if (Input.IsJoyButtonPressed(0, JoyButton.DpadDown))
			input.Y = 1;
		if (Input.IsJoyButtonPressed(0, JoyButton.DpadLeft))
			input.X = -1;
		if (Input.IsJoyButtonPressed(0, JoyButton.DpadRight))
			input.X = 1;
			
		return input;
	}
}
