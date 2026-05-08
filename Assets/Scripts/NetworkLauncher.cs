using Fusion;
using UnityEngine;
using System.Threading.Tasks;

public class NetworkLauncher : MonoBehaviour
{
    private NetworkRunner _runner;
    private int _maxPlayersSeen = 0;
    private bool _partnerDisconnected = false;

    [Header("Voice Chat")]
    [Tooltip("An invisible object with a Speaker component to play remote voices.")]
    public GameObject voiceNetworkPrefab;

    [Header("VR Spawning")]
    [Tooltip("Drag the Character Past GameObject here")]
    public GameObject pastCharacter;
    [Tooltip("Drag the Character Future GameObject here")]
    public GameObject futureCharacter;

    [Header("Key Teleport")]
    public GameObject keyTeleportPrefab;

    [Header("POV Sync")]
    public Transform pastMainCamera;    // drag Past room Main Camera here
    public Transform futureMainCamera;  // drag Future room Main Camera here
    public Transform pastPOVCamera;     // drag Past room POV camera here
    public Transform futurePOVCamera;   // drag Future room POV camera here

    async void Start()
    {
        await ConnectToSession();
    }

    void Update()
    {
        if (_runner != null && _runner.IsRunning && _runner.SessionInfo != null)
        {
            int currentPlayers = _runner.SessionInfo.PlayerCount;

            if (currentPlayers > _maxPlayersSeen)
            {
                _maxPlayersSeen = currentPlayers;
            }

            if (_maxPlayersSeen == 2 && currentPlayers < 2)
            {
                _partnerDisconnected = true;
            }

            if (_partnerDisconnected && currentPlayers == 2)
            {
                Debug.LogWarning("Player 2 disconnected! Force rebooting the game...");
                _maxPlayersSeen = 0;

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

            if (voiceNetworkPrefab != null)
            {
                // _runner.Spawn(voiceNetworkPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
                var spawnedVoice = _runner.Spawn(voiceNetworkPrefab, Vector3.zero,
                       Quaternion.identity, _runner.LocalPlayer);

            }
            if (keyTeleportPrefab != null && _runner.IsSharedModeMasterClient)
            {
                _runner.Spawn(keyTeleportPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
            }

            if (pastCharacter != null && futureCharacter != null)
            {

                // --- NEW LOGIC: PROTECT THE POV CAMERAS ---
                // We unparent them so they don't get disabled when we hide the characters.
                if (pastPOVCamera != null) pastPOVCamera.SetParent(null);
                if (futurePOVCamera != null) futurePOVCamera.SetParent(null);

                // Ensure the cameras themselves are enabled
                if (pastPOVCamera != null) pastPOVCamera.gameObject.SetActive(true);
                if (futurePOVCamera != null) futurePOVCamera.gameObject.SetActive(true);
                // ------------------------------------------

                if (LobbyData.SelectedRole == "Past")
                {
                    pastCharacter.SetActive(true);
                    futureCharacter.SetActive(false); // Now this won't kill the futurePOVCamera!
                    Debug.Log("Spawned in the PAST Room. Future POV remains active.");
                }
                else if (LobbyData.SelectedRole == "Future")
                {
                    pastCharacter.SetActive(false); // Now this won't kill the pastPOVCamera!
                    futureCharacter.SetActive(true);
                    Debug.Log("Spawned in the FUTURE Room. Past POV remains active.");
                }
                // if (LobbyData.SelectedRole == "Past")
                // {
                //     pastCharacter.SetActive(true);
                //     futureCharacter.SetActive(false);
                //     Debug.Log("Spawned in the PAST Room based on Lobby choice.");
                // }
                // else if (LobbyData.SelectedRole == "Future")
                // {
                //     pastCharacter.SetActive(false);
                //     futureCharacter.SetActive(true);
                //     Debug.Log("Spawned in the FUTURE Room based on Lobby choice.");
                // }
                else
                {
                    Debug.LogWarning("No choice detected! Defaulting to MasterClient logic.");
                    bool isMaster = _runner.IsSharedModeMasterClient;
                    pastCharacter.SetActive(isMaster);
                    futureCharacter.SetActive(!isMaster);
                }
            }

            var activeChar = pastCharacter.activeSelf ? pastCharacter : futureCharacter;

            // Freeze movement immediately so they can't wander around blind
            if (activeChar != null)
            {
                var moveScript = activeChar.GetComponentInChildren<CharacterMovement>(true);
                if (moveScript != null) moveScript.isFrozen = true;
            }

            Debug.Log("Waiting for Player 2 to join before starting...");
            // Wait for both players to be fully connected
            while (_runner != null && _runner.IsRunning && _runner.SessionInfo != null && _runner.SessionInfo.PlayerCount < 2)
            {
                await Task.Delay(500);
            }
            Debug.Log("Both players synced! Starting game.");

            // Unfreeze movement and remove blindfold
            if (activeChar != null)
            {
                var moveScript = activeChar.GetComponentInChildren<CharacterMovement>(true);
                if (moveScript != null) moveScript.isFrozen = false;

                var canvas = activeChar.GetComponentInChildren<Canvas>(true);
                if (canvas != null) canvas.gameObject.SetActive(false);
            }

            //toggle charecters into different rooms
            // if (pastCharacter != null && futureCharacter != null)
            // {
            //     // The server creator is always Player 1
            //     if (_runner.IsSharedModeMasterClient)
            //     {
            //         //enable Past, disable Future
            //         pastCharacter.SetActive(true);
            //         futureCharacter.SetActive(false);
            //         Debug.Log("Spawned as Player 1 in the Past Room!");
            //     }
            //     else
            //     {
            //         //enable Future, disable Past
            //         pastCharacter.SetActive(false);
            //         futureCharacter.SetActive(true);
            //         Debug.Log("Spawned as Player 2 in the Future Room!");
            //     }
            // }
        }
        else
        {
            Debug.LogError($"Failed to start Fusion: {result.ShutdownReason}");
            if (pastCharacter != null)
            {
                pastCharacter.SetActive(true);
                if (futureCharacter != null) futureCharacter.SetActive(false);
                Debug.LogWarning("Network Timeout/Error: Spawned Past Room locally as a fallback.");
            }
        }
    }
}
