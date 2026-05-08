using UnityEngine;
using Fusion;

public class PlayerPOVSync : NetworkBehaviour
{
    [Networked] private Vector3 SyncedPosition { get; set; }
    [Networked] private Vector3 SyncedEulerRotation { get; set; }

    private Transform myLocalCamera;
    private Transform otherRoomPOVCamera;
    private Camera targetCameraComponent;

    public override void Spawned()
    {
        string role = string.IsNullOrEmpty(LobbyData.SelectedRole) ? "Past" : LobbyData.SelectedRole;
        bool isPast = role == "Past";

        // Find the POV cameras globally
        GameObject pastPOV = FindInactiveByName("Pov_camera_past");
        GameObject futurePOV = FindInactiveByName("Pov_camera_future");

        if (isPast)
        {
            // I am in the Past room. I track my headset (Main Camera)...
            myLocalCamera = pastPOV?.transform.parent;
            // ...and I want to update the camera the Future player is looking at.
            otherRoomPOVCamera = futurePOV?.transform;
        }
        else
        {
            // I am in the Future room. I track my headset (Main Camera)...
            myLocalCamera = futurePOV?.transform.parent;
            // ...and I want to update the camera the Past player is looking at.
            otherRoomPOVCamera = pastPOV?.transform;
        }

        if (otherRoomPOVCamera != null)
        {
            targetCameraComponent = otherRoomPOVCamera.GetComponent<Camera>();
            // Force the camera to be enabled even if its parent is disabled
            if (targetCameraComponent != null) targetCameraComponent.enabled = true;
        }

        Debug.Log($"[POV Sync] Role: {role} | Authority: {Object.HasStateAuthority}");
    }

    public override void FixedUpdateNetwork()
    {
        // Only the owner of this player object writes to the network
        if (Object.HasStateAuthority && myLocalCamera != null)
        {
            SyncedPosition = myLocalCamera.position;
            SyncedEulerRotation = myLocalCamera.eulerAngles;
        }
    }

    void LateUpdate()
    {
        // Everyone who ISN'T the owner updates the target camera's position
        if (!Object.HasStateAuthority && otherRoomPOVCamera != null)
        {
            otherRoomPOVCamera.position = SyncedPosition;
            otherRoomPOVCamera.rotation = Quaternion.Euler(SyncedEulerRotation);
        }
    }

    private GameObject FindInactiveByName(string name)
    {
        Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in all)
        {
            if (t.hideFlags == HideFlags.None && t.name == name) return t.gameObject;
        }
        return null;
    }
}