using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    public AudioClip pickupSound;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        InventoryManager.Instance.AddItem(item);
        NotificationUI.Instance.ShowMessage("Đã nhặt vật phẩm " + item.itemName);

        // phát sound
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.audioSource != null && pickupSound != null)
        {
            player.audioSource.PlayOneShot(pickupSound);
        }

        Destroy(gameObject);
    }
}