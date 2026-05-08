using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string sceneName;

    bool playerInRange;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
    }

    void Update()
    {
        // Nhấn F khi đang đứng trong portal
        if (playerInRange && Keyboard.current.fKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}