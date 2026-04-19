using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the black 'Blindfold' Canvas here")]
    public GameObject loadingOverlay;

    void Start()
    {
        // Ensure the loading screen is hidden when the lobby starts
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(false);
        }
    }

    void Update()
    {
        // Check for role selection keys
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartGameSequence("Past");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            StartGameSequence("Future");
        }
    }

    public void StartGameSequence(string role)
    {
        // Debug.Log($"Role Selected: {role}. Starting Async Load...");

        // 1. Save the choice to the static class
        LobbyData.SelectedRole = role;

        // 2. Show the "Blindfold" immediately to the player
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(true);
        }

        // 3. Start the background loading process
        StartCoroutine(LoadAsyncScene());
    }

    private IEnumerator LoadAsyncScene()
    {
        // Use LoadSceneAsync to keep the Lobby active while Main Scene loads
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main Scene");

        // While the scene is loading, we can stay here. 
        // Unity switches automatically once asyncLoad.isDone is true.
        while (!asyncLoad.isDone)
        {
            // If you wanted a progress bar, you'd use (asyncLoad.progress) here
            yield return null;
        }

        // Debug.Log("Scene Loaded! Transitioning...");
    }
}

// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class LobbyController : MonoBehaviour
// {
//     void Update()
//     {
//         // Testing via Keyboard
//         if (Input.GetKeyDown(KeyCode.P))
//         {
//             Debug.Log("Selected: PAST");
//             LobbyData.SelectedRole = "Past";
//             EnterGame();
//         }

//         if (Input.GetKeyDown(KeyCode.F))
//         {
//             Debug.Log("Selected: FUTURE");
//             LobbyData.SelectedRole = "Future";
//             EnterGame();
//         }
//     }

//     void EnterGame()
//     {
//         // Make sure "Main Scene" is the exact name of your game scene
//         SceneManager.LoadScene("Main Scene");
//     }
// }