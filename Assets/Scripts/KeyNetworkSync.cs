using Photon.Pun;
using UnityEngine;

public class KeyNetworkSync : MonoBehaviourPun
{
    [PunRPC]
    public void TeleportToFuture(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        
        // Re-enable collider on all clients
        if (GetComponent<Collider>())
            GetComponent<Collider>().enabled = true;
            
        Debug.Log("Key teleported on: " + PhotonNetwork.NickName);
    }
}