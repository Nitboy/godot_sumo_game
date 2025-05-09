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

    // Bot strategy types
    public enum BotStrategy
    {
        Chaser,     // Always move toward the opponent
        Circler,    // Circle around the opponent to trick them
        Controller  // Control the center and push out
    }

    // Current bot strategy
    private BotStrategy currentStrategy = BotStrategy.Chaser;
    
    // The controlled wrestler
    private RigidBody2D wrestler;
    // The opponent wrestler
    private RigidBody2D opponent;
    // Reference to the dohyo center
    private Vector2 dohyoCenter;
    // Timer for changing direction when circling
    private float circleTimer = 0;
    // Current circling direction (clockwise or counter-clockwise)
    private bool circleClockwise = true;
    // Random number generator
    private Random random = new Random();

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
    
    // Set the bot strategy
    public void SetStrategy(BotStrategy strategy)
    {
        currentStrategy = strategy;
        // Reset circle timer when switching to circler
        if (strategy == BotStrategy.Circler)
        {
            circleTimer = 0;
            circleClockwise = random.Next(2) == 0;
        }
    }

    // Get the bot's input direction
    public Vector2 GetBotInput()
    {
        if (wrestler == null || opponent == null)
            return Vector2.Zero;

        // Choose strategy based on current setting
        Vector2 desiredDirection;
        switch (currentStrategy)
        {
            case BotStrategy.Circler:
                desiredDirection = GetCirclerDirection();
                break;
            case BotStrategy.Controller:
                desiredDirection = GetControllerDirection();
                break;
            case BotStrategy.Chaser:
            default:
                desiredDirection = GetChaserDirection();
                break;
        }
        
        // Find the closest of the 8 directional vectors
        return GetClosestDirectionalVector(desiredDirection);
    }
    
    // Update timers - call this from _Process in Main
    public void UpdateTimers(double delta)
    {
        if (currentStrategy == BotStrategy.Circler)
        {
            circleTimer += (float)delta;
            // Change circling direction every 2-4 seconds
            if (circleTimer > 2 + random.Next(3))
            {
                circleClockwise = !circleClockwise;
                circleTimer = 0;
            }
        }
    }
    
    // Strategy 1: Chaser - Always move toward opponent
    private Vector2 GetChaserDirection()
    {
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
        
        return desiredDirection;
    }
    
    // Strategy 2: Circler - Circle around the opponent to trick them
    private Vector2 GetCirclerDirection()
    {
        // Get current positions
        Vector2 myPosition = wrestler.GlobalPosition;
        Vector2 opponentPosition = opponent.GlobalPosition;
        
        // Vector from opponent to me
        Vector2 fromOpponentToMe = (myPosition - opponentPosition).Normalized();
        
        // Perpendicular vector for circling
        Vector2 perpendicular;
        if (circleClockwise)
            perpendicular = new Vector2(-fromOpponentToMe.Y, fromOpponentToMe.X);
        else
            perpendicular = new Vector2(fromOpponentToMe.Y, -fromOpponentToMe.X);
        
        // Maintain ideal distance from opponent
        float currentDistance = (myPosition - opponentPosition).Length();
        float idealDistance = 100; // Adjust as needed
        Vector2 distanceAdjust = Vector2.Zero;
        
        if (currentDistance < idealDistance * 0.8f)
        {
            // Too close, move away a bit
            distanceAdjust = fromOpponentToMe * 0.5f;
        }
        else if (currentDistance > idealDistance * 1.2f)
        {
            // Too far, move closer
            distanceAdjust = -fromOpponentToMe * 0.5f;
        }
        
        // Stay away from ring edge
        Vector2 myToCenter = dohyoCenter - myPosition;
        float distanceFromCenter = myToCenter.Length();
        float ringRadius = 200; // Adjust based on ring size
        Vector2 edgeAvoidance = Vector2.Zero;
        
        if (distanceFromCenter > ringRadius * 0.7f)
        {
            // Getting close to edge, bias toward center
            edgeAvoidance = myToCenter.Normalized();
        }
        
        // If opponent is near edge and behind us, switch to attack mode
        bool opponentNearEdge = (dohyoCenter - opponentPosition).Length() > ringRadius * 0.8f;
        Vector2 finalDirection;
        
        if (opponentNearEdge)
        {
            // Calculate if we're between opponent and center
            Vector2 opponentToMe = (myPosition - opponentPosition).Normalized();
            Vector2 opponentToCenter = (dohyoCenter - opponentPosition).Normalized();
            float dotProduct = opponentToMe.Dot(opponentToCenter);
            
            if (dotProduct > 0.7f) // We're between opponent and center
            {
                // Attack! Push them out
                finalDirection = (opponentPosition - myPosition).Normalized();
            }
            else
            {
                // Continue circling with adjustments
                finalDirection = (perpendicular + distanceAdjust + edgeAvoidance).Normalized();
            }
        }
        else
        {
            // Normal circling behavior
            finalDirection = (perpendicular + distanceAdjust + edgeAvoidance).Normalized();
        }
        
        return finalDirection;
    }
    
    // Strategy 3: Controller - Control the center and push out
    private Vector2 GetControllerDirection()
    {
        // Get current positions
        Vector2 myPosition = wrestler.GlobalPosition;
        Vector2 opponentPosition = opponent.GlobalPosition;
        
        // Vector to center
        Vector2 toCenter = (dohyoCenter - myPosition).Normalized();
        
        // Check distances
        float myDistanceToCenter = (myPosition - dohyoCenter).Length();
        float opponentDistanceToCenter = (opponentPosition - dohyoCenter).Length();
        float distanceBetweenWrestlers = (myPosition - opponentPosition).Length();
        
        // Determine ring radius (adjust as needed)
        float ringRadius = 200;
        
        Vector2 finalDirection;
        
        if (myDistanceToCenter > 50)
        {
            // Not at center yet, move toward center
            finalDirection = toCenter;
        }
        else
        {
            // We're at the center, now look for the opponent
            Vector2 toOpponent = (opponentPosition - myPosition).Normalized();
            
            if (opponentDistanceToCenter > ringRadius * 0.6f)
            {
                // Opponent is far from center, charge at them
                finalDirection = toOpponent;
            }
            else if (distanceBetweenWrestlers < 70)
            {
                // Opponent is close, push them away from center
                Vector2 pushDirection = (opponentPosition - dohyoCenter).Normalized();
                finalDirection = pushDirection;
            }
            else
            {
                // Move toward opponent but maintain center advantage
                finalDirection = (toOpponent * 0.7f + toCenter * 0.3f).Normalized();
            }
        }
        
        return finalDirection;
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