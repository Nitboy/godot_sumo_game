using Godot;
using System;

public partial class Rikishi : CharacterBody2D
{
	private Vector2 _velocity = Vector2.Zero;
	private float _acceleration = 800f;
	private float _maxSpeed = 450f;
	private float _friction = 300f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		 Vector2 inputDirection = Vector2.Zero;

		if (Input.IsActionPressed("ui_up"))
			inputDirection.Y -= 1;
		if (Input.IsActionPressed("ui_down"))
			inputDirection.Y += 1;
		if (Input.IsActionPressed("ui_left"))
			inputDirection.X -= 1;
		if (Input.IsActionPressed("ui_right"))
			inputDirection.X += 1;

		// Normalize to prevent faster diagonal movement
		if (inputDirection != Vector2.Zero)
		{
			inputDirection = inputDirection.Normalized();
			_velocity += inputDirection * _acceleration * (float)delta;
		}
		else
		{
			// Apply friction when no input
			if (_velocity.Length() > 0)
			{
				Vector2 frictionForce = _velocity.Normalized() * _friction * (float)delta;
				_velocity -= frictionForce;

				if (_velocity.Dot(_velocity - frictionForce) < 0)
					_velocity = Vector2.Zero;
			}
		}

		// Clamp to max speed
		if (_velocity.Length() > _maxSpeed)
			_velocity = _velocity.Normalized() * _maxSpeed;
					
		//Position += _velocity * (float)delta;		
		this.Velocity = _velocity;

		
		 // Check for collision
		 var collision = MoveAndCollide(Velocity * (float)delta);
		if (collision != null)
		{
			GD.Print("My Velocity: " + 	this.Velocity );
			
			// Handle collision response
			// For example, you might want to push the other wrestler
			var collider = collision.GetCollider() as Node;
			if (collider != null && collider.IsInGroup("wrestler"))
			{
				// Apply force to the other wrestler
				var otherWrestler = collider as CharacterBody2D;
				if (otherWrestler != null)
				{
					// Get the collider's velocity
					var colliderVelocity = collision.GetColliderVelocity();
					var collisionAngle = collision.GetAngle();
					var cosCollisionAngle = Math.Abs((float)Math.Cos(collisionAngle));
					// Ensure minimum force transfer
					cosCollisionAngle = Math.Max(cosCollisionAngle, 0.1f);
					
					GD.Print("Collider Velocity: " + colliderVelocity);
					GD.Print("Collider Normal: " + collision.GetNormal() );

					GD.Print("Velocity Length: " + Velocity.Length() );
					GD.Print("collisionAngle: " + collisionAngle);
					GD.Print("(float)Math.Cos(collisionAngle): " + cosCollisionAngle);

					// Calculate the push force based on the collision angle and the collider's velocity
					var pushForce = -collision.GetNormal() * Velocity.Length() * cosCollisionAngle;
					var receivedpushForce = collision.GetNormal() * otherWrestler.Velocity.Length() * cosCollisionAngle;

					GD.Print("collisionAngle: " + collisionAngle );
					GD.Print("pushForce = resulting velocity: " + pushForce );
					otherWrestler.Velocity = (otherWrestler.Velocity * (1 - cosCollisionAngle)) + pushForce;										
					otherWrestler.MoveAndCollide(Velocity * (float)delta);
					
					Velocity = (Velocity * (1 - cosCollisionAngle)) + receivedpushForce;
					var bounceVelocity = Velocity.Bounce(collision.GetNormal()) * cosCollisionAngle;
					MoveAndCollide(bounceVelocity * (float)delta);
					
				}
			}
		}
	}
}
