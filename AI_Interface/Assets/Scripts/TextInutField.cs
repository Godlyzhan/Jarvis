using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class TextInutField : MonoBehaviour
{
    [SerializeField] private TTSManager _ttsManager;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private string _audioFilePath;

    private OpenAIApi openai = new OpenAIApi("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", "org-Xpw1zuIw1inG2e7vIB6ZKaEe");
    private List<ChatMessage> messages = new List<ChatMessage>();

    private void Start()
    {
        _button.onClick.AddListener(TextInputText);
        _audioFilePath ="output_audio.wav";
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
            _ttsManager.SynthesizeAndPlay(message.Content, () => DownloadAudio(message.Content));
        }
    }

    private void DownloadAudio(string text)
    {
        _ttsManager.DownloadAudio(text, _audioFilePath);
    }
}
