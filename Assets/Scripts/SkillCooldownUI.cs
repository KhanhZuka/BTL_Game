using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCooldownUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text cooldownText;
    public Image overlay;

    float timer = 0;

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            cooldownText.text = timer.ToString("F1");

            // làm mờ icon
            icon.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            cooldownText.text = "";
            icon.color = Color.white;
        }
    }

    public void StartCooldown(float duration)
    {
        timer = duration;
    }
}