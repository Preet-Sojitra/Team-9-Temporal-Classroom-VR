using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObjectMenu : MonoBehaviour
{
    [Header("Puzzle Buttons")]
    public GameObject powerButton;
    public GameObject infoButton;
    public GameObject exitButton;

    [Header("Projector Puzzle Setup")]
    public GameObject projectorMainObject;
    public GameObject beamObject;
    public GameObject projectorCodeScreen;


    [Header("Beam Auto Setup")]
    public Transform projectorEmitPoint;
    public Transform screenTransform;

    [Header("Projector Beam")]
    public GameObject projectorBeamObject;

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

        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0);
        }
    }

    public void SelectCurrentButton()
    {
        if (currentHoveredButton == powerButton)
            StartCoroutine(ProjectorSequence());
        else if (currentHoveredButton == exitButton)
            CloseMenu();
    }

    IEnumerator ProjectorSequence()
    {
        CloseMenu();

        if (projectorMainObject != null)
        {
            if (projectorMainObject.TryGetComponent<Outline>(out var outline))
            {
                outline.OutlineColor = Color.green;
                outline.enabled = true;
            }
        }

        // Debug.Log("Projector starting sound would play now...");

        yield return new WaitForSeconds(2.5f);

        if (projectorBeamObject != null)
        {
            projectorBeamObject.SetActive(true);
        }

        if (projectorCodeScreen != null)
        {
            projectorCodeScreen.SetActive(true);

            Canvas codeCanvas = projectorCodeScreen.GetComponent<Canvas>();
            if (codeCanvas != null)
            {
                codeCanvas.enabled = true;
            }
        }

        // Debug.Log("Projector Sequence Complete: Code Visible.");
    }

    public void OpenMenu(GameObject obj)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera != null)
        {
            Vector3 shiftTowardPlayer = (mainCamera.transform.position - obj.transform.position).normalized * 0.8f;
            transform.position = obj.transform.position + new Vector3(0, 0.5f, 0) + shiftTowardPlayer;
        }
        else
        {
            transform.position = obj.transform.position + new Vector3(0, 1.2f, 0);
            Debug.LogWarning("ObjectMenu: No Main Camera found to calculate offset!");
        }

        menuCanvas.enabled = true;
    }


    public void CloseMenu()
    {
        menuCanvas.enabled = false;
        SetButtonHighlight(currentHoveredButton, false);
        currentHoveredButton = null;
    }

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