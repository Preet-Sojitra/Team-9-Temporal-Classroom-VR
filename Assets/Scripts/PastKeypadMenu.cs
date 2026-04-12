using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PastKeypadMenu : MonoBehaviour
{
    [Header("UI Display")]
    public TextMeshProUGUI displayScreen;
    public string correctCode = "7580";
    private string currentInput = "";

    [Header("Chest References")]
    public GameObject closedChest; // The one with the outline
    public GameObject openChest;   // Disabled by default
    public GameObject keyInside;   // Inside openChest, has Outline script

    private Canvas menuCanvas;
    private Camera mainCamera;
    private GameObject currentHoveredButton;

    private Color normalColor = Color.white;
    private Color highlightColor = Color.yellow;

    void Start()
    {
        menuCanvas = GetComponent<Canvas>();
        menuCanvas.enabled = false;
        mainCamera = Camera.main;
        displayScreen.text = "____";
    }

    void Update()
    {
        if (!menuCanvas.enabled) return;
        // Face player
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);
    }

    public void OpenMenu()
    {
        menuCanvas.enabled = true;
        currentInput = "";
        displayScreen.text = "";
    }

    public void CloseMenu()
    {
        menuCanvas.enabled = false;
        ClearHighlight();
    }

    public void HoverButton(GameObject hitObj)
    {
        // Safety check: if we hit nothing, just clear the highlight and stop
        if (hitObj == null)
        {
            ClearHighlight();
            return;
        }

        // Highlight logic: checking if hit object is a button child
        if (hitObj.CompareTag("KeypadButton"))
        {
            if (currentHoveredButton != hitObj)
            {
                ClearHighlight();
                currentHoveredButton = hitObj;
                SetButtonColor(currentHoveredButton, highlightColor);
            }
        }
        else
        {
            ClearHighlight();
        }
    }


    public void SelectButton()
    {
        if (currentHoveredButton == null) return;

        string val = currentHoveredButton.name;

        if (val == "y") // SUBMIT LOGIC
        {
            // Only check if the player has typed 4 digits
            if (currentInput.Length == 4)
            {
                if (currentInput == correctCode)
                {
                    StartCoroutine(UnlockSequence());
                }
                else
                {
                    StartCoroutine(WrongCodeFlash());
                }
            }
            else
            {
                // Optional: Flash "SHORT" or just do nothing if they haven't typed enough
                Debug.Log("Code too short to submit!");
            }
        }
        else if (val == "x") // CLEAR LOGIC
        {
            currentInput = "";
            displayScreen.text = "____"; // Reset placeholder
        }
        else if (currentInput.Length < 4) // NUMBER TYPING
        {
            // Only add if it's a number (prevents accidental naming issues)
            if (int.TryParse(val, out _))
            {
                currentInput += val;
                displayScreen.text = currentInput;
            }
        }
    }

    IEnumerator UnlockSequence()
    {
        displayScreen.text = "OPEN";
        yield return new WaitForSeconds(1f);

        if (closedChest != null) closedChest.SetActive(false); // Remove the closed one
        if (openChest != null) openChest.SetActive(true);    // Show the open one

        CloseMenu(); // Hide the keypad so they can see the chest
    }

    IEnumerator WrongCodeFlash()
    {
        displayScreen.text = "ERR";
        yield return new WaitForSeconds(1f);
        currentInput = "";
        displayScreen.text = "";
    }

    void SetButtonColor(GameObject btn, Color col)
    {
        if (btn == null) return;

        // Try to get Image on the object itself or its children (where the visual usually is)
        Image img = btn.GetComponent<Image>();
        if (img == null) img = btn.GetComponentInChildren<Image>();

        if (img != null) img.color = col;
    }

    void ClearHighlight()
    {
        if (currentHoveredButton != null) SetButtonColor(currentHoveredButton, normalColor);
        currentHoveredButton = null;
    }

    public bool IsMenuOpen() => menuCanvas != null && menuCanvas.enabled;
}