using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ObjectMenuController : MonoBehaviour
{
    public GameObject menuRoot;
    public Image destroyImage;
    public Image storeImage;
    public Image exitImage;
    public TMP_Text messageText;
    public Transform playerCamera;

    private GameObject currentTargetObject;
    private string currentHoveredOption = "";

    private Color normalColor = Color.white;
    private Color hoverColor = Color.yellow;

    public void OpenMenu(GameObject target)
    {
        currentTargetObject = target;

        Vector3 offset = new Vector3(1.0f, 0.8f, 0f);
        menuRoot.transform.position = target.transform.position + offset;

        FacePlayer();

        menuRoot.SetActive(true);
        ClearHover();
    }

    void Update()
    {
        if (menuRoot != null && menuRoot.activeSelf)
        {
            FacePlayer();
        }
    }

    void FacePlayer()
    {
        if (playerCamera == null || menuRoot == null) return;

        Vector3 direction = menuRoot.transform.position - playerCamera.position;
        menuRoot.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void CloseMenu()
    {
        menuRoot.SetActive(false);
        currentTargetObject = null;
        ClearHover();
    }

    public bool IsOpen()
    {
        return menuRoot.activeSelf;
    }

    public GameObject GetCurrentTarget()
    {
        return currentTargetObject;
    }

    public void SetHoveredOption(string option)
    {
        currentHoveredOption = option;
        UpdateButtonColors();
    }

    public void ClearHover()
    {
        currentHoveredOption = "";
        UpdateButtonColors();
    }

    private void UpdateButtonColors()
    {
        if (destroyImage != null)
            destroyImage.color = (currentHoveredOption == "Destroy") ? hoverColor : normalColor;

        if (storeImage != null)
            storeImage.color = (currentHoveredOption == "Store") ? hoverColor : normalColor;

        if (exitImage != null)
            exitImage.color = (currentHoveredOption == "Exit") ? hoverColor : normalColor;
    }

    public IEnumerator ShowMessage(string msg, float duration)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            messageText.gameObject.SetActive(false);
        }
    }
}