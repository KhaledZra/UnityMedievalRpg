using UnityEngine;
using TMPro;

// INSTRUCTIONS:
// 1. Create an empty GameObject or a UI Text object.
// 2. Attach this script.
// 3. Assign a TextMeshProUGUI component to the 'fpsText' slot in the inspector.
// 4. (Optional) If you don't assign a text component, it will draw a rough debug label on screen automatically.
// Made by Baba Gemini
public class FPSCounter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How often the HUD updates (in seconds). 0.1f to 0.5f is ideal.")]
    [SerializeField] private float updateInterval = 0.2f;

    [Header("References")]
    [Tooltip("Drag your TextMeshProUGUI component here. If null, uses OnGUI fallback.")]
    [SerializeField] private TextMeshProUGUI fpsText;

    private float _accumulatedTime = 0f;
    private int _frameCount = 0;
    private float _timeLeft;
    private float _fps;

    // GUI Style for fallback
    private GUIStyle _fallbackStyle;

    private void Start()
    {
        _timeLeft = updateInterval;

        // Setup fallback style just in case
        _fallbackStyle = new GUIStyle();
        _fallbackStyle.fontSize = 25;
        _fallbackStyle.fontStyle = FontStyle.Bold;
        _fallbackStyle.normal.textColor = Color.white;

        if (fpsText == null)
        {
            Debug.LogWarning("FPSCounter: No TextMeshProUGUI assigned. Falling back to OnGUI debug display.");
        }
    }

    private void Update()
    {
        _timeLeft -= Time.unscaledDeltaTime;
        _accumulatedTime += Time.unscaledDeltaTime; // simpler accumulation
        _frameCount++;

        // Only update the display logic when the interval runs out
        if (_timeLeft <= 0f)
        {
            _fps = _frameCount / _accumulatedTime;

            // Format logic
            if (fpsText != null)
            {
                fpsText.text = $"{_fps:F1} FPS";
                fpsText.color = GetColorByFPS(_fps);
            }

            // Reset
            _timeLeft = updateInterval;
            _accumulatedTime = 0f;
            _frameCount = 0;
        }
    }

    // Fallback: If you didn't setup the UI, this draws it on top of everything.
    private void OnGUI()
    {
        if (fpsText != null) return; // Don't draw if we have a UI element

        // Update color based on cached FPS value
        _fallbackStyle.normal.textColor = GetColorByFPS(_fps);

        // Draw top-left
        GUI.Label(new Rect(20, 20, 200, 50), $"{_fps:F1} FPS", _fallbackStyle);
    }

    private Color GetColorByFPS(float fps)
    {
        if (fps >= 60) return Color.green;
        if (fps >= 30) return Color.yellow;
        return Color.red;
    }
}
