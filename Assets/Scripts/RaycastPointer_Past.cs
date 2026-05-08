using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RaycastPointer_Past : MonoBehaviour
{
    public MonoBehaviour CharacterMovement;

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
    public Transform rayTip;

    [Header("Teleportation")]
    public GameObject pastPedestal;
    public Transform futurePedestalPos;
    private Color pedestalDefaultColor = Color.cyan;

    [Header("Timing")]
    public float waitTimeBeforeTeleport = 15.0f;

    public Transform pastDropPoint;


    void Start()
    {

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.015f;
            lineRenderer.endWidth = 0.005f;
        }
    }



    void LateUpdate()
    {
        if (pastMenu != null && CharacterMovement != null)
        {
            CharacterMovement.enabled = !pastMenu.IsMenuOpen();
        }

        ShootRaycast();
    }

    void ShootRaycast()
    {
        if (pastMenu == null) return;

        Vector3 mathOrigin = transform.position;
        Vector3 direction = transform.forward;
        Ray ray = new Ray(mathOrigin, direction);
        RaycastHit hit;

        if (grabbedKey != null)
        {
            grabbedKey.transform.position = rayTip != null ? rayTip.position : mathOrigin + (direction * 1.5f);
        }

        float sideDirection = offsetToRight ? 1f : -1f;
        Vector3 localOrigin = new Vector3(visualOffset.x * sideDirection, visualOffset.y, visualOffset.z);
        Vector3 worldOrigin = lineRenderer.transform.TransformPoint(localOrigin);

        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, worldOrigin);

        if (Physics.Raycast(ray, out hit, raycastLength))
        {
            // Debug.Log("Hit: " + hit.collider.gameObject.name + " | Tag: " + hit.collider.gameObject.tag);

            lineRenderer.SetPosition(1, hit.point);
            GameObject hitObject = hit.collider.gameObject;

            // Debug.DrawLine(mathOrigin, hit.point, Color.red);
            bool isLookingAtPedestal = (hitObject == pastPedestal || hitObject.transform.IsChildOf(pastPedestal.transform));

            if (grabbedKey != null)
            {
                if (isLookingAtPedestal)
                {
                    SetPedestalHighlight(true);

                    if (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X))
                    {
                        // Debug.Log("X Pressed while looking at Pedestal!");
                        TeleportKeyToFuture();
                    }
                }
                else
                {
                    SetPedestalHighlight(false);
                }
                return;
            }

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
                // Outline o = clockObj.GetComponent<Outline>();
                // Outline oChild = clockObj.GetComponentInChildren<Outline>();
                // Debug.Log($"Clock: {clockObj.name} | GetComponent Outline: {o != null} | GetComponentInChildren Outline: {oChild != null}");
                // if (oChild != null) Debug.Log($"Outline enabled: {oChild.enabled} | on object: {oChild.gameObject.name}");

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
            Vector3 worldEndPoint = mathOrigin + direction * raycastLength;
            lineRenderer.SetPosition(1, worldEndPoint);
            ClearHighlight();
            SetPedestalHighlight(false);

            if (pastMenu.IsMenuOpen()) pastMenu.HoverButton(null);
        }
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

    void SetPedestalHighlight(bool isHovering)
    {
        if (pastPedestal == null) return;

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
        if (key.GetComponent<Collider>()) key.GetComponent<Collider>().enabled = false;

        key.transform.SetParent(this.transform);
        // Debug.Log("Key Attached to Player!");
    }


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