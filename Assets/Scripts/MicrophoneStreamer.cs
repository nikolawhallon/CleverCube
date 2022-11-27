using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using FrostweepGames.Plugins.Native;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneStreamer : MonoBehaviour
{
    AudioSource audioSource;
    int lastPosition, currentPosition;

    bool recordingInitiated;

    public DeepgramStreamer deepgram;

    void Start()
    {
        recordingInitiated = false;
        CustomMicrophone.RequestMicrophonePermission();
#if UNITY_EDITOR
        audioSource = GetComponent<AudioSource>();
#endif
    }

    void Update()
    {
        if (!CustomMicrophone.HasConnectedMicrophoneDevices()) {
            Debug.Log("We don't have a connected microphone device yet");
            CustomMicrophone.RefreshMicrophoneDevices();
            CustomMicrophone.RequestMicrophonePermission();
        }

        if (CustomMicrophone.HasConnectedMicrophoneDevices() && !CustomMicrophone.IsRecording(CustomMicrophone.devices[0]) && recordingInitiated == false) {
            Debug.Log("We have a microphone, but we haven't started recording, so will attempt to start recording here");
#if UNITY_EDITOR
            audioSource.clip = CustomMicrophone.Start(CustomMicrophone.devices[0], true, 10, AudioSettings.outputSampleRate);
            audioSource.Play();
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
            CustomMicrophone.Start(CustomMicrophone.devices[0], true, 10, AudioSettings.outputSampleRate);
#endif
            recordingInitiated = true;
        }

        if (CustomMicrophone.IsRecording(CustomMicrophone.devices[0]))
		{
            if ((currentPosition = CustomMicrophone.GetPosition(CustomMicrophone.devices[0])) > 0) {
                if (lastPosition > currentPosition)
                    lastPosition = 0;

                if (currentPosition - lastPosition > 0)
                {
#if UNITY_EDITOR
                    float[] samples = new float[(currentPosition - lastPosition) * audioSource.clip.channels];
                    audioSource.clip.GetData(samples, lastPosition);
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
			        float[] samples = new float[0];
			        CustomMicrophone.GetRawData(ref samples);
#endif
                    short[] samplesAsShorts = new short[currentPosition - lastPosition];
                    for (int i = 0; i < currentPosition - lastPosition; i++)
                    {
#if UNITY_EDITOR
                        samplesAsShorts[i] = f32_to_i16(samples[i]);
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
                        samplesAsShorts[i] = f32_to_i16(samples[i + lastPosition]);
#endif
                    }

                    var samplesAsBytes = new byte[samplesAsShorts.Length * 2];

                    System.Buffer.BlockCopy(samplesAsShorts, 0, samplesAsBytes, 0, samplesAsBytes.Length);

                    deepgram.ProcessAudio(samplesAsBytes);

                    lastPosition = currentPosition;
                }
            }
        }
    }

    short f32_to_i16(float sample)
    {
        sample = sample * 32768;
        if (sample > 32767)
        {
            return 32767;
        }
        if (sample < -32768)
        {
            return -32768;
        }
        return (short) sample;
    }
}