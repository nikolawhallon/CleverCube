using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class CleverbotResponse
{
    public string cs;
    public string output;
}

public class CleverCubeConverser : MonoBehaviour
{
    private bool cleverbotConversationStarted = false;
    private string cleverbotCS;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    public void HandleASR(string message)
    {
        Debug.Log("HandleASR: " + message);

        if (message.Length > 0)
        {
            StartCoroutine(GetCleverbotResponse(message));
        }
    }

    IEnumerator GetCleverbotResponse(string text)
    {
        GameObject cleverCube = GameObject.Find("CleverCube");

        var cleverbot_api_key = Environment.GetEnvironmentVariable("CLEVERBOT_API_KEY");

        if (cleverbot_api_key == null) {
            cleverbot_api_key = "INSERT_YOUR_CLEVERBOT_API_KEY";
        }

        string url = "https://www.cleverbot.com/getreply?key=" + cleverbot_api_key;
        if (cleverbotConversationStarted)
        {
            url += "&cs=" + cleverbotCS;
        }
        url += "&input=" + text;

        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);

            CleverbotResponse cleverbotResponse = JsonUtility.FromJson<CleverbotResponse>(uwr.downloadHandler.text);
            StartCoroutine(PlayTextAsAudioAtPosition(cleverbotResponse.output, cleverCube.transform.position));
            cleverbotCS = cleverbotResponse.cs;
            cleverbotConversationStarted = true;
        }
    }

    IEnumerator PlayTextAsAudioAtPosition(string text, Vector3 position)
    {
        string url = "https://dgversetts.deepgram.com/text-to-speech/polly/pcm?text=" + text;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                var buffer = www.downloadHandler.data;

                short[] samplesAsShorts = new short[buffer.Length / 2];
                System.Buffer.BlockCopy(buffer, 0, samplesAsShorts, 0, buffer.Length);

                float[] samples = new float[samplesAsShorts.Length];
                for (int i = 0; i < samplesAsShorts.Length; i++) {
                    samples[i] = i16_to_f32(samplesAsShorts[i]);
                }
                int channels = 1;
                int sampleRate = 16000;

                AudioClip clip = AudioClip.Create("clip", samples.Length, channels, sampleRate, false);
                clip.SetData(samples, 0);
                AudioSource.PlayClipAtPoint(clip, position);
            }
        }
    }

    float i16_to_f32(short sample)
    {
        float sampleAsFloat = ((float) sample) / (float) 32768;
        if (sampleAsFloat > 1.0) {
            return 1.0f;
        }
        if (sampleAsFloat < -1.0) {
            return -1.0f;
        }

        return sampleAsFloat;
    }
}