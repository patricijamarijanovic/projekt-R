using System.Collections;
using System.IO;
using System;
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
    public InputField inputField;
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
                TalkingAnimIndex = UnityEngine.Random.Range(0, talkingAnimStates.Length);
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
            inputField.interactable = true;
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
            var inputImage = inputField.GetComponent<Image>();
            if (inputImage != null)
            {
                inputImage.color = originalButtonColor;
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
        using (UnityWebRequest www = UnityWebRequest.Post(uri + "/tts", body, "application/json"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                ShowError("Error: " + www.error);
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                byte[] audioData = www.downloadHandler.data;
                if (audioData == null || audioData.Length == 0)
                {
                    ShowError("Error: Received empty audio data.");
                    Debug.LogError("Received empty audio data.");
                    yield break;
                }

                Debug.Log("Received audio data successfully.");
                StartCoroutine(LoadAndPlayAudio(audioData, json));
            }
        }
    }

    private IEnumerator LoadAndPlayAudio(byte[] audioData, JObject json)
    {
        if (audioData == null || audioData.Length == 0)
        {
            ShowError("Error: Audio data is null or empty.");
            Debug.LogError("Audio data is null or empty.");
            yield break;
        }

        // Create an AudioClip from the byte array
        AudioClip newClip = CreateAudioClipFromWav(audioData);

        if (newClip == null)
        {
            ShowError("Failed to decode audio data.");
            Debug.LogError("Failed to decode audio data.");
            yield break;
        }

        audioSource.clip = newClip;
        MTC.AdjustMorphTargets(json);
        infoDisplay.text = string.Empty;
        audioSource.Play();

        Debug.Log("Audio is playing.");
        yield return null;
    }
    private AudioClip CreateAudioClipFromWav(byte[] wavData)
    {
        using (MemoryStream memStream = new MemoryStream(wavData))
        {
            var reader = new BinaryReader(memStream);
            reader.BaseStream.Seek(22, SeekOrigin.Begin);
            short channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            reader.BaseStream.Seek(34, SeekOrigin.Begin);
            short bitsPerSample = reader.ReadInt16();
            reader.BaseStream.Seek(40, SeekOrigin.Begin);
            int dataSize = reader.ReadInt32();
            int samples = dataSize / (channels * bitsPerSample / 8);
            reader.BaseStream.Seek(44, SeekOrigin.Begin);
            byte[] wavDataBytes = reader.ReadBytes(dataSize);
            float[] floatData = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(wavDataBytes, i * 2);
                floatData[i] = sample / 32768.0f;
            }
            AudioClip audioClip = AudioClip.Create("GeneratedClip", samples, channels, sampleRate, false);
            audioClip.SetData(floatData, 0);
            return audioClip;
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
            inputField.interactable = true;
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
            var inputImage = inputField.GetComponent<Image>();
            if (inputImage != null)
            {
                inputImage.color = originalButtonColor;
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
