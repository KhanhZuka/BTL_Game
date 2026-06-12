using UnityEngine;

public class HealthBarFollow : MonoBehaviour
{
    public Transform target;
    public float offsetY = 1f;

    void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.position = target.position + Vector3.up * offsetY;
    }
}