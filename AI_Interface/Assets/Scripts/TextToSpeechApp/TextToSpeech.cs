using System.IO;
using UnityEngine;

public class TextToSpeech : MonoBehaviour
{
    private OpenAIWrapper openAIWrapper;
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private TTSModel model = TTSModel.TTS_1;
    [SerializeField] private TTSVoice voice = TTSVoice.Alloy;
    [SerializeField, Range(0.25f, 4.0f)] private float speed = 1f;

    private void OnEnable()
    {
        if (!openAIWrapper) this.openAIWrapper = FindObjectOfType<OpenAIWrapper>();
    }

    private async void RequestTextToSpeechConversion(string inputText)
    {
        byte[] audioData = await openAIWrapper.RequestTextToSpeech(inputText, model, voice, speed);
        string filePath = Path.Combine(Application.persistentDataPath, "audio.mp3");
        File.WriteAllBytes(filePath, audioData);
    }
}
