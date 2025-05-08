# Godot Sumo Game

A 2D sumo wrestling game where two wrestlers battle to push each other out of the ring. Created with Godot Mono 4.4.1.

![Sumo Game](https://github.com/Nitboy/godot_sumo_game/assets/screenshot.png)

## Requirements

- Godot Mono 4.4.1 or newer
- .NET SDK 6.0 or newer

## Installation & Running

### 1. Install Godot Mono 4.4.1

1. Download Godot Mono 4.4.1 from the [official Godot website](https://godotengine.org/download/archive/) or [GitHub releases](https://github.com/godotengine/godot/releases/tag/4.4.1-stable)
2. Extract the downloaded archive to a location of your choice
3. Run the Godot executable

### 2. Install .NET SDK

1. Download and install .NET SDK 6.0 or newer from [Microsoft's .NET download page](https://dotnet.microsoft.com/en-us/download)
2. Verify installation by running `dotnet --version` in a terminal

### 3. Run the Game

#### Option 1: Using Godot Editor
1. Open Godot Mono 4.4.1
2. Click "Import" and select the `project.godot` file from this repository
3. Once the project is loaded, click the "Play" button in the top-right corner or press F5

#### Option 2: From Command Line
```bash
/path/to/godot --path /path/to/godot_sumo_game
```

## How to Play

### Controls
- **East Wrestler (Blue)**: 
  - W: Move up
  - A: Move left
  - S: Move down
  - D: Move right

- **West Wrestler (Red)**:
  - ↑: Move up
  - ←: Move left
  - ↓: Move down
  - →: Move right

### Gameplay
1. Two wrestlers start on opposite sides of the dohyo (sumo ring)
2. Use the controls to move your wrestler
3. Push your opponent out of the ring to win
4. After a winner is declared, press the "RESTART" button to play again

## Development

This game was developed using C# in Godot Mono. Key files:
- `Main.cs`: Controls game flow, input handling, and scene setup
- `rikishi_rigid.tscn`: The wrestler scene using RigidBody2D for physics
- `dohyo.tscn`: The sumo ring

## License

This project is licensed under the MIT License - see the LICENSE file for details. 