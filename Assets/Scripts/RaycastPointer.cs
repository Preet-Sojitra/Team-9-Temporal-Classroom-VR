using UnityEngine;

public partial class RaycastPointer : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float raycastLength = 10f;
    public LayerMask interactableLayer; // Added for better performance

    [Header("Line Renderer")]
    public LineRenderer lineRenderer;

    [Header("Visual Offset")]
    [Tooltip("X = Right/Left, Y = Up/Down, Z = Forward/Back")]
    public Vector3 visualOffset = new Vector3(0.2f, -0.2f, 0.1f);
    public bool offsetToRight = true;

    private GameObject currentHoveredObject;
    private ObjectMenu objectMenu;

    void Start()
    {
        // Automatically find the ObjectMenu in the scene so we don't have to assign it manually
        objectMenu = Object.FindFirstObjectByType<ObjectMenu>();

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.05f; // Thicker base
            lineRenderer.endWidth = 0.02f;   // Tapered tip
        }
    }

    void LateUpdate()
    {
        ShootRaycast();
    }

    void ShootRaycast()
    {
        // 1. Math Ray (Calculated from the actual transform)
        Vector3 mathOrigin = transform.position;
        Vector3 direction = transform.forward;
        Ray ray = new Ray(mathOrigin, direction);
        RaycastHit hit;

        // 2. Visual Origin (Local Space)
        float sideDirection = offsetToRight ? 1f : -1f;
        Vector3 localOrigin = new Vector3(visualOffset.x * sideDirection, visualOffset.y, visualOffset.z);

        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPosition(0, localOrigin);

        // 3. Physics Check
        if (Physics.Raycast(ray, out hit, raycastLength))
        {
            lineRenderer.SetPosition(1, lineRenderer.transform.InverseTransformPoint(hit.point));

            GameObject hitObject = hit.collider.gameObject;

            // CASE 1: Menu is OPEN - Handle button hovering and clicking
            if (objectMenu != null && objectMenu.IsMenuOpen())
            {
                objectMenu.HoverButton(hitObject);

                if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                {
                    objectMenu.SelectCurrentButton();
                }
                return; // Don't process world highlights if menu is priority
            }

            // CASE 2: Menu is CLOSED - Handle world interaction
            if (hitObject.CompareTag("Interactable"))
            {
                if (currentHoveredObject != hitObject)
                {
                    ClearHighlight();
                    currentHoveredObject = hitObject;
                    SetHighlight(currentHoveredObject, true);
                }

                // Press X to open menu near remote
                if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                {
                    if (objectMenu != null) objectMenu.OpenMenu(hitObject);
                }
            }
            else
            {
                ClearHighlight();
            }
        }
        else
        {
            // If we hit nothing, draw the line to its maximum length
            Vector3 worldEndPoint = mathOrigin + direction * raycastLength;
            lineRenderer.SetPosition(1, lineRenderer.transform.InverseTransformPoint(worldEndPoint));
            ClearHighlight();
            if (objectMenu != null && objectMenu.IsMenuOpen()) objectMenu.ClearButtonHighlight();
        }
    }

    void SetHighlight(GameObject obj, bool state)
    {
        if (obj.TryGetComponent<Outline>(out var outline))
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