using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LLMInteraction : MonoBehaviour
{
    public MorphTargetController MTC;
    public AvatarSpeech AS;
    public string Server_uri = "http://127.0.0.1:5006";
    public string Entry;
    public bool Send = false;

    public string Context { get; set; }


    // Start is called before the first frame update
    void Start()
    {
        Context = "";
    }

    // Update is called once per frame
    void Update()
    {
        if(Send)
        { 
            Send = false;
            var payload = new
            {
                context = Context,
                user_input = Entry
            };
            string body = JsonConvert.SerializeObject(payload);

            Debug.Log(body);
            StartCoroutine(GetRequest(Server_uri));
            StartCoroutine(PostRequest(Server_uri, body));
        }
    }

    IEnumerator GetRequest(string uri) 
    {
        using (UnityWebRequest www = UnityWebRequest.Get(uri))
        {
            yield return www.SendWebRequest();
            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("LLM - Error: " + www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator PostRequest(string uri, string body)
    {
        using (UnityWebRequest www = UnityWebRequest.Post($"{uri}/chat", $"{body}", "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("LLM - Error: " + www.error);
            }
            else
            {
                JObject response = JObject.Parse(www.downloadHandler.text);
                // Context = (response.GetValue("context") as JObject).ToString(Formatting.None);
                Debug.Log(response);
                Context = response["context"].ToString();
                string answer = response["response"].ToString();
                
                MTC.AdjustMorphTargets(response);
                AS.Say(answer);
            }
        }
    }
}