using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TextInutField : MonoBehaviour
{
    [SerializeField] private TTSManager _ttsManager;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    
    private OpenAIApi openai = new OpenAIApi("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", "org-Xpw1zuIw1inG2e7vIB6ZKaEe");
    private List<ChatMessage> messages = new List<ChatMessage>();

    private void Start()
    {
        _button.onClick.AddListener(TextInputText);
    }

    public void TextInputText()
    {
        GetData(_textMeshPro.text);
    }

    public async void GetData(string text)
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = text
        };

        messages.Add(newMessage);

        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo-0613",
            Messages = messages
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
            messages.Add(message);
            _ttsManager.SynthesizeAndPlay(message.Content);
        }
    }
}
