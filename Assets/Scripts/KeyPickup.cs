using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        PlayerOne playerOne = other.GetComponent<PlayerOne>();
        if (player != null)
        {
            player.hasKey = true;
            KeyUIManager.Instance.ShowKey(); // Hiển thị keyUI
            NotificationUI.Instance.ShowMessage("Đã nhặt chìa khóa");

            Destroy(gameObject);
        }

        if(playerOne != null)
        {
            // Debug.Log("Đã nhặt key");
            playerOne.hasKey = true;
            KeyUIManager.Instance.ShowKey(); // Hiển thị keyUI
            NotificationUI.Instance.ShowMessage("Đã nhặt chìa khóa");

            Destroy(gameObject);
        }
    }
}
