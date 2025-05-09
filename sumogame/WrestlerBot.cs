using Godot;
using System;

public partial class WrestlerBot : Node
{
    // Possible directional vectors (8 directions)
    private static readonly Vector2[] DirectionalVectors = new Vector2[]
    {
        new Vector2(0, -1),   // Up
        new Vector2(1, -1),   // Up-Right
        new Vector2(1, 0),    // Right
        new Vector2(1, 1),    // Down-Right
        new Vector2(0, 1),    // Down
        new Vector2(-1, 1),   // Down-Left
        new Vector2(-1, 0),   // Left
        new Vector2(-1, -1)   // Up-Left
    };

    // The controlled wrestler
    private RigidBody2D wrestler;
    // The opponent wrestler
    private RigidBody2D opponent;
    // Reference to the dohyo center
    private Vector2 dohyoCenter;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // We'll set these references from Main
    }

    public void Initialize(RigidBody2D controlledWrestler, RigidBody2D opponentWrestler, Vector2 center)
    {
        wrestler = controlledWrestler;
        opponent = opponentWrestler;
        dohyoCenter = center;
    }

    // Get the bot's input direction
    public Vector2 GetBotInput()
    {
        if (wrestler == null || opponent == null)
            return Vector2.Zero;

        // Get current positions
        Vector2 myPosition = wrestler.GlobalPosition;
        Vector2 opponentPosition = opponent.GlobalPosition;
        
        // Strategy 1: Move toward opponent
        Vector2 towardOpponent = (opponentPosition - myPosition).Normalized();
        
        // Strategy 2: If opponent is between us and the edge, push them out
        Vector2 opponentToCenter = dohyoCenter - opponentPosition;
        bool opponentNearEdge = opponentToCenter.Length() > 150; // Adjust threshold as needed
        
        // Final decision
        Vector2 desiredDirection;
        if (opponentNearEdge && (myPosition - dohyoCenter).Length() < (opponentPosition - dohyoCenter).Length())
        {
            // Push opponent toward edge
            desiredDirection = towardOpponent;
        }
        else
        {
            // Stay near center, move toward opponent carefully
            Vector2 towardCenter = (dohyoCenter - myPosition).Normalized();
            desiredDirection = (towardOpponent + towardCenter).Normalized();
        }
        
        // Find the closest of the 8 directional vectors
        return GetClosestDirectionalVector(desiredDirection);
    }
    
    // Find the closest of the 8 directional vectors to the desired direction
    private Vector2 GetClosestDirectionalVector(Vector2 desiredDirection)
    {
        float maxDot = -1;
        Vector2 bestVector = Vector2.Zero;
        
        foreach (Vector2 dirVector in DirectionalVectors)
        {
            float dot = dirVector.Normalized().Dot(desiredDirection);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestVector = dirVector;
            }
        }
        
        return bestVector;
    }
} 