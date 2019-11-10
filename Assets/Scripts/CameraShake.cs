using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour {

    [Header("Default values")]
    public float ShakeDuration = 0.3f;          // Time the Camera Shake effect will last
    public float ShakeAmplitude = 1.2f;         // Cinemachine Noise Profile Parameter
    public float ShakeFrequency = 2.0f;         // Cinemachine Noise Profile Parameter

    public CinemachineVirtualCamera VirtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    public void Shake(float shakeDuration = 0, float shakeAmplitude = 0, float shakeFerquency = 0)
    {
        float duration = shakeDuration > 0 ? shakeDuration : ShakeDuration;
        float amplitude = shakeAmplitude > 0 ? shakeAmplitude : ShakeAmplitude;
        float ferquency = shakeFerquency > 0 ? shakeFerquency : ShakeFrequency;

        virtualCameraNoise.m_AmplitudeGain = amplitude;
        virtualCameraNoise.m_FrequencyGain = ferquency;

        StartCoroutine(MakeShake(duration));
    }

    void Start()
    {
        if (VirtualCamera != null)
        {
            virtualCameraNoise = VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            virtualCameraNoise.enabled = false;
        }
        else
            Debug.LogError("VirtualCamera is null");
    }

    private IEnumerator MakeShake(float duration)
    {
        virtualCameraNoise.enabled = true;
        yield return new WaitForSeconds(duration);
        virtualCameraNoise.enabled = false;
    }
}
