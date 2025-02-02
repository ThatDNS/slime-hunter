using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public enum NotificationTransitions
{
    FADE,
    FLY_UP,
    SHAKE
}


// Specifically for use with canvas
[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class Notification : MonoBehaviour
{
    [Header("Notification")]
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public List<NotificationTransitions> transitions;
    public bool playOnStart = true;
    public bool noAlphaOnStart = false;
    public float transitionDuration = 1.0f;

    protected Vector2 startPosition;

    // For temporary in out transition
    private bool inOutActive = false;
    private float inOutTimer = 0.0f;
    private float inOutDuration = 1.0f;

    protected virtual void Start()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.pivot;

        if (noAlphaOnStart) canvasGroup.alpha = 0;
        if (playOnStart) Play();
    }

    protected virtual void Update()
    {
        if (inOutActive)
        {
            inOutTimer += Time.deltaTime;
            if (inOutTimer > transitionDuration+inOutDuration) {
                Play(true);
                inOutActive = false;
            }
        }
    }

    public void PlayInOut(float _duration)
    {
        inOutDuration = _duration;
        inOutTimer = 0.0f;
        if (!inOutActive)
        {
            inOutActive = true;
            Play();
        }
    }

    public void Play(bool reverse = false)
    {
        StopAllCoroutines();
        StartCoroutine(PlayNotification(reverse));
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    IEnumerator PlayNotification(bool reverse)
    {
        float timer = reverse ? transitionDuration : 0;

        while (reverse ? timer > 0 : timer < transitionDuration)
        {
            timer += reverse ? -Time.deltaTime : Time.deltaTime;
            float normalTime = timer / transitionDuration;
            foreach (var transition in transitions)
            {
                switch (transition)
                {
                    case NotificationTransitions.FADE:
                        Fade(normalTime);
                        break;
                    case NotificationTransitions.FLY_UP:
                        FlyUp(normalTime);
                        break;
                    case NotificationTransitions.SHAKE:
                        Shake(normalTime);
                        break;
                }
            }
            yield return null;
        }  
    }

    private void Fade(float time)
    {
        float t = Easing.EaseInOutCubic(time);
        canvasGroup.alpha = t;
    }

    private void FlyUp(float time)
    {
        Vector2 offset = startPosition;
        offset.y += 1;
        rectTransform.pivot = Vector3.LerpUnclamped(offset, startPosition, Easing.EaseOut(time));
    }

    private void Shake(float time)
    {
        float speed = 5 * Mathf.PI; // Adjust the speed of the shake
        float magnitude = 5f; // Adjust the magnitude of the shake

        float x = Mathf.Sin(time * speed) * magnitude;
        Vector2 offset = startPosition;
        offset.x += x;
        rectTransform.pivot = Vector3.Lerp(offset, startPosition, Easing.EaseOut(time));
    }
}
