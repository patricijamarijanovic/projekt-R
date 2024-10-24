using LMNT;
using System;
using System.IO;
using UnityEngine;

public class AvatarSpeech : MonoBehaviour
{
    public AudioSource audioSource;
    public string audioFileName = "hello.wav";

    private LMNTSpeech speech;

    private string sentimentAPIUrl = "http://127.0.0.1:5000/analyze-sentiment";  // URL sentiment API-ja


    void Start()
    {
        /*
        string audioPath = Path.Combine(Application.streamingAssetsPath, audioFileName);

        AudioClip clip = LoadClip(audioPath);
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Failed to load the audio clip from path: " + audioPath);
        }
        */
        speech = GetComponent<LMNTSpeech>();
        StartCoroutine(speech.Talk());
    }

    AudioClip LoadClip(string path)
    {
        if (File.Exists(path))
        {
            byte[] audioData = File.ReadAllBytes(path);
            int sampleCount = audioData.Length / 2;
            float[] floatData = ConvertByteToFloat(audioData);

            AudioClip clip = AudioClip.Create(audioFileName, sampleCount, 1, 44100, false);

            clip.SetData(floatData, 0);

            return clip;
        }
        else
        {
            Debug.LogError("Audio file not found at: " + path);
            return null;
        }
    }

    float[] ConvertByteToFloat(byte[] byteArray)
    {
        int length = byteArray.Length / 2;
        float[] floatArray = new float[length];

        for (int i = 0; i < length; i++)
        {
            short value = BitConverter.ToInt16(byteArray, i * 2);
            floatArray[i] = value / 32768f;
        }

        return floatArray;
    }
}
