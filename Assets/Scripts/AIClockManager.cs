using UnityEngine;
using Fusion;
using System.Collections;
using TMPro;

/// <summary>
/// The AI Clock Companion. Place this on the Clock GameObject.
/// Requires GroqLLMClient, FreeTTSClient, and GroqWhisperClient on the same object.
/// Tag the Clock GameObject as "AIClock" so raycast pointers can detect it.
/// </summary>
[RequireComponent(typeof(GroqLLMClient))]
[RequireComponent(typeof(FreeTTSClient))]
[RequireComponent(typeof(GroqWhisperClient))]
[RequireComponent(typeof(AudioSource))]
public class AIClockManager : NetworkBehaviour
{
    [Header("Timer Settings")]
    public float escapeTimeSeconds = 420f; // 7 minutes
    [Networked] public float CurrentTime { get; set; }
    [Networked] public NetworkBool TimerStarted { get; set; }

    [Header("UI (Optional)")]
    public TMP_Text clockTextDisplay;
    public TMP_Text subtitleDisplay; // Shows what the clock says as text

    [Header("Random Torment Settings")]
    [Tooltip("Minimum seconds between random taunts")]
    public float minTormentInterval = 60f;
    [Tooltip("Maximum seconds between random taunts")]
    public float maxTormentInterval = 120f;

    private GroqLLMClient llmClient;
    private FreeTTSClient ttsClient;
    private GroqWhisperClient whisperClient;

    private bool isBusy = false; // Prevents overlapping conversations
    private Coroutine tormentCoroutine;
    private bool countdownStarted = false;
    private int lastAnnouncedSecond = -1;

    // ============================================================
    //  PHOTON FUSION LIFECYCLE
    // ============================================================

    private bool isInitialized = false;
    private float localTimer; // Fallback timer when no network

    /// <summary>
    /// Safe accessor that never throws even before Spawned().
    /// </summary>
    private float SafeCurrentTime
    {
        get
        {
            if (isInitialized && Object != null && Object.IsValid)
            {
                return CurrentTime;
            }
            return localTimer;
        }
    }

    private void Awake()
    {
        // Force the timer to 7 minutes (420 seconds) overriding any old Unity Inspector values
        escapeTimeSeconds = 420f;

        // Always grab references early so nothing is null
        llmClient = GetComponent<GroqLLMClient>();
        ttsClient = GetComponent<FreeTTSClient>();
        whisperClient = GetComponent<GroqWhisperClient>();
        localTimer = escapeTimeSeconds;
    }

    public override void Spawned()
    {
        isInitialized = true;

        if (Object.HasStateAuthority && !TimerStarted)
        {
            CurrentTime = escapeTimeSeconds;
            TimerStarted = true;
        }

        // Start the random torment loop!
        if (tormentCoroutine == null)
            tormentCoroutine = StartCoroutine(RandomTormentLoop());
    }

    private void Start()
    {
        // If Fusion hasn't spawned this object yet, start torment loop anyway
        if (!isInitialized)
        {
            localTimer = escapeTimeSeconds;
            if (tormentCoroutine == null)
                tormentCoroutine = StartCoroutine(RandomTormentLoop());
        }
    }

    // Regular Update as fallback when Fusion is not active
    private void Update()
    {
        if (!isInitialized)
        {
            // Use local timer as fallback
            if (localTimer > 0)
            {
                localTimer -= Time.deltaTime;
                if (localTimer < 0) localTimer = 0;
            }

            UpdateTimerUI(localTimer);
            HandleCountdown(localTimer);
            HandleGameOver(localTimer);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!isInitialized || Object == null || !Object.IsValid) return;

        // ---------- TIMER SYNC ----------
        if (Object.HasStateAuthority && CurrentTime > 0 && Runner.SessionInfo != null && Runner.SessionInfo.PlayerCount >= 2)
        {
            CurrentTime -= Runner.DeltaTime;
            if (CurrentTime < 0) CurrentTime = 0;
        }

        // Keep localTimer in sync for safe access
        localTimer = CurrentTime;

        UpdateTimerUI(CurrentTime);
        HandleCountdown(CurrentTime);
        HandleGameOver(CurrentTime);
    }

    private void UpdateTimerUI(float time)
    {
        if (clockTextDisplay != null)
        {
            int m = Mathf.FloorToInt(time / 60f);
            int s = Mathf.FloorToInt(time % 60f);
            clockTextDisplay.text = $"{m:00}:{s:00}";
        }
    }

    private void HandleCountdown(float time)
    {
        if (time <= 10f && time > 0 && !countdownStarted)
        {
            countdownStarted = true;
            Debug.Log("[AIClock] Final countdown started!");
        }

        if (countdownStarted && time > 0)
        {
            int currentSecond = Mathf.CeilToInt(time);
            if (currentSecond != lastAnnouncedSecond && currentSecond >= 1 && currentSecond <= 10)
            {
                lastAnnouncedSecond = currentSecond;
                if (ttsClient != null) ttsClient.SpeakText(currentSecond.ToString());
                ShowSubtitle(currentSecond.ToString());
            }
        }
    }

    private void HandleGameOver(float time)
    {
        if (time <= 0 && !isBusy)
        {
            isBusy = true;
            if (ttsClient != null) ttsClient.SpeakText("Time is up. You failed. How disappointing... but not surprising.");
            ShowSubtitle("Time is up. You failed. How disappointing... but not surprising.");
        }
    }

    // ============================================================
    //  PUSH-TO-TALK (Called by the Raycast Pointers)
    // ============================================================

