using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
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
            lineRenderer.startWidth = 0.015f;
            lineRenderer.endWidth = 0.005f;
        }
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.green);

        float sideDirection = offsetToRight ? 1f : -1f;
        Vector3 localOrigin = new Vector3(visualOffset.x * sideDirection, visualOffset.y, visualOffset.z);
        Vector3 worldOrigin = lineRenderer.transform.TransformPoint(localOrigin);

        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, worldOrigin);

        if (Physics.Raycast(ray, out hit, rayDistance, uiLayer))
        {
            // Debug.Log("Ray HIT something: " + hit.collider.name);
            lineRenderer.SetPosition(1, hit.point);

            Button btn = hit.collider.GetComponent<Button>();
            if (btn != null && btn != currentButton)
            {
                ClearHover();
                currentButton = btn;

                currentOutline = currentButton.GetComponent<UnityEngine.UI.Outline>();

                if (currentOutline != null)
                {
                    currentOutline.enabled = true;
                }
            }

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