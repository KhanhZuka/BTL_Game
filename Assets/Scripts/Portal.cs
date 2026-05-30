using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public GameObject openButton;

    private bool playerInRange;

    private void Start()
    {
        if (openButton != null)
            openButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        bool hasKey = false;

        if (PlayerOne.instance != null && PlayerOne.instance.hasKey)
            hasKey = true;

        if (PlayerController.instance != null && PlayerController.instance.hasKey)
            hasKey = true;

        if (hasKey)
        {
            playerInRange = true;

            if (openButton != null)
                openButton.SetActive(true);
        }
        else
        {
            playerInRange = false;

            if (openButton != null)
                openButton.SetActive(false);

            if (NotificationUI.Instance != null)
                NotificationUI.Instance.ShowMessage("Bạn cần nhặt chìa khóa để mở");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (openButton != null)
            openButton.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;

            GameData.lastMap = SceneManager.GetActiveScene().name;
            GameData.backToWinPanel = true;

            if (PlayerOne.instance != null)
                PlayerOne.instance.hasKey = false;

            if (PlayerController.instance != null)
                PlayerController.instance.hasKey = false;

            SceneManager.LoadScene("UIScene");
        }
    }
}