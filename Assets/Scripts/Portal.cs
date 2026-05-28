using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string sceneName;
    public Transform teleportPoint;
    PlayerController player;
    bool playerInRange;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        player = other.GetComponent<PlayerController>();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
    }
    void Update()
    {
        if (playerInRange && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (player != null && player.hasKey)
            {
                // Debug.Log("Load scene");
                KeyUIManager.Instance.HideKey();
                SceneManager.LoadScene(sceneName);
                NotificationUI.Instance.ShowMessage("Đã mở cổng");
            }
            else
            {
                Debug.Log("Cần chìa khóa để dùng cổng");
                NotificationUI.Instance.ShowMessage("Cần chìa khóa để dùng cổng");
            }
        }
    }

}