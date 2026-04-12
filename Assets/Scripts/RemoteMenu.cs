using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObjectMenu : MonoBehaviour
{
    [Header("Puzzle Buttons")]
    public GameObject powerButton; // Turn on projector
    public GameObject infoButton;  // Optional puzzle hint
    public GameObject exitButton;

    [Header("Projector Puzzle Setup")]
    public GameObject projectorMainObject; // The actual projector machine
    public GameObject beamObject;          // The light beam cylinder/shader
    public GameObject projectorCodeScreen; // The UI/Plane with the code
                                           // public AudioSource projectorAudio;  // Placeholder for sound

    private Canvas menuCanvas;
    private GameObject currentHoveredButton;
    private Camera mainCamera;

    private Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    private Color highlightColor = Color.yellow;

    void Start()
    {
        menuCanvas = GetComponent<Canvas>();
        menuCanvas.enabled = false;
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!menuCanvas.enabled) return;

        // Make menu face the player
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);
    }

    public void SelectCurrentButton()
    {
        if (currentHoveredButton == powerButton)
            StartCoroutine(ProjectorSequence()); // Start the delayed sequence
        else if (currentHoveredButton == exitButton)
            CloseMenu();
    }

    IEnumerator ProjectorSequence()
    {
        // 1. Immediate Feedback: Close Menu and Highlight Projector Green
        CloseMenu();

        if (projectorMainObject != null)
        {
            if (projectorMainObject.TryGetComponent<Outline>(out var outline))
            {
                outline.OutlineColor = Color.green;
                outline.enabled = true;
            }
        }

        // 2. Audio Placeholder
        // if(projectorAudio != null) projectorAudio.Play();
        Debug.Log("Projector starting sound would play now...");

        // 3. The Delay (Wait for 2.5 seconds)
        yield return new WaitForSeconds(2.5f);

        // 4. Activate Visuals
        if (beamObject != null) beamObject.SetActive(true);
        if (projectorCodeScreen != null)
        {
            // First, turn on the GameObject
            projectorCodeScreen.SetActive(true);

            // Second, force the Canvas component to be checked/enabled
            Canvas codeCanvas = projectorCodeScreen.GetComponent<Canvas>();
            if (codeCanvas != null)
            {
                codeCanvas.enabled = true;
            }
        }

        Debug.Log("Projector Sequence Complete: Code Visible.");
    }

    public void OpenMenu(GameObject obj)
    {
        // Move it UP (0.5f) and TOWARD the camera (-1.0f on Z or based on direction)
        Vector3 shiftTowardPlayer = (mainCamera.transform.position - obj.transform.position).normalized * 0.8f;
        transform.position = obj.transform.position + new Vector3(0, 0.5f, 0) + shiftTowardPlayer;

        menuCanvas.enabled = true;
    }

    public void CloseMenu()
    {
        menuCanvas.enabled = false;
        SetButtonHighlight(currentHoveredButton, false);
        currentHoveredButton = null;
    }

    // Called by RaycastPointer when hovering over a button
    public void HoverButton(GameObject hitObj)
    {
        GameObject hitButton = GetButtonFromHit(hitObj);

        if (hitButton != null)
        {
            if (currentHoveredButton != hitButton)
            {
                SetButtonHighlight(currentHoveredButton, false);
                currentHoveredButton = hitButton;
                SetButtonHighlight(currentHoveredButton, true);
            }
        }
        else
        {
            ClearButtonHighlight();
        }
    }

    public void ClearButtonHighlight()
    {
        SetButtonHighlight(currentHoveredButton, false);
        currentHoveredButton = null;
    }


    GameObject GetButtonFromHit(GameObject hitObj)
    {
        if (hitObj == powerButton || hitObj.transform.IsChildOf(powerButton.transform)) return powerButton;
        if (hitObj == infoButton || hitObj.transform.IsChildOf(infoButton.transform)) return infoButton;
        if (hitObj == exitButton || hitObj.transform.IsChildOf(exitButton.transform)) return exitButton;
        return null;
    }

    void SetButtonHighlight(GameObject button, bool highlighted)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null)
            img.color = highlighted ? highlightColor : normalColor;
    }

    public bool IsMenuOpen() => menuCanvas != null && menuCanvas.enabled;
}