using UnityEditor.Sprites;
using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public int healAmount = 1;
    AudioSource audioSource;
    bool picked;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (player.health >= player.maxHealth)
            return;

        if (picked) return; // Phát âm thanh khi nhặt

        picked = true;
        player.ChangeHealth(healAmount);

        if (audioSource != null)
        {
            audioSource.Play();
        }
        NotificationUI.Instance.ShowMessage("Đã nhặt vật phẩm hồi máu");

        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject, audioSource.clip.length);
    }
}