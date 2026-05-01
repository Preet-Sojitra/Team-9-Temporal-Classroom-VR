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

    [Header("Future Pedestal")]
    public GameObject futurePedestal;  // drag in Inspector

    [Header("Key Drop")]
    public Transform futureDropPoint; // Drag your "spawn location" empty object here

    void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.015f; // Standard VR laser width
            lineRenderer.endWidth = 0.005f;   // Tiny dot tip
        }
        SetPedestalHighlight(false);
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

        // Update held key position every frame
        if (grabbedKey != null)
        {
            // Follow rayTip position every frame without parenting
            grabbedKey.transform.position = rayTip != null ? rayTip.position : mathOrigin + (direction * 1.5f);
            // Don't set rotation — keeps key stable visually
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
            Debug.Log("Hit: " + hit.collider.gameObject.name + " | Tag: " + hit.collider.gameObject.tag);

            lineRenderer.SetPosition(1, hit.point);
            GameObject hitObject = hit.collider.gameObject;

            bool isLookingAtPedestal = (hitObject == futurePedestal || hitObject.transform.IsChildOf(futurePedestal.transform));

            // PRIORITY 1: Holding a key — pedestal interaction takes over everything
            if (grabbedKey != null)
            {
                if (isLookingAtPedestal)
                {
                    SetPedestalHighlight(true);

                    if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                    {
                        DropKeyOnPedestal();
                    }
                }
                else
                {
                    SetPedestalHighlight(false);
                }
                return; // Don't process menu or other highlights while holding
            }

            if (IsAIClock(hitObject) && aiClock != null)
            {
                GameObject clockObj = FindAIClockParent(hitObject);
                Debug.Log("Clock obj found: " + clockObj.name);

                Outline o = clockObj.GetComponent<Outline>();
                Debug.Log("Outline found: " + (o != null) + " | enabled: " + (o != null ? o.enabled.ToString() : "N/A"));

                UpdateHighlight(clockObj);

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
                return;
            }


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

            SetPedestalHighlight(isLookingAtPedestal);

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

    void SetPedestalHighlight(bool isHovering)
    {
        if (futurePedestal == null) return;
        Outline outline = futurePedestal.GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.enabled = isHovering;
            if (isHovering)
            {
                outline.OutlineColor = Color.yellow;
                outline.OutlineWidth = 8f;
            }
        }
    }

    void GrabKey(GameObject key)
    {
        grabbedKey = key;
        if (key.GetComponent<Collider>()) key.GetComponent<Collider>().enabled = false;

        // DON'T parent to rayTip — just follow it via code each frame like Past room does
        // Parenting causes weird rotation inheritance issues
        key.transform.SetParent(null);
        Debug.Log("Future Key Picked Up!");
    }

    void UpdateHighlight(GameObject hitObject)
    {
        Debug.Log("UpdateHighlight called. current: " + (currentHoveredObject?.name ?? "null") + " new: " + hitObject.name);
        if (currentHoveredObject != hitObject)
        {
            ClearHighlight();
            currentHoveredObject = hitObject;
            SetHighlight(currentHoveredObject, true);
            Debug.Log("SetHighlight called with true");
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

    void DropKeyOnPedestal()
    {
        if (grabbedKey == null) return;

        grabbedKey.transform.SetParent(null);

        // Use your existing spawn location empty object
        if (futureDropPoint != null)
        {
            grabbedKey.transform.position = futureDropPoint.position;
            grabbedKey.transform.rotation = futureDropPoint.rotation;
        }
        else
        {
            // Fallback if not assigned
            grabbedKey.transform.position = futurePedestal.transform.position + Vector3.up * 0.1f;
        }

        if (grabbedKey.GetComponent<Collider>())
            grabbedKey.GetComponent<Collider>().enabled = true;

        Debug.Log("Key dropped at future spawn point!");
        grabbedKey = null;
        SetPedestalHighlight(false);
    }

    private bool IsAIClock(GameObject obj)
    {
        return FindAIClockParent(obj) != null;
    }

    private GameObject FindAIClockParent(GameObject obj)
    {
        if (obj.CompareTag("AIClock")) return obj;
        if (obj.transform.parent != null && obj.transform.parent.CompareTag("AIClock")) return obj.transform.parent.gameObject;
        if (obj.transform.root.CompareTag("AIClock")) return obj.transform.root.gameObject;
        return null;
    }
}