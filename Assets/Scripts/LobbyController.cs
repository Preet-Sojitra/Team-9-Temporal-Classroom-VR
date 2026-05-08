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
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(false);
        }
    }

    void Update()
    {
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

        LobbyData.SelectedRole = role;

        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(true);
        }

        StartCoroutine(LoadAsyncScene());
    }

    private IEnumerator LoadAsyncScene()
    {

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main Scene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
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