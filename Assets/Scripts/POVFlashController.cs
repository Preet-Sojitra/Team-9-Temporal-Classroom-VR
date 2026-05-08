using System.Collections;
using UnityEngine;

public class POVFlashController : MonoBehaviour
{
    public GameObject projectorScreen;
    public GameObject blackboardScreen;

    public float minInterval = 5f;
    public float maxInterval = 10f;
    public float flashDuration = 3.5f;

    void Start()
    {
        SetScreensVisible(false);
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
            SetScreensVisible(true);
            yield return new WaitForSeconds(flashDuration);
            SetScreensVisible(false);
        }
    }

    void SetScreensVisible(bool visible)
    {
        if (projectorScreen != null) projectorScreen.SetActive(visible);
        if (blackboardScreen != null) blackboardScreen.SetActive(visible);
    }
}