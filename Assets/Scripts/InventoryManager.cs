using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static ItemData;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<ItemData> items = new List<ItemData>();
    public int maxSlots = 5;

    void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            InventoryManager.Instance.UseItem(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            InventoryManager.Instance.UseItem(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            InventoryManager.Instance.UseItem(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            InventoryManager.Instance.UseItem(3);

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            InventoryManager.Instance.UseItem(4);
    }
    public void AddItem(ItemData item)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (i >= items.Count)
            {
                items.Add(item); break;
            }
        }
        UIManager.Instance.UpdateInventory(items);
    }

    public void UseItem(int index)
    {
        if (index >= items.Count) return;
        ItemData item = items[index];
        ApplyItemEffect(item);
        items.RemoveAt(index);
        UIManager.Instance.UpdateInventory(items);
    }

    void ApplyItemEffect(ItemData item)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        switch (item.itemType)
        {
            case ItemType.Damage:
                Debug.Log("Buff Damage x2 trong 15s");
                player.StartCoroutine(player.DamageBuff(2f, 15f));
                BuffUIManager.Instance.AddBuff(item, 15f);
                break;

            case ItemType.Speed:
                Debug.Log("Buff Speed lên 10 trong 15s");
                player.StartCoroutine(player.SpeedBuff(10f, 15f));
                BuffUIManager.Instance.AddBuff(item, 15f);
                break;

            case ItemType.Shield:
                Debug.Log("Kích hoạt Shield 5 hit");
                player.ActivateShield(5);
                BuffUIManager.Instance.AddBuff(item, 5);
                break;

            case ItemType.HighJump:
                Debug.Log("Buff Jump = 20 trong 15s");
                player.StartCoroutine(player.HighJumpBuff(20f, 15f));
                BuffUIManager.Instance.AddBuff(item, 15f);
                break;

            case ItemType.Freeze:
                Debug.Log("Freeze toàn bộ enemy trong 5s");
                player.StartCoroutine(player.FreezeEnemies(5f));
                BuffUIManager.Instance.AddBuff(item, 5f);
                break;
        }
    }
}
