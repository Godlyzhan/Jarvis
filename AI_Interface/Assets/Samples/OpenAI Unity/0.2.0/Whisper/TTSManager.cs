using UnityEngine;

public class TTSManager : MonoBehaviour
{
    private static TTSManager instance;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject tts;
#endif

    public static TTSManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TTSManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "TTSManager";
                    instance = obj.AddComponent<TTSManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        tts = new AndroidJavaObject("android.speech.tts.TextToSpeech",
                                   new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                                       .GetStatic<AndroidJavaObject>("currentActivity"), null);
#endif
    }

    public void Speak(string text)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (tts != null)
        {
            tts.Call("speak", text, 0, null);
        }
        else
        {
            Debug.LogError("TextToSpeech object is null.");
        }
#else
        Debug.Log("Speaking: " + text);
        // Unity-based TTS solution for other platforms
#endif
    }
}