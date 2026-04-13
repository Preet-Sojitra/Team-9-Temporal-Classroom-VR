using Fusion;
using UnityEngine;
using System.Threading.Tasks;

public class NetworkLauncher : MonoBehaviour
{
    private NetworkRunner _runner;
    private int _maxPlayersSeen = 0;

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

    void Update()
    {
        // Check if anyone disconnected mid-game
        if (_runner != null && _runner.IsRunning && _runner.SessionInfo != null)
        {
            // Record maximum lobby size
            if (_runner.SessionInfo.PlayerCount > _maxPlayersSeen)
            {
                _maxPlayersSeen = _runner.SessionInfo.PlayerCount;
            }

            // If we successfully had 2 players, and now we only have 1 (meaning the other player quit/crashed)
            if (_maxPlayersSeen == 2 && _runner.SessionInfo.PlayerCount < 2)
            {
                Debug.LogWarning("Player 2 disconnected! Force rebooting the game...");
                _maxPlayersSeen = 0; 
                
                // Nuke the session and reload the entire scene to force a totally clean restart for everyone!
                _runner.Shutdown();
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
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
                // The server creator is always Player 1
                if (_runner.IsSharedModeMasterClient)
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
