using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))] // Ensures a LineRenderer is attached
public class LobbyRaycaster : MonoBehaviour
{
    public float rayDistance = 5f;
    public LayerMask uiLayer;

    private Button currentButton;
    private UnityEngine.UI.Outline currentOutline;

    [Header("Line Renderer")]
    public LineRenderer lineRenderer;

    [Header("Visual Offset")]
    public Vector3 visualOffset = new Vector3(0.2f, -0.2f, 0.1f);
    public bool offsetToRight = true;

    void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.015f; // Standard VR laser width
            lineRenderer.endWidth = 0.005f;   // Tiny dot tip
        }
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // This draws a GREEN line in the SCENE VIEW so you can see the actual physics ray
        // Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.green);

        // Offset the visual origin like the working reference
        float sideDirection = offsetToRight ? 1f : -1f;
        Vector3 localOrigin = new Vector3(visualOffset.x * sideDirection, visualOffset.y, visualOffset.z);
        Vector3 worldOrigin = lineRenderer.transform.TransformPoint(localOrigin);

        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, worldOrigin);

        if (Physics.Raycast(ray, out hit, rayDistance, uiLayer))
        {
            // Debug.Log("Ray HIT something: " + hit.collider.name);
            lineRenderer.SetPosition(1, hit.point);

            // 1. Handle Hover Visuals
            Button btn = hit.collider.GetComponent<Button>();
            if (btn != null && btn != currentButton)
            {
                ClearHover();
                currentButton = btn;

                // Get the native Unity UI Outline
                currentOutline = currentButton.GetComponent<UnityEngine.UI.Outline>();

                if (currentOutline != null)
                {
                    currentOutline.enabled = true;
                }
            }

            // 2. Handle Click
            if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
            {
                // Debug.Log("X Pressed while hitting: " + hit.collider.name);
                if (currentButton != null) currentButton.onClick.Invoke();
            }
        }
        else
        {
            lineRenderer.SetPosition(1, transform.position + (transform.forward * rayDistance));
            ClearHover();
        }
    }

    void ClearHover()
    {
        if (currentOutline != null) currentOutline.enabled = false;
        currentButton = null;
        currentOutline = null;
    }
}