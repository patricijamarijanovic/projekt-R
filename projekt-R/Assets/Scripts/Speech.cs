using System.Collections;
using System.IO;
using System.Linq.Expressions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.Json;
using UnityEngine.UI;

public class AvatarSpeech : MonoBehaviour
{
    public LLMInteraction llm;
    public AudioSource audioSource;
    public Animator animator;
    public string idleAnimState = "Idle";
    public string[] talkingAnimStates = { "Talking 1", "Talking 2", "Talking 3" }; 
    public int TalkingAnimIndex = 0;

    void Start()
    {
        audioSource.Stop();
        animator.SetBool("IsTalking", false);
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
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AS - Error: " + www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                StartCoroutine(LoadAndPlayAudio());
            }
        }
    }

    private IEnumerator LoadAndPlayAudio()
    {
        string filePath = Path.Combine(Application.dataPath, "Resources", "output.wav");

        // checking if file exists before attempting to load
        if (!File.Exists(filePath))
        {
            Debug.LogError("Audiofile not found at: " + filePath);
            yield break;
        }

        // UnityWebRequest reloads audiofile as AudioClip
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load audiofile: " + www.error);
                yield break;
            }

            AudioClip newClip = DownloadHandlerAudioClip.GetContent(www);

            // Assign and play the new audio clip
            audioSource.clip = newClip;
            audioSource.Play();
            Debug.Log("Playing new audio clip from updated output.wav file");
        }
    }
}