using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AvatarSpeech : MonoBehaviour
{
    public LLMInteraction llm;
    public AudioSource audioSource;
    public Animator animator;
    public string idleAnimState = "Idle";
    public string[] talkingAnimStates = { "Talking 1", "Talking 2", "Talking 3" };
    public int TalkingAnimIndex = 0;
    private string defaultFilePath;

    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        audioSource.Stop();
        animator.SetBool("IsTalking", false);

        defaultFilePath = Path.Combine(Application.persistentDataPath, "output.wav");
    }

    void Update()
    {
        if (audioSource.isPlaying)
        {
            if (!animator.GetBool("IsTalking"))
            {
                TalkingAnimIndex = Random.Range(0, talkingAnimStates.Length);
                animator.SetBool("IsTalking", true);
                animator.CrossFade(talkingAnimStates[TalkingAnimIndex], 0.2f);
            }
        }
        else
        {
            if (animator.GetBool("IsTalking"))
            {
                animator.SetBool("IsTalking", false);
                animator.CrossFade(idleAnimState, 0.2f);
            }
        }
    }

    public void Say(string input)
    {
        var obj = new
        {
            text = input
        };

        string text = JsonConvert.SerializeObject(obj);
        Debug.Log("Speech: " + text);
        StartCoroutine(PostRequest(llm.Server_uri, text));
    }

    public IEnumerator PostRequest(string uri, string body)
    {
        using (UnityWebRequest www = UnityWebRequest.Post($"{uri}/tts", $"{body}", "application/json"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AS - Error: " + www.error);
            }
            else
            {
                byte[] audioData = www.downloadHandler.data;
                SaveAudioFile(audioData);
                StartCoroutine(LoadAndPlayAudio());
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
            Debug.LogError("Failed to save audio file: " + e.Message);
        }
    }

    private IEnumerator LoadAndPlayAudio()
    {
        if (!File.Exists(defaultFilePath))
        {
            Debug.LogError("Audio file not found at: " + defaultFilePath);
            yield break;
        }

        // UnityWebRequest reloads audio file as AudioClip
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + defaultFilePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load audio file: " + www.error);
                yield break;
            }

            AudioClip newClip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = newClip;
            audioSource.Play();
        }
    }
}
