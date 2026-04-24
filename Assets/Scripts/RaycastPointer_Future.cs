using UnityEngine;

public class RaycastPointerFuture : MonoBehaviour
{
    public CharacterMovement CharacterMovement;

    [Header("Raycast Settings")]
    public float raycastLength = 2f;

    [Header("Line Renderer")]
    public LineRenderer lineRenderer;

    [Header("Visual Offset")]
    public Vector3 visualOffset = new Vector3(0.2f, -0.2f, 0.1f);
    public bool offsetToRight = true;

    [Header("UI References")]
    public ObjectMenu objectMenu;

    [Header("Interaction State")]
    private GameObject currentHoveredObject;
    private GameObject grabbedKey = null;
    public Transform rayTip;

    [Header("AI Clock")]
    public AIClockManager aiClock;
    private bool isTalkingToClock = false;

    void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.015f; // Standard VR laser width
            lineRenderer.endWidth = 0.005f;   // Tiny dot tip
        }
    }

    void LateUpdate()
    {
        if (CharacterMovement != null && objectMenu != null)
        {
            CharacterMovement.enabled = !objectMenu.IsMenuOpen();
        }

        ShootRaycast();
    }

    void ShootRaycast()
    {
        Vector3 mathOrigin = transform.position;
        Vector3 direction = transform.forward;
        Ray ray = new Ray(mathOrigin, direction);
        RaycastHit hit;

        // 1. Handle Held Object (If player picks up the teleported key)
        if (grabbedKey != null)
        {
            grabbedKey.transform.position = rayTip != null ? rayTip.position : mathOrigin + (direction * 1.5f);
            grabbedKey.transform.rotation = rayTip != null ? rayTip.rotation : Quaternion.identity;

            // Logic for dropping the key in the future room can go here later
        }

        // 2. Visual Line Setup (World Space)
        float sideDirection = offsetToRight ? 1f : -1f;
        Vector3 localOrigin = new Vector3(visualOffset.x * sideDirection, visualOffset.y, visualOffset.z);
        Vector3 worldOrigin = lineRenderer.transform.TransformPoint(localOrigin);

        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, worldOrigin);

        // 3. Physics Check
        if (Physics.Raycast(ray, out hit, raycastLength))
        {
            lineRenderer.SetPosition(1, hit.point);
            GameObject hitObject = hit.collider.gameObject;

            // Priority 1: The Menu
            if (objectMenu != null && objectMenu.IsMenuOpen())
            {
                objectMenu.HoverButton(hitObject);
                if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                {
                    objectMenu.SelectCurrentButton();
                }
                return;
            }

            // Priority 2: World Objects (Key or Interactables)
            if (hitObject.CompareTag("Key") || hitObject.CompareTag("Interactable"))
            {
                UpdateHighlight(hitObject);

                if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                {
                    if (hitObject.CompareTag("Key"))
                    {
                        GrabKey(hitObject);
                    }
                    else if (hitObject.CompareTag("Interactable"))
                    {
                        objectMenu.OpenMenu(hitObject);
                    }
                }
            }
            // --- AI CLOCK PUSH-TO-TALK ---
            else if (IsAIClock(hitObject) && aiClock != null)
            {
                if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                {
                    isTalkingToClock = true;
                    aiClock.OnPlayerStartTalking();
                }
                if (isTalkingToClock && (Input.GetButtonUp("js2") || Input.GetKeyUp(KeyCode.X)))
                {
                    isTalkingToClock = false;
                    aiClock.OnPlayerStopTalking();
                }
            }
            else
            {
                ClearHighlight();
            }
        }
        else
        {
            Vector3 worldEndPoint = mathOrigin + direction * raycastLength;
            lineRenderer.SetPosition(1, worldEndPoint);
            ClearHighlight();
            if (objectMenu != null && objectMenu.IsMenuOpen()) objectMenu.ClearButtonHighlight();
        }
    }

    void GrabKey(GameObject key)
    {
        grabbedKey = key;
        if (key.GetComponent<Collider>()) key.GetComponent<Collider>().enabled = false;
        key.transform.SetParent(this.transform);
        Debug.Log("Future Key Picked Up!");
    }

    void UpdateHighlight(GameObject hitObject)
    {
        if (currentHoveredObject != hitObject)
        {
            ClearHighlight();
            currentHoveredObject = hitObject;
            SetHighlight(currentHoveredObject, true);
        }
    }

    void SetHighlight(GameObject obj, bool state)
    {
        if (obj != null && obj.TryGetComponent<Outline>(out var outline))
        {
            outline.enabled = state;
        }
    }

    void ClearHighlight()
    {
        if (currentHoveredObject != null)
        {
            SetHighlight(currentHoveredObject, false);
            currentHoveredObject = null;
        }
    }

    private bool IsAIClock(GameObject obj)
    {
        if (obj.CompareTag("AIClock")) return true;
        if (obj.transform.parent != null && obj.transform.parent.CompareTag("AIClock")) return true;
        if (obj.transform.root.CompareTag("AIClock")) return true;
        return false;
    }
}