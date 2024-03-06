using System.Collections.Generic;
using OpenAI;
using UnityEngine;
using UnityEngine.UI;

public class Whisper : MonoBehaviour
{
    [SerializeField] private TTSManager ttsManager;
    [SerializeField] private Button recordButton;
    [SerializeField] private Image progressBar;
    [SerializeField] private Text message;

    private readonly string fileName = "output.wav";
    private readonly int duration = 5;

    private AudioClip clip;
    private bool isRecording;
    private float time;
    private OpenAIApi openai = new OpenAIApi("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", "org-Xpw1zuIw1inG2e7vIB6ZKaEe");
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
            ttsManager.SynthesizeAndPlay(message.Content);
        }
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
