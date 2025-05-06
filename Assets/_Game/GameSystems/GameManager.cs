using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Disable curser during gameplay
        Cursor.lockState = CursorLockMode.Locked;
        
        // Set the target frame rate to 144 FPS
        Application.targetFrameRate = 144;
    }
}
