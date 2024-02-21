using UnityEngine;

[CreateAssetMenu(fileName = "ElevenLabsConfig", menuName = "ElvenLabs/ElvenLabs Configuration")]
public class ElevenLabsConfig : ScriptableObject
{
    public string apiKey = "9a773cabeb1016b1e4c492be628746fb";
    public string voiceId = "";
    public string ttsUrl = "https://api.elevenlabs.io/v1/text-to-speech/{0}/stream";

}

