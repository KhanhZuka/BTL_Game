using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.hasKey = true;
            // Debug.Log("Đã nhặt key");
            KeyUIManager.Instance.ShowKey(); // Hiển thị keyUI
            NotificationUI.Instance.ShowMessage("Đã nhặt chìa khóa");

            Destroy(gameObject);
        }
    }
}
