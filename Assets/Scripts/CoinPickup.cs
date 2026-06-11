using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    bool picked = false;
    AudioSource audioSource;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return;

        if (!other.CompareTag("Player")) return; // chỉ player mới nhặt

        picked = true;      
        PlayerOne playerone = other.GetComponent<PlayerOne>();
        if(playerone != null) PlayerOne.instance.soXu++;
        PlayerController playercontroller = other.GetComponent<PlayerController>();
        if (playercontroller != null) PlayerController.instance.soXu++;
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // cập nhật UI
        CoinUIManager.Instance.AddCoin();
        NotificationUI.Instance.ShowMessage("Đã nhặt coin");

        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject, audioSource.clip.length);
    }
}
