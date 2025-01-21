using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // Required for UI components

public class LLMInteraction : MonoBehaviour
{
    public MorphTargetController MTC;
    public AvatarSpeech AS;
    public string Server_uri = "http://127.0.0.1:5005"; // Default server URI
    public string Entry;
    public bool Send = false;

    public InputField ServerUriInputField; // UI InputField for server URI
    public string Context { get; set; }

    void Start()
    {
        Context = "";

        // Initialize the InputField value if it exists
        if (ServerUriInputField != null)
        {
            ServerUriInputField.text = Server_uri; // Set the default URI in the InputField
            ServerUriInputField.onValueChanged.AddListener(OnServerUriChanged);
        }
    }

    void Update()
    {
        if (Send)
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

    // Triggered whenever the ServerUriInputField's value changes
    private void OnServerUriChanged(string newUri)
    {
        Server_uri = newUri;
        Debug.Log("Server URI updated to: " + Server_uri);
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(uri))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
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
                Debug.Log(response);
                Context = response["context"].ToString();
                string answer = response["response"].ToString();

                MTC.AdjustMorphTargets(response);
                AS.Say(answer);
            }
        }
    }
}
