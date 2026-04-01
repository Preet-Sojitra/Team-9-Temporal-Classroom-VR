using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryPanelController : MonoBehaviour
{
    public GameObject inventoryRoot;
    public Image[] slotImages;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private List<Sprite> currentSprites = new List<Sprite>();
    private int selectedIndex = 0;

    public void OpenPanel(List<Sprite> sprites)
    {
        currentSprites = new List<Sprite>(sprites);
        selectedIndex = 0;
        inventoryRoot.SetActive(true);
        RefreshSlots();
    }

    public void ClosePanel()
    {
        inventoryRoot.SetActive(false);
    }

    public bool IsOpen()
    {
        return inventoryRoot.activeSelf;
    }

    public void MoveSelection(int direction)
    {
        if (currentSprites.Count == 0) return;

        selectedIndex += direction;

        if (selectedIndex < 0)
            selectedIndex = currentSprites.Count - 1;
        if (selectedIndex >= currentSprites.Count)
            selectedIndex = 0;

        RefreshSlots();
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i < currentSprites.Count)
            {
                slotImages[i].gameObject.SetActive(true);
                slotImages[i].sprite = currentSprites[i];
                slotImages[i].color = (i == selectedIndex) ? highlightColor : normalColor;
            }
            else
            {
                slotImages[i].gameObject.SetActive(false);
            }
        }
    }
}