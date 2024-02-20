using System.IO;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace OpenAI
{
    public class OpenAIApi
    {
        private const string BASE_PATH = "https://api.openai.com/v1";

        private Configuration configuration;
        private Configuration Configuration => configuration ??= new Configuration();

        public OpenAIApi(string apiKey = null, string organization = null)
        {
            if (apiKey != null)
            {
                configuration = new Configuration(apiKey, organization);
            }
        }
        
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore, 
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CustomNamingStrategy()
            },
            Culture = CultureInfo.InvariantCulture
        };

        private async Task<T> DispatchRequest<T>(string path, string method, byte[] payload = null) where T: IResponse
        {
            T data;
            
            using (var request = UnityWebRequest.Put(path, payload))
            {
                request.method = method;
                request.SetHeaders(Configuration, ContentType.ApplicationJson);
                
                var asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone) await Task.Yield();
                
                data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text, jsonSerializerSettings);
            }

            if (data?.Error != null)
            {
                ApiError error = data.Error;
                Debug.LogError($"Error Message: {error.Message}\nError Type: {error.Type}\n");
            }

            if (data?.Warning != null)
            {
                Debug.LogWarning(data.Warning);
            }
            
            return data;
        }

        private async Task<T> DispatchRequest<T>(string path, List<IMultipartFormSection> form) where T: IResponse
        {
            T data;
            
            using (var request = new UnityWebRequest(path, "POST"))
            {
                request.SetHeaders(Configuration);
                var boundary = UnityWebRequest.GenerateBoundary();
                var formSections = UnityWebRequest.SerializeFormSections(form, boundary);
                var contentType = $"{ContentType.MultipartFormData}; boundary={Encoding.UTF8.GetString(boundary)}";
                request.uploadHandler = new UploadHandlerRaw(formSections) {contentType = contentType};
                request.downloadHandler = new DownloadHandlerBuffer();
                var asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone) await Task.Yield();
                
                data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text, jsonSerializerSettings);
            }
            
            if (data != null && data.Error != null)
            {
                ApiError error = data.Error;
                Debug.LogError($"Error Message: {error.Message}\nError Type: {error.Type}\n");
            }

            return data;
        }

        private byte[] CreatePayload<T>(T request)
        {
            var json = JsonConvert.SerializeObject(request, jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(json);
        }

        public async Task<CreateChatCompletionResponse> CreateChatCompletion(CreateChatCompletionRequest request)
        {
            var path = $"{BASE_PATH}/chat/completions";
            var payload = CreatePayload(request);
            
            return await DispatchRequest<CreateChatCompletionResponse>(path, UnityWebRequest.kHttpVerbPOST, payload);
        }

        public async Task<CreateAudioResponse> CreateAudioTranscription(CreateAudioTranscriptionsRequest request)
        {
            var path = $"{BASE_PATH}/audio/transcriptions";
            
            var form = new List<IMultipartFormSection>();
            if (string.IsNullOrEmpty(request.File))
            {
                form.AddData(request.FileData, "file", $"audio/{Path.GetExtension(request.File)}");
            }
            else
            {
                form.AddFile(request.File, "file", $"audio/{Path.GetExtension(request.File)}");
            }
            form.AddValue(request.Model, "model");
            form.AddValue(request.Prompt, "prompt");
            form.AddValue(request.ResponseFormat, "response_format");
            form.AddValue(request.Temperature, "temperature");
            form.AddValue(request.Language, "language");

            return await DispatchRequest<CreateAudioResponse>(path, form);
        }
    }
}
