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

        // Note: I tried everything under the sun to be able to get the key from an environment variable on linux, but no dice
        string url = "https://www.cleverbot.com/getreply?key=" + "INSERT_YOUR_CLEVERBOT_API_KEY";
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
        string url = "https://dgversetts.deepgram.com/text-to-speech/polly?text=" + text;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                AudioSource.PlayClipAtPoint(myClip, position);
            }
        }
    }
}