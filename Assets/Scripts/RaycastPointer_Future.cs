using UnityEngine;

public class RaycastPointerFuture : MonoBehaviour
{
    public CharacterMovement CharacterMovement;

    [Header("Raycast Settings")]
    public float raycastLength = 10f;

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
    public Transform rayTip; // Same setup as Past room for holding the key

    void Update()
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

        // 2. Visual Line Setup
        float sideDirection = offsetToRight ? 1f : -1f;
        Vector3 visualOrigin = mathOrigin
                            + (transform.right * visualOffset.x * sideDirection)
                            + (transform.up * visualOffset.y)
                            + (transform.forward * visualOffset.z);

        lineRenderer.SetPosition(0, visualOrigin);

        // 3. Physics Check
        if (Physics.Raycast(ray, out hit, raycastLength))
        {
            lineRenderer.SetPosition(1, hit.point);
            GameObject hitObject = hit.collider.gameObject;

            // Priority 1: The Menu
            if (objectMenu != null && objectMenu.IsMenuOpen())
            {
                objectMenu.HoverButton(hitObject);
                if (Input.GetButtonDown("js10") || Input.GetKeyDown(KeyCode.X))
                {
                    objectMenu.SelectCurrentButton();
                }
                return;
            }

            // Priority 2: World Objects (Key or Interactables)
            if (hitObject.CompareTag("Key") || hitObject.CompareTag("Interactable"))
            {
                UpdateHighlight(hitObject);

                if (Input.GetButtonDown("js10") || Input.GetKeyDown(KeyCode.X))
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
            lineRenderer.SetPosition(1, mathOrigin + direction * raycastLength);
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
}