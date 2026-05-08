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
    public GameObject closedChest;
    public GameObject openChest;
    public GameObject keyInside;

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

        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0);
        }
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
        if (hitObj == null)
        {
            ClearHighlight();
            return;
        }

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

        if (val == "y")
        {
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
                Debug.Log("Code too short to submit!");
            }
        }
        else if (val == "x")
        {
            currentInput = "";
            displayScreen.text = "____";
        }
        else if (currentInput.Length < 4)
        {
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

        if (closedChest != null) closedChest.SetActive(false);
        if (openChest != null)
        {
            openChest.SetActive(true);    // Show the open one
            if (keyInside != null) keyInside.SetActive(true);
        }

        CloseMenu();
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