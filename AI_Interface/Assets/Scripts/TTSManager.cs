using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks; // Add this using directive

public class TTSManager : MonoBehaviour
{
    private OpenAIWrapper openAIWrapper;
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private TTSModel model = TTSModel.TTS_1;
    [SerializeField] private TTSVoice voice = TTSVoice.Alloy;
    [SerializeField, Range(0.25f, 4.0f)] private float speed = 1f;
    
    private void OnEnable()
    {
        if (!openAIWrapper) this.openAIWrapper = FindObjectOfType<OpenAIWrapper>();
        if (!audioPlayer) this.audioPlayer = GetComponentInChildren<AudioPlayer>();
    }

    public async void SynthesizeAndPlay(string text, Action callback = null)
    {
        Debug.Log("Trying to synthesize " + text);
        byte[] audioData = await RequestTextToSpeech(text, model, voice, speed);
        if (audioData != null)
        {
            Debug.Log("Playing audio.");
            audioPlayer.ProcessAudioBytes(audioData);
        }
        else
        {
            Debug.LogError("Failed to get audio data from OpenAI.");
        }
        
        // Invoke the callback function if provided
        callback?.Invoke();
    }


    public async void DownloadAudio(string text, string filePath)
    {
        byte[] audioData = await RequestTextToSpeech(text, model, voice, speed);
        if (audioData != null)
        {
            File.WriteAllBytes(filePath, audioData);
            Debug.Log("Audio file downloaded: " + filePath);
        }
        else
        {
            Debug.LogError("Failed to get audio data from OpenAI.");
        }
    }

    private async Task<byte[]> RequestTextToSpeech(string text, TTSModel model, TTSVoice voice, float speed)
    {
        return await openAIWrapper.RequestTextToSpeech(text, model, voice, speed);
    }
}
