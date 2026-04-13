using UnityEngine;

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
        if (pastPedestal != null)
        {
            if (pastPedestal.TryGetComponent<Outline>(out var outline))
            {
                outline.enabled = true;
                pedestalDefaultColor = outline.OutlineColor; // Remember the starting color
                outline.OutlineWidth = 2f;
            }
        }

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

    // void ShootRaycast()
    // {
    //     if (pastMenu == null) return;

    //     // 1. Math Ray
    //     Vector3 mathOrigin = transform.position;
    //     Vector3 direction = transform.forward;
    //     Ray ray = new Ray(mathOrigin, direction);
    //     RaycastHit hit;

    //     // --- 1. HANDLE GRABBED OBJECT FIRST ---
    //     // This moves the key EVERY frame, whether the ray hits a wall or not.
    //     if (grabbedKey != null)
    //     {
    //         // Position the key at rayTip while holding
    //         grabbedKey.transform.position = rayTip != null ? rayTip.position : mathOrigin + (direction * 1.5f);

    //         // Are we looking at the pedestal?
    //         if (hitObject == pastPedestal || hitObject.transform.IsChildOf(pastPedestal.transform))
    //         {
    //             // 1. Give Visual Feedback (Yellow Outline)
    //             SetPedestalHighlight(true);

    //             // 2. Handle the Drop
    //             if (Input.GetButtonDown("js10") || Input.GetKeyDown(KeyCode.X))
    //             {
    //                 TeleportKeyToFuture();
    //             }
    //         }
    //         else
    //         {
    //             // Not looking at pedestal? Reset color
    //             SetPedestalHighlight(false);
    //         }
    //         return; // Exit so we don't highlight other objects while holding the key
    //     }

    //     // 2. Visual Origin logic
    //     float sideDirection = offsetToRight ? 1f : -1f;
    //     Vector3 visualOrigin = mathOrigin
    //                         + (transform.right * visualOffset.x * sideDirection)
    //                         + (transform.up * visualOffset.y)
    //                         + (transform.forward * visualOffset.z);

    //     lineRenderer.useWorldSpace = true;
    //     lineRenderer.SetPosition(0, visualOrigin);


    //     // 3. Physics Check
    //     if (Physics.Raycast(ray, out hit, raycastLength))
    //     {
    //         lineRenderer.SetPosition(1, hit.point);
    //         GameObject hitObject = hit.collider.gameObject;


    //         if (grabbedKey != null)
    //         {
    //             // Position the key at rayTip while holding
    //             grabbedKey.transform.position = rayTip != null ? rayTip.position : mathOrigin + (direction * 2f);

    //             // If looking at the pedestal and pressing X
    //             if (hitObject == pastPedestal)
    //             {
    //                 if (Input.GetButtonDown("js10") || Input.GetKeyDown(KeyCode.X))
    //                 {
    //                     TeleportKeyToFuture();
    //                 }
    //             }
    //             return; // Don't process other highlights while holding the key
    //         }

    //         if (Input.GetKey(KeyCode.F)) // Hold D while playing to see hits
    //         {
    //             Debug.Log("Ray currently hitting: " + hitObject.name + " on Layer: " + hitObject.layer);
    //         }


    //         // CASE 1: Keypad Menu is OPEN
    //         if (pastMenu.IsMenuOpen())
    //         {
    //             pastMenu.HoverButton(hitObject);

    //             if (Input.GetButtonDown("js10") || Input.GetKeyDown(KeyCode.X))
    //             {
    //                 pastMenu.SelectButton();
    //             }
    //             return; // Prioritize menu over world highlights
    //         }

    //         if (hitObject.CompareTag("Key"))
    //         {
    //             if (currentHoveredObject != hitObject)
    //             {
    //                 ClearHighlight();
    //                 currentHoveredObject = hitObject;
    //                 SetHighlight(currentHoveredObject, true);
    //                 Debug.Log("HIT THE KEY!");
    //             }

    //             if (Input.GetButtonDown("js10") || Input.GetKeyDown(KeyCode.X))
    //             {
    //                 GrabKey(hitObject);
    //             }
    //         }
    //         // CASE 2: Menu is CLOSED - World interaction (Chest, Key, etc.)
    //         else if (hitObject.CompareTag("Interactable"))
    //         {
    //             if (currentHoveredObject != hitObject)
    //             {
    //                 ClearHighlight();
    //                 currentHoveredObject = hitObject;
    //                 SetHighlight(currentHoveredObject, true);
    //                 Debug.Log("Highlighting Interactable: " + hitObject.name);
    //             }

    //             // Press X to open the keypad menu
    //             if (Input.GetButtonDown("js10") || Input.GetKeyDown(KeyCode.X))
    //             {
    //                 Debug.Log("X Pressed on: " + hitObject.name);
    //                 // Specifically check if we are looking at the closed chest
    //                 if (hitObject.name == "chest_close")
    //                 {
    //                     pastMenu.OpenMenu();
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             ClearHighlight();
    //         }
    //     }
    //     else
    //     {
    //         SetPedestalHighlight(false);
    //         // Ray hits nothing
    //         Vector3 endPoint = mathOrigin + direction * raycastLength;
    //         lineRenderer.SetPosition(1, endPoint);

    //         // If nothing is hit but we have a key, move key to the end of the ray
    //         if (grabbedKey != null)
    //         {
    //             grabbedKey.transform.position = endPoint;
    //         }

    //         ClearHighlight();

    //         if (pastMenu.IsMenuOpen())
    //         {
    //             pastMenu.HoverButton(null);
    //         }
    //     }
    // }

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
            lineRenderer.SetPosition(1, hit.point);
            GameObject hitObject = hit.collider.gameObject; // hitObject is created HERE

            // DEBUG: Draw a line in the Scene view so you can see where the ray is REALLY hitting
            Debug.DrawLine(mathOrigin, hit.point, Color.red);
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

            if (hitObject.CompareTag("Key") || hitObject.CompareTag("Interactable"))
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
        if (pastPedestal != null && pastPedestal.TryGetComponent<Outline>(out var outline))
        {
            outline.OutlineColor = isHovering ? Color.yellow : pedestalDefaultColor;
            // Optional: make the outline thicker when hovering
            outline.OutlineWidth = isHovering ? 8f : 4f;
        }
    }

    void TeleportKeyToFuture()
    {
        // Start the sequence
        StartCoroutine(KeyTeleportSequence());
    }

    // System.Collections.IEnumerator KeyTeleportSequence()
    // {
    //     GameObject keyToMove = grabbedKey;
    //     grabbedKey = null;

    //     keyToMove.transform.SetParent(null);

    //     // Position on Past Pedestal
    //     if (pastDropPoint != null)
    //     {
    //         keyToMove.transform.position = pastDropPoint.position;
    //     }
    //     else
    //     {
    //         keyToMove.transform.position = pastPedestal.transform.position + Vector3.up * 0.8f;
    //     }

    //     // Ensure physics doesn't make it fall through the floor
    //     Rigidbody rb = keyToMove.GetComponent<Rigidbody>();
    //     if (rb != null) rb.isKinematic = true;

    //     yield return new WaitForSeconds(waitTimeBeforeTeleport);

    //     // TELEPORT TO FUTURE
    //     Debug.Log("Teleporting Key to Future Pedestal!");
    //     keyToMove.transform.position = futurePedestalPos.position;
    //     keyToMove.transform.rotation = futurePedestalPos.rotation;

    //     // Optional: Re-enable physics in the future if needed
    //     // if (rb != null) rb.isKinematic = false; 
    // }

    System.Collections.IEnumerator KeyTeleportSequence()
    {
        GameObject keyToMove = grabbedKey;
        grabbedKey = null;

        keyToMove.transform.SetParent(null);

        // Use the explicit drop point we created visually
        if (pastDropPoint != null)
        {
            keyToMove.transform.position = pastDropPoint.position;
            keyToMove.transform.rotation = pastDropPoint.rotation;
        }
        else
        {
            // Fallback: Use a much higher offset if you forgot to assign the point
            keyToMove.transform.position = pastPedestal.transform.position + Vector3.up * 1.5f;
        }

        if (keyToMove.GetComponent<Collider>())
            keyToMove.GetComponent<Collider>().enabled = true;

        // Now bring back the wait and teleport
        yield return new WaitForSeconds(waitTimeBeforeTeleport);

        keyToMove.transform.position = futurePedestalPos.position;
        keyToMove.transform.rotation = futurePedestalPos.rotation;
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

    private void OnDisable()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
        ClearHighlight();
    }

    private void OnEnable()
    {
        if (lineRenderer != null) lineRenderer.enabled = true;
    }
}