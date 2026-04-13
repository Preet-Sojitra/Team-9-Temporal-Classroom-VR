using Fusion;
using UnityEngine;
using System.Threading.Tasks;

public class NetworkLauncher : MonoBehaviour
{
    private NetworkRunner _runner;

    [Header("Voice Chat")]
    [Tooltip("An invisible object with a Speaker component to play remote voices.")]
    public GameObject voiceNetworkPrefab;

    [Header("VR Spawning")]
    [Tooltip("Drag the Character Past GameObject here")]
    public GameObject pastCharacter;
    [Tooltip("Drag the Character Future GameObject here")]
    public GameObject futureCharacter;

    async void Start()
    {
        //start connection when scene loads
        await ConnectToSession();
    }

    private async Task ConnectToSession()
    {
        Debug.Log("Starting Fusion Network Runner...");
        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }
        _runner.ProvideInput = true;

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "TemporalClassroomVoiceTest"
        });

        if (result.Ok)
        {
            Debug.Log("Successfully joined the Temporal Classroom shared session! (Audio/Objects Sync Only)");

            //voice radio
            if (voiceNetworkPrefab != null)
            {
                _runner.Spawn(voiceNetworkPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
            }

            //toggle charecters into different rooms
            if (pastCharacter != null && futureCharacter != null)
            {
                //first player that joins is player1
                if (_runner.SessionInfo.PlayerCount <= 1)
                {
                    //enable Past, disable Future
                    pastCharacter.SetActive(true);
                    futureCharacter.SetActive(false);
                    Debug.Log("Spawned as Player 1 in the Past Room!");
                }
                else
                {
                    //enable Future, disable Past
                    pastCharacter.SetActive(false);
                    futureCharacter.SetActive(true);
                    Debug.Log("Spawned as Player 2 in the Future Room!");
                }
            }
        }
        else
        {
            Debug.LogError($"Failed to start Fusion: {result.ShutdownReason}");

            // FALLBACK: If Android blocks the connection (e.g. during a Microphone Permission popup, or no WiFi),
            // we MUST activate a camera anyway, otherwise Unity will render a pure Black Screen!
            if (pastCharacter != null)
            {
                pastCharacter.SetActive(true);
                if (futureCharacter != null) futureCharacter.SetActive(false);
                Debug.LogWarning("Network Timeout/Error: Spawned Past Room locally as a fallback.");
            }
        }
    }
}
