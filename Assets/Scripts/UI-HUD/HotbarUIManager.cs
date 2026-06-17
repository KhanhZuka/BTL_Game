using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class HotbarUIManager : MonoBehaviour
{
    public static HotbarUIManager Instance;

    public Image[] slotIcons;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateInventory(List<ItemData> items)
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i < items.Count)
            {
                slotIcons[i].sprite = items[i].icon;
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].enabled = false;
            }
        }
    }
}