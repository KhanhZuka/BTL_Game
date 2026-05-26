using UnityEngine;

public class CameraFollowAdvanced : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public float smoothTime = 0.2f;
    public Vector2 offset;

    [Header("Look Ahead")]
    public float lookAheadDistance = 2.0f;
    public float lookAheadSpeed = 5.0f;

    [Header("Y Clamp")]
    public bool clampY = true;
    public float minY = -2f;
    public float maxY = 5f;

    Vector3 velocity = Vector3.zero;
    float currentLookAhead;

    void LateUpdate()
    {
        if (target == null) return;

        float targetMoveX = target.GetComponent<Rigidbody2D>().linearVelocity.x;

        // Look ahead based on direction
        float targetLookAhead = Mathf.Sign(targetMoveX) * lookAheadDistance;
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

        Vector3 targetPos = new Vector3(target.position.x + currentLookAhead + offset.x, target.position.y + offset.y, transform.position.z);

        if (clampY)
        {
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
