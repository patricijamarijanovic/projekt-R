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
    public string Server_uri = "http://127.0.0.1:5005";
    public AudioSource audioSource;
    // private string previousText = "";

    // string filePath = Path.Combine("Assets", "Resources", "output.txt");

    void Start()
    {
        // if (File.Exists(filePath)) 
        // {
        //     previousText = getTextGeneratedByPython();
        //     StartCoroutine(LoadAndPlayAudio());
        // }
    }

    void Update()
    {
        // // avatar speaks again only if the text has changed in output.txt
        // if (File.Exists(filePath) && !previousText.Equals(getTextGeneratedByPython()))
        // {
        //     previousText = getTextGeneratedByPython();
        //     StartCoroutine(LoadAndPlayAudio());
        // }
    }

    // public string getTextGeneratedByPython() {
    //     return File.ReadAllText(filePath);
    // }

    public void Say(string input)
    {
        var obj = new 
        {
            text = input
        };

        string text = JsonConvert.SerializeObject(obj);
        Debug.Log("Speech: " + text);
        StartCoroutine(PostRequest(Server_uri, text));
    }

    public IEnumerator PostRequest(string uri, string body)
    {
        using (UnityWebRequest www = UnityWebRequest.Post($"{uri}", $"{body}", "application/json"))
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