using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance;
    public TMP_Text text;
    public float displayTime = 2f;

    Coroutine currentRoutine;
    void Awake()
    {
        Instance = this;
        text.text = "";
        text.alpha = 0;
    }

    public void ShowMessage(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message));
    }

    IEnumerator ShowRoutine(string message)
    {
        text.text = message;
        text.alpha = 1;

        yield return new WaitForSeconds(displayTime);

        // mờ dần 
        float t = 1f;
        while (t > 0)
        {
            t -= Time.deltaTime * 2f;
            text.alpha = t;
            yield return null;
        }
        text.text = "";
    }
}