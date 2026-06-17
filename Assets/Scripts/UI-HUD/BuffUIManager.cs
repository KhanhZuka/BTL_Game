using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class BuffUIManager : MonoBehaviour
{
    public static BuffUIManager Instance;

    public Image[] buffIcons;
    public TMP_Text[] buffTimers;

    List<BuffData> activeBuffs = new List<BuffData>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].timeLeft -= Time.deltaTime;
            if (activeBuffs[i].timeLeft <= 0)
            {
                RemoveBuff(i);
            }
        }

        UpdateUI();
    }

    public void AddBuff(ItemData item, float duration)
    {
        BuffData newBuff = new BuffData(item, duration);
        activeBuffs.Add(newBuff);

        NotificationUI.Instance.ShowMessage("Đã kích hoạt " + item.itemName);
        UpdateUI();
    }
    void RemoveBuff(int index)
    {
        activeBuffs.RemoveAt(index);
        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < buffIcons.Length; i++)
        {
            if (i < activeBuffs.Count)
            {
                buffIcons[i].sprite = activeBuffs[i].icon;
                buffIcons[i].enabled = true;

                buffTimers[i].text = activeBuffs[i].timeLeft.ToString("F1");
                buffTimers[i].enabled = true;
                // Debug.Log("UpdateUI chạy: " + activeBuffs.Count);
            }
            else
            {
                buffIcons[i].enabled = false;
                buffTimers[i].enabled = false;
            }
        }
    }
}