using System;
using System.IO;
using UnityEngine;

public class AvatarSpeech : MonoBehaviour
{
    public AudioSource audioSource; // Attach the AudioSource from the avatar
    public string audioFileName = "hello.wav"; // The name of the TTS file

    void Start()
    {
        // Call the method to make the avatar say the sentence
        SaySentence();
    }

    void SaySentence()
    {
        // Create the full path to the audio file
        string audioPath = Path.Combine(Application.streamingAssetsPath, audioFileName);

        // Load the AudioClip from the local path
        AudioClip clip = LoadClip(audioPath);

        if (clip != null)
        {
            // Set the loaded AudioClip and play it
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Failed to load the audio clip from path: " + audioPath);
        }
    }

    AudioClip LoadClip(string path)
    {
        if (File.Exists(path))
        {
            // Load the data into a byte array
            byte[] audioData = File.ReadAllBytes(path);

            // Calculate the total number of samples (assuming 16-bit PCM)
            int sampleCount = audioData.Length / 2; // 16-bit = 2 bytes
            float[] floatData = ConvertByteToFloat(audioData);

            // Create an AudioClip
            AudioClip clip = AudioClip.Create(audioFileName, sampleCount, 1, 44100, false);

            // Load the audio data into the clip
            clip.SetData(floatData, 0);

            return clip;
        }
        else
        {
            Debug.LogError("Audio file not found at: " + path);
            return null;
        }
    }

    // Helper method to convert a byte array (16-bit PCM) into float data for Unity's AudioClip
    float[] ConvertByteToFloat(byte[] byteArray)
    {
        int length = byteArray.Length / 2; // 16-bit = 2 bytes
        float[] floatArray = new float[length];

        for (int i = 0; i < length; i++)
        {
            // Convert each pair of bytes to a float (-1.0 to 1.0)
            short value = BitConverter.ToInt16(byteArray, i * 2);
            floatArray[i] = value / 32768f; // Convert to a float
        }

        return floatArray;
    }
}
