using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using UnityEngine;
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
    private OpenAIApi openai = new OpenAIApi();

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

    private void EndRecording()
    {
        message.text = "Transcripting...";

        Microphone.End(null);

        byte[] data = SaveWav.Save(fileName, clip);

        Task.Run(async () =>
        {
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                progressBar.fillAmount = 0;
                message.text = res.Text;
                recordButton.enabled = true;

                var newMessage = new ChatMessage()
                {
                    Role = "user",
                    Content = message.text
                };

                messages.Add(newMessage);

                Task.Run(async () =>
                {
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

                        // Use Android's native TTS capabilities to speak the text.
                        using (AndroidJavaObject tts =
                               new AndroidJavaObject("android.speech.tts.TextToSpeech",
                                   new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"), null))
                        {
                            if (tts != null)
                            {
                                tts.Call("speak", message.Content, tts.GetStatic<int>("QUEUE_FLUSH"), null);
                            }
                            else
                            {
                                Debug.LogError("TextToSpeech object is null.");
                            }
                        }
                    }
                });
            });
        });
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

