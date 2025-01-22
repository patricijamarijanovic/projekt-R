using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Input : MonoBehaviour
{
    public InputField inputField;
    public Button sendButton;
    public InputField ServerUriInputField;
    public Text errorDisplay;
    public Text infoDisplay;
    private Color originalButtonColor;

    public LLMInteraction llmInteraction;

    private void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }
        if (sendButton != null)
        {
            var buttonImage = sendButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                originalButtonColor = buttonImage.color;
            }
        }
    }

    private void OnSendButtonClicked()
    {
        if (llmInteraction != null && inputField != null)
        {
            string userInput = inputField.text;
            inputField.interactable = false;
            sendButton.interactable = false;
            ServerUriInputField.interactable = false;
            var ServerUriInputFieldImage = ServerUriInputField.GetComponent<Image>();
            if (ServerUriInputFieldImage != null)
            {
                ServerUriInputFieldImage.color = Color.red;
            }
            var buttonImage = sendButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.red;
            }
            var inputImage = inputField.GetComponent<Image>();
            if (inputImage != null)
            {
                inputImage.color = Color.red;
            }

            if (!string.IsNullOrEmpty(userInput))
            {
                llmInteraction.Entry = userInput;
                llmInteraction.Send = true;
                Debug.Log($"Text sent to LLM: {userInput}");
            }
            else
            {
                ShowError("Error: input is empty");
                Debug.LogWarning("Input field is empty. Please enter some text.");
            }
        }
        else
        {
            Debug.LogError("LLMInteraction or InputField is not assigned.");
        }
    }

    void ShowError(string message)
    {
        if (errorDisplay != null)
        {
            infoDisplay.text = string.Empty;
            errorDisplay.text = message.Replace("\n", " ");
            sendButton.interactable = true;
            ServerUriInputField.interactable = true;
            inputField.interactable = true;
            var buttonImage = sendButton.GetComponent<Image>();
            var ServerUriInputFieldImage = ServerUriInputField.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = originalButtonColor;
            }
            if (ServerUriInputFieldImage != null)
            {
                ServerUriInputFieldImage.color = originalButtonColor;
            }
            var inputImage = inputField.GetComponent<Image>();
            if (inputImage != null)
            {
                inputImage.color = originalButtonColor;
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
