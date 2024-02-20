using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OpenAI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Whisper : MonoBehaviour
{
    [SerializeField] private Button recordButton;
    [SerializeField] private Image progressBar;
    [SerializeField] private Text message;

    private readonly string fileName = "output.wav";
    private readonly int duration = 5;

    private AudioClip clip;
    private bool isRecording;
    private float time;
    private OpenAIApi openai = new OpenAIApi("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", "org-Xpw1zuIw1inG2e7vIB6ZKaEe");
    string speechFilePath = Path.Combine(Environment.CurrentDirectory, "speech.mp3");
    private List<ChatMessage> messages = new List<ChatMessage>();

    private void Start()
    {
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

            messages.Add(message);

            // Use OpenAI's TTS to speak the text
            GetTTS(message.Content);
        }
    }

    private async void GetTTS(string text)
    {
        // Create request object
        var request = new CreateTextToSpeechRequest
        {
            Text = text,
            Model = "tts-1-hd",
            Voice = "alloy",
        };
        
        // Call the OpenAI API to generate speech
        CreateAudioResponse response = await openai.CreateTextToSpeech(request);
        
        File.WriteAllBytes(speechFilePath, response.AudioData);

        // Play the generated audio
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = AudioClip.Create("GeneratedSpeech", response.AudioData.Length, 1, 44100, false);
        audioSource.clip.SetData(Convert16BitByteArrayToFloat(response.AudioData), 0);
        audioSource.Play();

        // Destroy the AudioSource after playing
        Destroy(audioSource, response.AudioData.Length / 44100);
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
    // Function to convert 16-bit byte array to float array
    private float[] Convert16BitByteArrayToFloat(byte[] audioData)
    {
        int length = audioData.Length / 2;
        float[] floatData = new float[length];

        for (int i = 0; i < length; i++)
        {
            floatData[i] = (float)((short)((audioData[i * 2 + 1] << 8) | audioData[i * 2])) / 32768.0f;
        }

        return floatData;
    }
}
