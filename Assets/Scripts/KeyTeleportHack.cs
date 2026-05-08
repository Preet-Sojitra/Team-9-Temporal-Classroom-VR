using Fusion;
using UnityEngine;

public class KeyTeleportHack : NetworkBehaviour
{
    [Header("Key References")]
    public GameObject pastKey;
    public GameObject futureKey;

    [Networked]
    [OnChangedRender(nameof(UpdateKeyVisibility))]
    public NetworkBool isKeyInFuture { get; set; }

    public void RequestTeleport()
    {
        if (Object == null || !Object.IsValid)
        {
            // Debug.LogWarning("Fusion Object not valid — swapping locally only.");
            LocalManualSwap(true);
            return;
        }

        // Debug.Log($"RequestTeleport called. HasStateAuthority={Object.HasStateAuthority}");

        if (Object.HasStateAuthority)
        {
            isKeyInFuture = true;
        }
        else
        {
            Object.RequestStateAuthority();
            StartCoroutine(SetAfterAuthority());
        }
    }

    private System.Collections.IEnumerator SetAfterAuthority()
    {
        float timeout = 3f;
        while (!Object.HasStateAuthority && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (Object.HasStateAuthority)
        {
            isKeyInFuture = true;
            // Debug.Log("Authority granted — key teleported network-wide.");
        }
        else
        {
            // Debug.LogWarning("Authority timeout — sending RPC as fallback.");
            RPC_SetKeyStatus(true);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetKeyStatus(NetworkBool inFuture)
    {
        isKeyInFuture = inFuture;
    }

    public void UpdateKeyVisibility()
    {
        LocalManualSwap(isKeyInFuture);
    }

    private void LocalManualSwap(bool inFuture)
    {
        if (pastKey != null) pastKey.SetActive(!inFuture);
        if (futureKey != null) futureKey.SetActive(inFuture);
        // Debug.Log($"Key Swap — inFuture: {inFuture}");
    }

    public override void Spawned()
    {
        pastKey = FindInactiveByName("rust_key_past");
        futureKey = FindInactiveByName("rust_key_future");

        // Debug.Log($"KeyTeleportHack Spawned. pastKey={pastKey}, futureKey={futureKey}");
    }

    private GameObject FindInactiveByName(string name)
    {
        Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in all)
        {
            if (t.hideFlags != HideFlags.None) continue;
            if (t.name == name) return t.gameObject;
        }
        // Debug.LogWarning($"Could not find: {name}");
        return null;
    }

    // void Start()
    // {
    // Debug.Log($"KeyTeleportHack Start — Object null? {Object == null}");
    // if (Object != null)
    // Debug.Log($"Object.IsValid={Object.IsValid}, HasStateAuthority={Object.HasStateAuthority}");
    // }
}