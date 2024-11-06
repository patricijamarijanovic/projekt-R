using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarSpeech : MonoBehaviour
{
    public AudioSource audioSource;
    private string previousText = "";

    void Start()
    {
        if (File.Exists(MorphTargetController.filePath)) 
        {
            previousText = MorphTargetController.getTextGeneratedByPython();
            StartCoroutine(LoadAndPlayAudio());
        }
    }

    void Update()
    {
        // avatar speaks again only if the text has changed in output.txt
        if (File.Exists(MorphTargetController.filePath) && !previousText.Equals(MorphTargetController.getTextGeneratedByPython()))
        {
            previousText = MorphTargetController.getTextGeneratedByPython();
            StartCoroutine(LoadAndPlayAudio());
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