    /// <summary>
    /// Call this from RaycastPointer when the player PRESSES on the Clock.
    /// Starts recording the player's microphone.
    /// </summary>
    public void OnPlayerStartTalking()
    {
        if (isBusy) return;
        isBusy = true;

        ttsClient.SpeakText("Yes?");
        ShowSubtitle("Yes?");
        whisperClient.StartRecording();
        Debug.Log("[AIClock] Player started talking to the clock.");
    }

    /// <summary>
    /// Call this from RaycastPointer when the player RELEASES on the Clock. 
    /// Stops recording and sends the audio off for transcription.
    /// </summary>
    public void OnPlayerStopTalking()
    {
        if (!whisperClient.IsRecording) return;

        Debug.Log("[AIClock] Player stopped talking. Transcribing...");
        whisperClient.StopRecordingAndTranscribe(OnTranscriptionDone);
    }

    private void OnTranscriptionDone(string transcribedText)
    {
        if (string.IsNullOrEmpty(transcribedText))
        {
            Debug.LogWarning("[AIClock] Transcription returned empty.");
            isBusy = false;
            return;
        }

        Debug.Log("[AIClock] Player said: " + transcribedText);

        // Prepend time context so the AI knows the exact remaining time
        int m = Mathf.FloorToInt(SafeCurrentTime / 60f);
        int s = Mathf.FloorToInt(SafeCurrentTime % 60f);
        string timeInfo = $"[CONTEXT: There are {m} minutes and {s} seconds remaining on the clock.] ";
        string fullPrompt = timeInfo + "The player asked: " + transcribedText;

        // Send to all clients via Photon (or local fallback)
        AskClockSafe(fullPrompt);
    }

    // ============================================================
    //  RANDOM TORMENT SYSTEM (The bully clock!)
    // ============================================================

    private IEnumerator RandomTormentLoop()
    {
        // Wait a bit after the game starts before the first taunt
        yield return new WaitForSeconds(30f);

        while (SafeCurrentTime > 0)
        {
            float waitTime = Random.Range(minTormentInterval, maxTormentInterval);
            yield return new WaitForSeconds(waitTime);

            // Don't interrupt if the clock is already talking to a player
            if (isBusy) continue;

            // Build a context-aware taunt prompt based on remaining time
            string timeContext = GetTimeContext();
            
            string tormentPrompt;
            if (Runner != null && Runner.SessionInfo != null && Runner.SessionInfo.PlayerCount < 2)
            {
                tormentPrompt = $"The player's partner has disconnected or vanished from the timeline! Roast the player for being left all alone. Keep it short (1 sentence) and sarcastic.";
            }
            else
            {
                tormentPrompt = $"The players have NOT asked you anything. You are bored and want to torment them. {timeContext} Say something short (1 sentence) to mock, rush, or scare them. Be creative and mean.";
            }

            Debug.Log("[AIClock] Tormenting the players...");
            AskClockSafe(tormentPrompt);
        }
    }

    private string GetTimeContext()
    {
        float timeLeft = SafeCurrentTime;
        int m = Mathf.FloorToInt(timeLeft / 60f);
        int s = Mathf.FloorToInt(timeLeft % 60f);
        string exactTime = $"There are exactly {m} minutes and {s} seconds left.";

        if (timeLeft > 210f)
            return $"{exactTime} They just started and are probably clueless. Mock their confidence.";
        else if (timeLeft > 120f)
            return $"{exactTime} They are past the halfway mark and seem lost. Pressure them about wasting time.";
        else if (timeLeft > 60f)
            return $"{exactTime} Time is running low! Be more aggressive and urgent. Make them panic.";
        else
            return $"{exactTime} They have less than 1 minute left! Be dramatic. Tell them they are absolutely doomed.";
    }

    // ============================================================
    //  NETWORK RPCs (Sync across both players)
    // ============================================================

    /// <summary>
    /// Smart wrapper: uses RPCs when networked, falls back to local when solo testing.
    /// </summary>
    private void AskClockSafe(string question)
    {
        if (isInitialized && Object != null && Object.IsValid)
        {
            RPC_AskClock(question);
        }
        else
        {
            // Fallback: just call LLM directly without network
            llmClient.AskQuestion(question, OnLLMResponseLocal);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AskClock(string question)
    {
        llmClient.AskQuestion(question, OnLLMResponse);
    }

    private void OnLLMResponse(string responseText)
    {
        // Broadcast the response to ALL clients
        RPC_PlayTTS(responseText);
    }

    private void OnLLMResponseLocal(string responseText)
    {
        // Local-only playback (no network)
        Debug.Log("[AIClock] Clock says: " + responseText);
        if (ttsClient != null) ttsClient.SpeakText(responseText);
        ShowSubtitle(responseText);
        isBusy = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayTTS(string responseText)
    {
        Debug.Log("[AIClock] Clock says: " + responseText);
        ttsClient.SpeakText(responseText);
        ShowSubtitle(responseText);
        isBusy = false;
    }

    // ============================================================
    //  HELPERS
    // ============================================================

    private void ShowSubtitle(string text)
    {
        if (subtitleDisplay != null)
        {
            subtitleDisplay.text = text;
            // Auto-clear subtitle after a few seconds
            StartCoroutine(ClearSubtitleAfterDelay(5f));
        }
    }

    private IEnumerator ClearSubtitleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (subtitleDisplay != null) subtitleDisplay.text = "";
    }

    private void OnDisable()
    {
        if (tormentCoroutine != null) StopCoroutine(tormentCoroutine);
    }
}
