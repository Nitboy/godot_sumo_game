using Godot;
using System;

public partial class Rikishi2 : CharacterBody2D
{	

	private float _friction = 100f;

	public override void _PhysicsProcess(double delta)
	{	
		// Apply friction when no input
			//if (Velocity.Length() > 0)
			//{
				//Vector2 frictionForce = Velocity.Normalized() * _friction * (float)delta;
				//Velocity -= frictionForce;
//
				//if (Velocity.Dot(Velocity - frictionForce) < 0)
					//Velocity = Vector2.Zero;
			//}
	//
		//MoveAndSlide();
	}
}
