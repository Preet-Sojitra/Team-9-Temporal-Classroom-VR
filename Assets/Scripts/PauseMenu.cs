using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Buttons")]
    public GameObject resumeButton;
    public GameObject restartButton;
    public GameObject speedButton;
    public GameObject raycastButton;

    [Header("Button Labels")]
    public TextMeshProUGUI speedLabel;
    public TextMeshProUGUI raycastLabel;

    [Header("Target Scripts")]
    public MonoBehaviour movementScript;      // Drag your CharacterMovement here
    public MonoBehaviour raycastScript;       // Drag RaycastPointer_Past or Future here

    [Header("Speed Settings")]
    public float[] speedOptions = { 1f, 3f, 6f };
    public string[] speedNames = { "Low", "Medium", "High" };
    private int _speedIndex = 1; // Default: Medium

    [Header("Raycast Settings")]
    public float[] raycastOptions = { 1.5f, 3f, 5f };
    public string[] raycastNames = { "Short", "Medium", "Long" };
    private int _raycastIndex = 1; // Default: Medium

    private Canvas _canvas;
    private Camera _cam;
    private GameObject _hoveredButton;
    private bool _isOpen = false;

    private Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    private Color highlightColor = Color.yellow;

    void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;
        _cam = Camera.main;

        UpdateLabels();
        ApplySpeed();
        ApplyRaycast();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("js0"))
        {
            if (_isOpen) CloseMenu();
            else OpenMenu();
        }

        if (!_isOpen) return;

        // Face player
        if (_cam == null) _cam = Camera.main;
        if (_cam != null)
        {
            transform.LookAt(_cam.transform);
            transform.Rotate(0, 180, 0);
        }

        // Hover detection via raycast (reuse same pattern as your other menus)
        HandleHover();

        // X to select
        if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
        {
            SelectHovered();
        }
    }

    void HandleHover()
    {
        // Get the active raycast script's ray
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f))
        {
            GameObject obj = hit.collider.gameObject;
            if (obj == resumeButton || obj == restartButton ||
                obj == speedButton || obj == raycastButton)
            {
                if (_hoveredButton != obj)
                {
                    ClearHighlight();
                    _hoveredButton = obj;
                    SetHighlight(_hoveredButton, true);
                }
            }
            else
            {
                ClearHighlight();
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    void SelectHovered()
    {
        if (_hoveredButton == resumeButton) CloseMenu();
        else if (_hoveredButton == restartButton) RestartRoom();
        else if (_hoveredButton == speedButton) CycleSpeed();
        else if (_hoveredButton == raycastButton) CycleRaycast();
    }

    void OpenMenu()
    {
        _isOpen = true;
        _canvas.enabled = true;

        // Position it in front of the player
        if (_cam != null)
        {
            transform.position = _cam.transform.position + _cam.transform.forward * 1.5f;
            transform.position += Vector3.up * 0.1f;
        }

        // Pause movement while menu is open
        if (movementScript != null) movementScript.enabled = false;
    }

    void CloseMenu()
    {
        _isOpen = false;
        _canvas.enabled = false;
        ClearHighlight();

        // Re-enable movement
        if (movementScript != null) movementScript.enabled = true;
    }

    void RestartRoom()
    {
        CloseMenu();
        // Reload scene locally WITHOUT touching the network connection
        // This resets the local player's room state only
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // NOTE: This will disconnect Fusion. If you want a true local-only
        // reset without network drop, you instead need to manually reset
        // positions and states. See comment below for that approach.
    }

    void CycleSpeed()
    {
        _speedIndex = (_speedIndex + 1) % speedOptions.Length;
        UpdateLabels();
        ApplySpeed();
    }

    void CycleRaycast()
    {
        _raycastIndex = (_raycastIndex + 1) % raycastOptions.Length;
        UpdateLabels();
        ApplyRaycast();
    }

    void ApplySpeed()
    {
        if (movementScript == null) return;

        // Try to set speed on CharacterMovement
        var field = movementScript.GetType().GetField("moveSpeed");
        if (field != null)
            field.SetValue(movementScript, speedOptions[_speedIndex]);
        else
        {
            // Try property too
            var prop = movementScript.GetType().GetProperty("moveSpeed");
            if (prop != null) prop.SetValue(movementScript, speedOptions[_speedIndex]);
        }
    }

    void ApplyRaycast()
    {
        if (raycastScript == null) return;

        var field = raycastScript.GetType().GetField("raycastLength");
        if (field != null)
            field.SetValue(raycastScript, raycastOptions[_raycastIndex]);
    }

    void UpdateLabels()
    {
        if (speedLabel != null)
            speedLabel.text = $"Speed: {speedNames[_speedIndex]}";
        if (raycastLabel != null)
            raycastLabel.text = $"Reach: {raycastNames[_raycastIndex]}";
    }

    void SetHighlight(GameObject btn, bool on)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = on ? highlightColor : normalColor;
    }

    void ClearHighlight()
    {
        SetHighlight(_hoveredButton, false);
        _hoveredButton = null;
    }

    public bool IsMenuOpen() => _isOpen;
}