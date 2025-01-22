using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // Required for UI components

public class LLMInteraction : MonoBehaviour
{
    public AvatarSpeech AS;
    public string Server_uri = "http://127.0.0.1:5005"; // Default server URI
    public string Entry;
    public bool Send = false;

    public InputField ServerUriInputField;
    public string Context { get; set; }
    public Text errorDisplay;
    public Text infoDisplay;
    public Button recordButton;
    private Color originalButtonColor;

    void Start()
    {
        Context = "";

        // Initialize the InputField value if it exists
        if (ServerUriInputField != null)
        {
            ServerUriInputField.text = Server_uri; // Set the default URI in the InputField
            ServerUriInputField.onValueChanged.AddListener(OnServerUriChanged);
        }

        if (recordButton != null)
        {
            var buttonImage = recordButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                originalButtonColor = buttonImage.color;
            }
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
            infoDisplay.text = "INFO: Waiting for response...";

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
                ShowError("Error: " + www.error);
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
                ShowError("Error: " + www.error);
                Debug.LogError("LLM - Error: " + www.error);
            }
            else
            {
                JObject response = JObject.Parse(www.downloadHandler.text);
                Debug.Log(response);
                Context = response["context"].ToString();
                string answer = response["response"].ToString();           
                AS.Say(answer, response);
            }
        }
    }
    
    void ShowError(string message)
    {
        if (errorDisplay != null)
        {
            infoDisplay.text = string.Empty;
            errorDisplay.text = message.Replace("\n", " ");
            recordButton.interactable = true;
            ServerUriInputField.interactable = true;
            var buttonImage = recordButton.GetComponent<Image>();
            var ServerUriInputFieldImage = ServerUriInputField.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = originalButtonColor;
            }
            if (ServerUriInputFieldImage != null)
            {
                ServerUriInputFieldImage.color = originalButtonColor;
            }
            StartCoroutine(ClearErrorAfterDelay(5));
        }
        else
        {
            Debug.LogError("ErrorDisplay UI element is not assigned!");
        }
    }

    IEnumerator ClearErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        errorDisplay.text = string.Empty;
    }

}
