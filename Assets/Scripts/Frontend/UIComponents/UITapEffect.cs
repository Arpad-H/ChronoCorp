using System;
using UnityEngine;
using System.Collections;
using NaughtyAttributes;
using Unity.VisualScripting;

public class UITapEffect : MonoBehaviour
{[Header("Hinge Settings")]
    public float tapSpeed = 0.12f;
    public float pauseBetweenTaps = 0.1f;
    public float liftAngle = -30f; // Negative lifts the finger up

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip tapSound;

    private RectTransform rectTransform;
    private float startX, startY, startZ;
    private bool isAnimating = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void UpdateRestRotation()
    {
        startX = rectTransform.localEulerAngles.x;
        startY = rectTransform.localEulerAngles.y;
        startZ = rectTransform.localEulerAngles.z;
    }

    public void TriggerMultiTap(int count)
    {
        if (!isAnimating) StartCoroutine(MultiTapRoutine(count));
    }

    IEnumerator MultiTapRoutine(int count)
    {
        isAnimating = true;
        for (int i = 0; i < count; i++)
        {
            float elapsed = 0;
            // Lift
            while (elapsed < tapSpeed)
            {
                elapsed += Time.deltaTime;
                float currentX = Mathf.Lerp(startX, startX + liftAngle, elapsed / tapSpeed);
                rectTransform.localEulerAngles = new Vector3(currentX, startY, startZ);
                yield return null;
            }
            // Slam
            elapsed = 0;
            while (elapsed < tapSpeed)
            {
                elapsed += Time.deltaTime;
                float currentX = Mathf.Lerp(startX + liftAngle, startX, elapsed / tapSpeed);
                rectTransform.localEulerAngles = new Vector3(currentX, startY, startZ);
                yield return null;
            }
            if (audioSource && tapSound) audioSource.PlayOneShot(tapSound);
            yield return new WaitForSeconds(pauseBetweenTaps);
        }
        isAnimating = false;
    }
}