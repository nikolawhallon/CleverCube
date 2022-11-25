using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using NativeWebSocket;

[System.Serializable]
public class DeepgramResponse
{
    public int[] channel_index;
    public bool is_final;
    public Channel channel;
}

[System.Serializable]
public class Channel
{
    public Alternative[] alternatives;
}

[System.Serializable]
public class Alternative
{
    public string transcript;
}

public class DeepgramStreamer : MonoBehaviour
{
    WebSocket websocket;

    public CleverCubeConverser cleverCubeConverser;

    async void Start()
    {
        var deepgram_api_key = Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");

        if (deepgram_api_key == null) {
            deepgram_api_key = "INSERT_YOUR_DEEPGRAM_API_KEY";
        }

        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Token " + deepgram_api_key }
        };
        websocket = new WebSocket("wss://api.deepgram.com/v1/listen?encoding=linear16&sample_rate=" + AudioSettings.outputSampleRate.ToString(), headers);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connected to Deepgram!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage: " + message);
            DeepgramResponse deepgramResponse = JsonUtility.FromJson<DeepgramResponse>(message);
            if (deepgramResponse.is_final)
            {
                var transcript = deepgramResponse.channel.alternatives[0].transcript;
                Debug.Log(transcript);

                cleverCubeConverser.HandleASR(transcript);
            }
        };

        await websocket.Connect();
    }
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    public async void ProcessAudio(byte[] audio)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.Send(audio);
        }
    }
}
