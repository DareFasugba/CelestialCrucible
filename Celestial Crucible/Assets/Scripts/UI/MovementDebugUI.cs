using UnityEngine;

public class MovementDebugUI : MonoBehaviour
{
    public AdvancedPlayerMovement player;

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 30), "Velocity: " + player.GetVelocity());
        GUI.Label(new Rect(10, 30, 400, 30), "Grounded: " + player.IsGrounded());
    }
}

