using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    public GameObject settingsMenuRoot;

    public Image resumeImage;
    public Image raycastLengthImage;
    public Image inventoryImage;
    public Image speedImage;
    public Image quitImage;

    public TMP_Text raycastLengthText;
    public TMP_Text speedText;

    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private int selectedIndex = 0;

    private readonly string[] menuItems =
    {
        "Resume",
        "RaycastLength",
        "Inventory",
        "Speed",
        "Quit"
    };

    private readonly float[] rayLengths = { 1f, 10f, 50f };
    private int rayLengthIndex = 1; 

    private readonly string[] speedModes = { "Low", "Medium", "High" };
    private readonly float[] speedValues = { 5f, 10f, 20f };
    private int speedIndex = 1;

    public void OpenMenu()
    {
        settingsMenuRoot.SetActive(true);
        selectedIndex = 0;
        UpdateVisuals();
    }

    public void CloseMenu()
    {
        settingsMenuRoot.SetActive(false);
    }

    public bool IsOpen()
    {
        return settingsMenuRoot.activeSelf;
    }

    public void MoveSelection(int direction)
    {
        selectedIndex += direction;

        if (selectedIndex < 0) selectedIndex = menuItems.Length - 1;
        if (selectedIndex >= menuItems.Length) selectedIndex = 0;

        UpdateVisuals();
    }

    public string GetSelectedItem()
    {
        return menuItems[selectedIndex];
    }

    public float ToggleRaycastLength()
    {
        rayLengthIndex = (rayLengthIndex + 1) % rayLengths.Length;
        UpdateVisuals();
        return rayLengths[rayLengthIndex];
    }

    public string ToggleSpeedMode(out float speedValue)
    {
        speedIndex = (speedIndex + 1) % speedModes.Length;
        speedValue = speedValues[speedIndex];
        UpdateVisuals();
        return speedModes[speedIndex];
    }

    public float GetCurrentRayLength()
    {
        return rayLengths[rayLengthIndex];
    }

    private void UpdateVisuals()
    {
        if (resumeImage != null) resumeImage.color = (selectedIndex == 0) ? highlightColor : normalColor;
        if (raycastLengthImage != null) raycastLengthImage.color = (selectedIndex == 1) ? highlightColor : normalColor;
        if (inventoryImage != null) inventoryImage.color = (selectedIndex == 2) ? highlightColor : normalColor;
        if (speedImage != null) speedImage.color = (selectedIndex == 3) ? highlightColor : normalColor;
        if (quitImage != null) quitImage.color = (selectedIndex == 4) ? highlightColor : normalColor;

        if (raycastLengthText != null)
            raycastLengthText.text = "Raycast Length: " + rayLengths[rayLengthIndex] + "m";

        if (speedText != null)
            speedText.text = "Speed: " + speedModes[speedIndex];
    }
}