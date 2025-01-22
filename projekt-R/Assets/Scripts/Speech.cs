using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AvatarSpeech : MonoBehaviour
{
    public MorphTargetController MTC;
    public LLMInteraction llm;
    public AudioSource audioSource;
    public Animator animator;
    public string idleAnimState = "Idle";
    public string[] talkingAnimStates = { "Talking 1", "Talking 2", "Talking 3" };
    public int TalkingAnimIndex = 0;
    private string defaultFilePath;
    public Text errorDisplay;
    public Text infoDisplay;
    public Button recordButton;
    private Color originalButtonColor;
    public InputField ServerUriInputField;
    private bool wasPlaying = false;

    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        audioSource.Stop();
        animator.SetBool("IsTalking", false);

        defaultFilePath = Path.Combine(Application.persistentDataPath, "output.wav");

        if (recordButton != null)
        {
            var buttonImage = recordButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                originalButtonColor = buttonImage.color;
            }
        }
    }

    void Update()
    {
        if (audioSource.isPlaying)
        {
            if (!animator.GetBool("IsTalking"))
            {
                TalkingAnimIndex = Random.Range(0, talkingAnimStates.Length);
                animator.SetBool("IsTalking", true);
                animator.CrossFade(talkingAnimStates[TalkingAnimIndex], 0.15f);
            }
        }
        else if (wasPlaying)
        {
            HandleAudioStopped();
        }

        wasPlaying = audioSource.isPlaying;
    }

    private void HandleAudioStopped()
    {
        if (animator.GetBool("IsTalking"))
        {
            animator.SetBool("IsTalking", false);
            animator.CrossFade(idleAnimState, 0.15f);
        }
        if (recordButton != null)
        {
            recordButton.interactable = true;
            ServerUriInputField.interactable = true;
            var buttonImage = recordButton.GetComponent<Image>();
            var ServerUriInputFieldImage = ServerUriInputField.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = originalButtonColor;
            }
            if (ServerUriInputFieldImage != null)
            {
                ServerUriInputFieldImage.color = originalButtonColor;
            }
        }
    }

    public void Say(string input, JObject json)
    {
        var obj = new
        {
            text = input
        };

        string text = JsonConvert.SerializeObject(obj);
        Debug.Log("Speech: " + text);
        StartCoroutine(PostRequest(llm.Server_uri, text, json));
    }

    public IEnumerator PostRequest(string uri, string body, JObject json)
    {
        using (UnityWebRequest www = UnityWebRequest.Post($"{uri}/tts", $"{body}", "application/json"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                ShowError("Error: " + www.error);
                Debug.LogError("AS - Error: " + www.error);
            }
            else
            {
                byte[] audioData = www.downloadHandler.data;
                SaveAudioFile(audioData);
                StartCoroutine(LoadAndPlayAudio(json));
            }
        }
    }

    private void SaveAudioFile(byte[] audioData)
    {
        try
        {
            File.WriteAllBytes(defaultFilePath, audioData);
            Debug.Log("Audio file saved at: " + defaultFilePath);
        }
        catch (IOException e)
        {
            ShowError("Error: " + e.Message);
            Debug.LogError("Failed to save audio file: " + e.Message);
        }
    }

    private IEnumerator LoadAndPlayAudio(JObject json)
    {
        if (!File.Exists(defaultFilePath))
        {
            ShowError("Audio file not found at: " + defaultFilePath);
            Debug.LogError("Audio file not found at: " + defaultFilePath);
            yield break;
        }

        // UnityWebRequest reloads audio file as AudioClip
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + defaultFilePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                ShowError("Failed to load audio file: " + www.error);
                Debug.LogError("Failed to load audio file: " + www.error);
                yield break;
            }

            AudioClip newClip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = newClip;
            MTC.AdjustMorphTargets(json);
            infoDisplay.text = string.Empty;
            audioSource.Play();
        }
    }

    void ShowError(string message)
    {
        if (errorDisplay != null)
        {
            infoDisplay.text = string.Empty;
            errorDisplay.text = message.Replace("\n", " ");
            recordButton.interactable = true;
            ServerUriInputField.interactable = true;
            var buttonImage = recordButton.GetComponent<Image>();
            var ServerUriInputFieldImage = ServerUriInputField.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = originalButtonColor;
            }
            if (ServerUriInputFieldImage != null)
            {
                ServerUriInputFieldImage.color = originalButtonColor;
            }
            StartCoroutine(ClearErrorAfterDelay(5));
        }
        else
        {
            Debug.LogError("ErrorDisplay UI element is not assigned!");
        }
    }

    IEnumerator ClearErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        errorDisplay.text = string.Empty;
    }
}
