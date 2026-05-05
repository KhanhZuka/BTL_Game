using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    bool activated;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            activated = true;

            // Lưu checkpoint
            player.SetCheckpoint(transform.position);

            // Phát âm thanh
            if (audioSource != null)
            {
                audioSource.Play();
            }

            // Debug
            Debug.Log("Checkpoint activated");
        }
    }
}