using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class Notification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    
    private float elapsedTime = 0f;

    public event Action<Notification> OnNotificationFinished;

    private void Awake()
    {
        if (notificationText == null)
        {
            Debug.LogWarning("No reference for Notification Text");
        }

        if (canvasGroup == null)
        {
            Debug.LogWarning("No reference for Canvas Group");
        }
    }

    public void FillNotificationText(string text)
    {
        StopAllCoroutines();
        
        
        notificationText.text = text;
        canvasGroup.alpha = 0f;

        StartCoroutine(FadeInNotification());
    }

    private IEnumerator FadeInNotification()
    {
        float duration = fadeDuration;
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / duration));
            yield return null;
        }

        OnNotificationFinished?.Invoke(this);
    }

    public void HideNotification()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0f;
    }
}
