using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenAI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Whisper : MonoBehaviour
{
    [SerializeField] private Elevenlabs elevenlabs;
    [SerializeField] private Button recordButton;
    [SerializeField] private Image progressBar;
    [SerializeField] private Text message;

    private AudioSource audioSource;
    private readonly string fileName = "output.wav";
    private readonly int duration = 5;

    private AudioClip clip;
    private bool isRecording;
    private float time;
    private OpenAIApi openai = new OpenAIApi("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", "org-Xpw1zuIw1inG2e7vIB6ZKaEe");
    private List<ChatMessage> messages = new List<ChatMessage>();

    private void OnEnable()
    {
        elevenlabs.AudioReceived.AddListener(PlayAudio);
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        recordButton.onClick.AddListener(StartRecording);
    }

    private void StartRecording()
    {
        isRecording = true;
        recordButton.enabled = false;
        clip = Microphone.Start(null, false, duration, 44100);
    }

    private async void EndRecording()
    {
        message.text = "Transcribing...";

        Microphone.End(null);

        byte[] data = SaveWav.Save(fileName, clip);
        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = data, Name = "audio.wav" },
            Model = "whisper-1",
            Language = "en"
        };
        var res = await openai.CreateAudioTranscription(req);

        progressBar.fillAmount = 0;
        message.text = res.Text;
        recordButton.enabled = true;

        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = message.text
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
            /*// Define the pattern for special characters
            string pattern = "[^a-zA-Z0-9]";
        
            // Use Regex to replace special characters with an empty string
            string result = Regex.Replace(message.Content, pattern, "");*/

            messages.Add(message);

            HandleAudio(message.Content);
        }
    }

    private void HandleAudio(string generatedText)
    {
        elevenlabs.GetAudio(generatedText);
    }

    private void PlayAudio(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void Update()
    {
        if (isRecording)
        {
            time += Time.deltaTime;
            progressBar.fillAmount = time / duration;

            if (time >= duration)
            {
                time = 0;
                isRecording = false;
                EndRecording();
            }
        }
    }
}
