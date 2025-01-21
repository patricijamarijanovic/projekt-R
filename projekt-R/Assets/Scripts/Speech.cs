using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SFB; // Namespace for Standalone File Browser

public class AvatarSpeech : MonoBehaviour
{
    public LLMInteraction llm;
    public AudioSource audioSource;
    public Animator animator;
    public string idleAnimState = "Idle";
    public string[] talkingAnimStates = { "Talking 1", "Talking 2", "Talking 3" };
    public int TalkingAnimIndex = 0;
    private string folderPath; // Store the selected folder path

    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        audioSource.Stop();
        animator.SetBool("IsTalking", false);

        if (PlayerPrefs.HasKey("SelectedFolderPath"))
        {
            folderPath = PlayerPrefs.GetString("SelectedFolderPath");
        }
        else
        {
            folderPath = Application.persistentDataPath;
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
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("No folder path selected.");
            yield break;
        }

        string filePath = Path.Combine(folderPath, "output.wav");
        filePath = filePath.Replace("\\", "/");

        if (!File.Exists(filePath))
        {
            Debug.LogError("Audio file not found at: " + filePath);
            yield break;
        }

        // UnityWebRequest reloads audio file as AudioClip
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load audio file: " + www.error);
                yield break;
            }

            AudioClip newClip = DownloadHandlerAudioClip.GetContent(www);

            // Assign and play the new audio clip
            audioSource.clip = newClip;
            audioSource.Play();
            Debug.Log("Playing new audio clip from selected folder.");
        }
    }

    public void OpenFolderPicker()
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            folderPath = paths[0];
            PlayerPrefs.SetString("SelectedFolderPath", folderPath);
            PlayerPrefs.Save();
            Debug.Log("Selected folder: " + folderPath);
        }
        else
        {
            Debug.LogWarning("No folder selected.");
        }
    }
}
