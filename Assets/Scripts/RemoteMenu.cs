using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectMenu : MonoBehaviour
{
    [Header("Puzzle Buttons")]
    public GameObject powerButton; // Turn on projector
    public GameObject infoButton;  // Optional puzzle hint
    public GameObject exitButton;

    [Header("Projector Reference")]
    public GameObject projectorCodeScreen; // The screen showing the code

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

    public void OpenMenu(GameObject obj)
    {
        transform.position = obj.transform.position + new Vector3(0, 0.5f, 0); // Position above remote
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

    public void SelectCurrentButton()
    {
        if (currentHoveredButton == powerButton)
            TurnOnProjector();
        else if (currentHoveredButton == exitButton)
            CloseMenu();
    }

    void TurnOnProjector()
    {
        if (projectorCodeScreen != null)
            projectorCodeScreen.SetActive(true); // Reveal the code for the Past player
        CloseMenu();
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