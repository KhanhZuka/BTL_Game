using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    public static HealthUIManager Instance;

    public Image healthBar;

    public Image[] hearts; // 3 trái tim

    int maxHearts;
    int currentHearts;

    void Awake()
    {
        Instance = this;

        maxHearts = hearts.Length;
        currentHearts = maxHearts;
    }

    // cập nhật thanh máu
    public void UpdateHealth(int current, int max)
    {
        float percent = (float)current / max;
        healthBar.fillAmount = percent;
    }

    // mất 1 mạng
    public void LoseLife()
    {
        if (currentHearts <= 0) return;

        currentHearts--;

        hearts[currentHearts].enabled = false;
    }

    public bool IsGameOver()
    {
        return currentHearts <= 0;
    }

    public void ResetHearts()
    {
        currentHearts = maxHearts;

        foreach (var h in hearts)
            h.enabled = true;
    }
}