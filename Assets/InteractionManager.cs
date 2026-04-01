using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    [Header("Button Mapping")]
    public string xButton = "js2";
    public string yButton = "js3";
    public string aButton = "js0";
    public string bButton = "js1";
    public string okButton = "js11";
    public string verticalAxis = "Vertical";

    [Header("Raycast Settings")]
    public float rayDistance = 100f;
    public float spawnYOffset = 0.55f;
    public Vector3 beamStartLocalOffset = new Vector3(0.75f, -0.42f, 0.55f);

    [System.Serializable]
    public class InventoryItem
    {
        public GameObject sceneObject;
        public Sprite thumbnail;
    }

    [Header("References")]
    public Transform character;
    public CharacterController characterController;
    public MonoBehaviour characterMovementScript;
    public LineRenderer lineRenderer;
    public ObjectMenuController objectMenuController;
    public SettingsMenuController settingsMenuController;
    public InventoryPanelController inventoryPanelController;

    private HighlightingScript currentHighlight;
    private GameObject lastDestroyedObject;
    private List<InventoryItem> inventory = new List<InventoryItem>();
    private const int maxInventory = 3;

    private float nextMenuMoveTime = 0f;
    private float menuMoveCooldown = 0.2f;

    private GameObject heldInventoryObject = null;
    private bool hasCurrentFloorHit = false;
    private Vector3 currentFloorHitPoint;
    private Vector3 heldSmoothVelocity = Vector3.zero;
    private float blockSpawnUntil = 0f;

    private Rigidbody heldInventoryRigidbody = null;
    private Collider[] heldInventoryColliders = null;
    private bool heldOriginalUseGravity = true;
    private bool heldOriginalIsKinematic = false;

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.03f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.useWorldSpace = false;
        }
    }

    void Update()
    {
        bool xPress = Input.GetButtonDown(xButton) || Input.GetKeyDown(KeyCode.X);
        bool yPress = Input.GetButtonDown(yButton) || Input.GetKeyDown(KeyCode.Y);
        bool aPress = Input.GetButtonDown(aButton) || Input.GetKeyDown(KeyCode.A);
        bool bPress = Input.GetButtonDown(bButton) || Input.GetKeyDown(KeyCode.Return);
        bool openSettingsPress = Input.GetButtonDown(okButton) || Input.GetKeyDown(KeyCode.O);

        float v = Input.GetAxisRaw(verticalAxis);

        if (Input.GetKeyDown(KeyCode.UpArrow))
            v = 1f;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            v = -1f;

        if (heldInventoryObject != null)
        {
            Vector3 holdOffset =
                Camera.main.transform.forward * 3.5f +
                Camera.main.transform.right * 1.7f +
                Camera.main.transform.up * -0.4f;

            Vector3 targetPos = Camera.main.transform.position + holdOffset;

            if (hasCurrentFloorHit)
            {
                float minHeight = currentFloorHitPoint.y + 0.8f;
                if (targetPos.y < minHeight)
                    targetPos.y = minHeight;
            }

            Vector3 smoothedPos = Vector3.SmoothDamp(
                heldInventoryObject.transform.position,
                targetPos,
                ref heldSmoothVelocity,
                0.05f
            );

            if (heldInventoryRigidbody != null)
                heldInventoryRigidbody.position = smoothedPos;
            else
                heldInventoryObject.transform.position = smoothedPos;

            if (aPress)
            {
                ReleaseHeldObject();
                return;
            }
        }

        if (settingsMenuController != null && openSettingsPress)
        {
            if (!settingsMenuController.IsOpen())
            {
                if (objectMenuController != null && objectMenuController.IsOpen())
                    objectMenuController.CloseMenu();

                if (inventoryPanelController != null && inventoryPanelController.IsOpen())
                    inventoryPanelController.ClosePanel();

                settingsMenuController.OpenMenu();
                SetMovementEnabled(false);

                if (lineRenderer != null)
                    lineRenderer.enabled = false;
            }
            else
            {
                settingsMenuController.CloseMenu();
                SetMovementEnabled(true);

                if (lineRenderer != null)
                    lineRenderer.enabled = true;
            }

            return;
        }

        if (settingsMenuController != null && settingsMenuController.IsOpen())
        {
            if (Time.time >= nextMenuMoveTime)
            {
                if (v > 0.5f)
                {
                    settingsMenuController.MoveSelection(-1);
                    nextMenuMoveTime = Time.time + menuMoveCooldown;
                }
                else if (v < -0.5f)
                {
                    settingsMenuController.MoveSelection(1);
                    nextMenuMoveTime = Time.time + menuMoveCooldown;
                }
            }

            if (bPress)
            {
                string selected = settingsMenuController.GetSelectedItem();

                if (selected == "Resume")
                {
                    settingsMenuController.CloseMenu();
                    SetMovementEnabled(true);

                    if (lineRenderer != null)
                        lineRenderer.enabled = true;
                }
                else if (selected == "RaycastLength")
                {
                    rayDistance = settingsMenuController.ToggleRaycastLength();
                }
                else if (selected == "Speed")
                {
                    float speedValue;
                    settingsMenuController.ToggleSpeedMode(out speedValue);

                    if (characterMovementScript != null)
                    {
                        var type = characterMovementScript.GetType();
                        var field =
                            type.GetField("Speed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                            ?? type.GetField("speed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                        if (field != null)
                            field.SetValue(characterMovementScript, speedValue);
                    }
                }
                else if (selected == "Inventory")
                {
                    settingsMenuController.CloseMenu();

                    if (inventoryPanelController != null)
                        inventoryPanelController.OpenPanel(GetInventorySprites());
                }
                else if (selected == "Quit")
                {
                    Application.Quit();
                }
            }

            return;
        }

        if (inventoryPanelController != null && inventoryPanelController.IsOpen())
        {
            if (Time.time >= nextMenuMoveTime)
            {
                if (v > 0.5f)
                {
                    inventoryPanelController.MoveSelection(-1);
                    nextMenuMoveTime = Time.time + menuMoveCooldown;
                }
                else if (v < -0.5f)
                {
                    inventoryPanelController.MoveSelection(1);
                    nextMenuMoveTime = Time.time + menuMoveCooldown;
                }
            }

            if (bPress && inventory.Count > 0)
            {
                int selectedIndex = inventoryPanelController.GetSelectedIndex();
                GrabInventoryItem(selectedIndex);

                inventoryPanelController.ClosePanel();
                SetMovementEnabled(true);

                if (lineRenderer != null)
                    lineRenderer.enabled = true;
            }

            return;
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 worldEndPoint = ray.origin + ray.direction * rayDistance;

        ClearCurrentHighlight();

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            worldEndPoint = hit.point;
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.CompareTag("Floor"))
            {
                hasCurrentFloorHit = true;
                currentFloorHitPoint = hit.point;
            }
            else
            {
                hasCurrentFloorHit = false;
            }

            if (hitObj.CompareTag("Interactable"))
            {
                HighlightingScript highlighter = hitObj.GetComponent<HighlightingScript>();

                if (highlighter == null)
                    highlighter = hitObj.GetComponentInParent<HighlightingScript>();

                if (highlighter == null)
                    highlighter = hitObj.GetComponentInChildren<HighlightingScript>();

                if (highlighter != null)
                {
                    currentHighlight = highlighter;
                    currentHighlight.EnableHighlight();
                }

                if (xPress)
                {
                    if (objectMenuController.IsOpen())
                        objectMenuController.CloseMenu();

                    objectMenuController.OpenMenu(hitObj);
                    SetMovementEnabled(false);
                }
            }

            if (!objectMenuController.IsOpen() && yPress && hitObj.CompareTag("Floor"))
            {
                TeleportCharacter(hit.point);
            }

            if (heldInventoryObject == null &&
                Time.time >= blockSpawnUntil &&
                !objectMenuController.IsOpen() &&
                aPress &&
                hitObj.CompareTag("Floor"))
            {
                SpawnLastDestroyed(hit.point);
            }

            if (objectMenuController.IsOpen())
            {
                HandleObjectMenuRaycast(hitObj, bPress);
            }
        }
        else
        {
            hasCurrentFloorHit = false;

            if (objectMenuController != null && objectMenuController.IsOpen())
                objectMenuController.ClearHover();
        }

        if (lineRenderer != null)
        {
            Vector3 localEndPoint = transform.InverseTransformPoint(worldEndPoint);
            lineRenderer.SetPosition(0, beamStartLocalOffset);
            lineRenderer.SetPosition(1, localEndPoint);
        }
    }

    void HandleObjectMenuRaycast(GameObject hitObj, bool bPress)
    {
        if (hitObj.CompareTag("MenuDestroy"))
        {
            objectMenuController.SetHoveredOption("Destroy");
            if (bPress) DestroyCurrentObject();
        }
        else if (hitObj.CompareTag("MenuStore"))
        {
            objectMenuController.SetHoveredOption("Store");
            if (bPress) StoreCurrentObject();
        }
        else if (hitObj.CompareTag("MenuExit"))
        {
            objectMenuController.SetHoveredOption("Exit");
            if (bPress) ExitObjectMenu();
        }
        else
        {
            objectMenuController.ClearHover();
        }
    }

    void DestroyCurrentObject()
    {
        GameObject target = objectMenuController.GetCurrentTarget();
        if (target == null) return;

        lastDestroyedObject = target;
        target.SetActive(false);

        objectMenuController.CloseMenu();
        SetMovementEnabled(true);
    }

    void StoreCurrentObject()
    {
        GameObject target = objectMenuController.GetCurrentTarget();
        if (target == null) return;

        if (inventory.Count >= maxInventory)
        {
            StartCoroutine(objectMenuController.ShowMessage("Inventory is full!", 2f));
            return;
        }

        InteractableObject interactable = target.GetComponent<InteractableObject>();
        if (interactable == null)
            interactable = target.GetComponentInChildren<InteractableObject>();

        InventoryItem item = new InventoryItem();
        item.sceneObject = target;
        item.thumbnail = (interactable != null) ? interactable.thumbnail : null;

        inventory.Add(item);
        target.SetActive(false);

        objectMenuController.CloseMenu();
        SetMovementEnabled(true);
    }

    void ExitObjectMenu()
    {
        objectMenuController.CloseMenu();
        SetMovementEnabled(true);
    }

    void SpawnLastDestroyed(Vector3 floorHitPoint)
    {
        if (lastDestroyedObject == null) return;

        Vector3 spawnPos = floorHitPoint;
        spawnPos.y += spawnYOffset;

        lastDestroyedObject.transform.position = spawnPos;
        lastDestroyedObject.SetActive(true);

        Rigidbody rb = lastDestroyedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        lastDestroyedObject = null;
    }

    List<Sprite> GetInventorySprites()
    {
        List<Sprite> sprites = new List<Sprite>();

        foreach (var item in inventory)
            sprites.Add(item.thumbnail);

        return sprites;
    }

    void GrabInventoryItem(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        heldInventoryObject = inventory[index].sceneObject;
        heldInventoryObject.SetActive(true);
        heldSmoothVelocity = Vector3.zero;

        heldInventoryRigidbody = heldInventoryObject.GetComponent<Rigidbody>();
        if (heldInventoryRigidbody == null)
            heldInventoryRigidbody = heldInventoryObject.GetComponentInChildren<Rigidbody>();

        if (heldInventoryRigidbody != null)
        {
            heldOriginalUseGravity = heldInventoryRigidbody.useGravity;
            heldOriginalIsKinematic = heldInventoryRigidbody.isKinematic;

            heldInventoryRigidbody.linearVelocity = Vector3.zero;
            heldInventoryRigidbody.angularVelocity = Vector3.zero;
            heldInventoryRigidbody.useGravity = false;
            heldInventoryRigidbody.isKinematic = true;
        }

        heldInventoryColliders = heldInventoryObject.GetComponentsInChildren<Collider>(true);
        if (heldInventoryColliders != null)
        {
            foreach (var col in heldInventoryColliders)
                col.enabled = false;
        }

        inventory.RemoveAt(index);
    }

    void ReleaseHeldObject()
    {
        if (heldInventoryObject == null) return;
        if (!hasCurrentFloorHit) return;

        Vector3 dropPos = currentFloorHitPoint;
        dropPos.y += spawnYOffset;

        if (heldInventoryRigidbody != null)
        {
            heldInventoryRigidbody.linearVelocity = Vector3.zero;
            heldInventoryRigidbody.angularVelocity = Vector3.zero;
            heldInventoryRigidbody.position = dropPos;
            heldInventoryRigidbody.rotation = heldInventoryObject.transform.rotation;
        }

        heldInventoryObject.transform.position = dropPos;

        if (heldInventoryColliders != null)
        {
            foreach (var col in heldInventoryColliders)
                col.enabled = true;
        }

        if (heldInventoryRigidbody != null)
        {
            heldInventoryRigidbody.useGravity = heldOriginalUseGravity;
            heldInventoryRigidbody.isKinematic = heldOriginalIsKinematic;
        }

        Physics.SyncTransforms();

        heldSmoothVelocity = Vector3.zero;
        heldInventoryObject = null;
        heldInventoryRigidbody = null;
        heldInventoryColliders = null;
        blockSpawnUntil = Time.time + 0.35f;
    }

    void SetMovementEnabled(bool enabled)
    {
        if (characterMovementScript != null)
            characterMovementScript.enabled = enabled;
    }

    void ClearCurrentHighlight()
    {
        if (currentHighlight != null)
        {
            currentHighlight.DisableHighlight();
            currentHighlight = null;
        }
    }

    void TeleportCharacter(Vector3 floorHitPoint)
    {
        if (character == null) return;

        Vector3 target = floorHitPoint;

        if (characterController != null)
        {
            float yOffset = characterController.height * 0.5f;
            target.y += yOffset;

            characterController.enabled = false;
            character.position = target;
            characterController.enabled = true;
        }
        else
        {
            target.y += 0.05f;
            character.position = target;
        }
    }
}