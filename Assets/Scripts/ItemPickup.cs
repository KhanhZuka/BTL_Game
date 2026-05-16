using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        InventoryManager.Instance.AddItem(item);
        NotificationUI.Instance.ShowMessage("Đã nhặt vật phẩm " + item.itemName);
        Destroy(gameObject);
    }
}