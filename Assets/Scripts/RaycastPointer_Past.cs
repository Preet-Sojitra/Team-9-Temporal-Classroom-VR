using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RaycastPointer_Past : MonoBehaviour
{
    public MonoBehaviour CharacterMovement; // Reference to the player's movement script

    [Header("Raycast Settings")]
    public float raycastLength = 2f;

    [Header("Line Renderer")]
    public LineRenderer lineRenderer;

    [Header("Visual Offset")]
    [Tooltip("X = Right/Left, Y = Up/Down, Z = Forward/Back")]
    public Vector3 visualOffset = new Vector3(0.2f, -0.2f, 0.1f);
    public bool offsetToRight = true;

    [Header("Past Room Reference")]
    public PastKeypadMenu pastMenu;

    [Header("AI Clock")]
    public AIClockManager aiClock;
    private bool isTalkingToClock = false;

    private GameObject currentHoveredObject;

    [Header("Key Interaction")]
    private GameObject grabbedKey = null;
    public Transform rayTip; // Create an empty GameObject at the tip of your ray/hand

    [Header("Teleportation")]
    public GameObject pastPedestal;     // The one in the image
    public Transform futurePedestalPos; // A Transform/Empty at the pedestal in the future
    private Color pedestalDefaultColor = Color.cyan;

    [Header("Timing")]
    public float waitTimeBeforeTeleport = 15.0f;

    public Transform pastDropPoint; // Drag the 'DropPoint' object here in Inspector


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
        // 1. Safety check and movement toggle
        if (pastMenu != null && CharacterMovement != null)
        {
            CharacterMovement.enabled = !pastMenu.IsMenuOpen();
        }

        ShootRaycast();
    }

    void ShootRaycast()
    {
        if (pastMenu == null) return;

        // 1. Math Ray Setup
        Vector3 mathOrigin = transform.position;
        Vector3 direction = transform.forward;
        Ray ray = new Ray(mathOrigin, direction);
        RaycastHit hit;

        // --- 2. HANDLE GRABBED OBJECT POSITION (Always follows you) ---
        if (grabbedKey != null)
        {
            grabbedKey.transform.position = rayTip != null ? rayTip.position : mathOrigin + (direction * 1.5f);
        }

        // 3. Visual Line Renderer Origin (World Space)
        float sideDirection = offsetToRight ? 1f : -1f;
        Vector3 localOrigin = new Vector3(visualOffset.x * sideDirection, visualOffset.y, visualOffset.z);
        Vector3 worldOrigin = lineRenderer.transform.TransformPoint(localOrigin);

        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, worldOrigin);

        // --- 4. PHYSICS CHECK ---
        if (Physics.Raycast(ray, out hit, raycastLength))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name + " | Tag: " + hit.collider.gameObject.tag);

            lineRenderer.SetPosition(1, hit.point);
            GameObject hitObject = hit.collider.gameObject; // hitObject is created HERE

            // DEBUG: Draw a line in the Scene view so you can see where the ray is REALLY hitting
            // Debug.DrawLine(mathOrigin, hit.point, Color.red);
            bool isLookingAtPedestal = (hitObject == pastPedestal || hitObject.transform.IsChildOf(pastPedestal.transform));

            // --- NEW PEDESTAL LOGIC (Inside the hit check) ---
            if (grabbedKey != null)
            {
                // Are we looking at the pedestal?
                if (isLookingAtPedestal)
                {
                    SetPedestalHighlight(true); // Glow Yellow

                    if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                    {
                        Debug.Log("X Pressed while looking at Pedestal!");
                        TeleportKeyToFuture();
                    }
                }
                else
                {
                    SetPedestalHighlight(false); // Back to Cyan
                }
                return; // Don't process other world interactions while holding the key
            }

            // --- NORMAL INTERACTION LOGIC (When not holding a key) ---
            if (pastMenu.IsMenuOpen())
            {
                pastMenu.HoverButton(hitObject);
                if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X)) pastMenu.SelectButton();
                return;
            }

            if (IsAIClock(hitObject) && aiClock != null)
            {
                GameObject clockObj = FindAIClockParent(hitObject);
                UpdateHighlight(clockObj);

                // DEBUG
                Outline o = clockObj.GetComponent<Outline>();
                Outline oChild = clockObj.GetComponentInChildren<Outline>();
                Debug.Log($"Clock: {clockObj.name} | GetComponent Outline: {o != null} | GetComponentInChildren Outline: {oChild != null}");
                if (oChild != null) Debug.Log($"Outline enabled: {oChild.enabled} | on object: {oChild.gameObject.name}");

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
                return;  // ← critical, prevents falling through to ClearHighlight
            }
            else if (hitObject.CompareTag("Key") || hitObject.CompareTag("Interactable"))
            {
                UpdateHighlight(hitObject);
                if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                {
                    if (hitObject.CompareTag("Key")) GrabKey(hitObject);
                    else if (hitObject.name == "chest_close") pastMenu.OpenMenu();
                }
            }
            else
            {
                ClearHighlight();
                SetPedestalHighlight(false);
            }
        }
        else
        {
            // Ray hits nothing
            Vector3 worldEndPoint = mathOrigin + direction * raycastLength;
            lineRenderer.SetPosition(1, worldEndPoint);
            ClearHighlight();
            SetPedestalHighlight(false); // Reset pedestal if we look at the sky

            if (pastMenu.IsMenuOpen()) pastMenu.HoverButton(null);
        }
    }

    // Added this helper to keep the Raycast method clean
    void UpdateHighlight(GameObject hitObject)
    {
        if (currentHoveredObject != hitObject)
        {
            ClearHighlight();
            currentHoveredObject = hitObject;
            SetHighlight(currentHoveredObject, true);
        }
    }

    void SetPedestalHighlight(bool isHovering)
    {
        if (pastPedestal == null) return;

        // Search in children too, not just root
        Outline outline = pastPedestal.GetComponentInChildren<Outline>();

        if (outline != null)
        {
            if (isHovering)
            {
                outline.enabled = true;
                outline.OutlineColor = Color.yellow;
                outline.OutlineWidth = 8f;
            }
            else
            {
                outline.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("No Outline component found on pedestal or its children!");
        }
    }

    void TeleportKeyToFuture()
    {
        // Start the sequence
        StartCoroutine(KeyTeleportSequence());
    }

    System.Collections.IEnumerator KeyTeleportSequence()
    {
        GameObject keyToMove = grabbedKey;
        grabbedKey = null;

        keyToMove.transform.position = pastDropPoint.position;
        keyToMove.transform.rotation = pastDropPoint.rotation;
        keyToMove.transform.SetParent(null);

        yield return new WaitForSeconds(waitTimeBeforeTeleport);

        // Retry finding it for up to 5 seconds in case it spawned late
        KeyTeleportHack hack = null;
        float searchTimeout = 5f;
        while (hack == null && searchTimeout > 0f)
        {
            hack = FindObjectOfType<KeyTeleportHack>();
            if (hack == null)
            {
                searchTimeout -= Time.deltaTime;
                yield return null;
            }
        }

        if (hack != null)
        {
            hack.RequestTeleport();
        }
        else
        {
            Debug.LogError("KeyTeleportHack not found after waiting! Was the prefab spawned by MasterClient?");
        }
    }

    void GrabKey(GameObject key)
    {
        grabbedKey = key;
        // Disable collider so it doesn't hit itself with the raycast
        if (key.GetComponent<Collider>()) key.GetComponent<Collider>().enabled = false;

        // Optional: Make it a child of the camera/hand so it moves perfectly
        key.transform.SetParent(this.transform);
        Debug.Log("Key Attached to Player!");
    }

    // void SetHighlight(GameObject obj, bool state)
    // {
    //     if (obj != null && obj.TryGetComponent<Outline>(out var outline))
    //     {
    //         outline.enabled = state;
    //     }
    // }

    void SetHighlight(GameObject obj, bool state)
    {
        Outline outline = obj.GetComponent<Outline>() ?? obj.GetComponentInChildren<Outline>();
        if (outline != null)
            outline.enabled = state;
    }

    void ClearHighlight()
    {
        if (currentHoveredObject != null)
        {
            SetHighlight(currentHoveredObject, false);
            currentHoveredObject = null;
        }
    }

    private void OnDisable()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
        ClearHighlight();
    }

    private void OnEnable()
    {
        if (lineRenderer != null) lineRenderer.enabled = true;
    }

    // private bool IsAIClock(GameObject obj)
    // {
    //     // Check the hit object itself
    //     if (obj.CompareTag("AIClock")) return true;
    //     // Check parent (since raycast hits child meshes like obj1, obj2, etc.)
    //     if (obj.transform.parent != null && obj.transform.parent.CompareTag("AIClock")) return true;
    //     // Check root
    //     if (obj.transform.root.CompareTag("AIClock")) return true;
    //     return false;
    // }

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