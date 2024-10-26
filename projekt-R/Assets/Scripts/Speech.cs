using LMNT;
using System;
using System.IO;
using UnityEngine;

public class AvatarSpeech : MonoBehaviour
{
    public AudioSource audioSource;
    private LMNTSpeech speech;

    void Start()
    {
        speech = GetComponent<LMNTSpeech>();
        StartCoroutine(speech.Talk());
    }
}
