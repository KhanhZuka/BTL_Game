using UnityEngine;

public class GroundSpearEnemy : GroundEnemy
{
    [Header("--- Ranged Setup ---")]
    public float throwRange = 7f; //  tầm ném lao
    public Transform firePoint;
    public GameObject spearPrefab;

    [Header("--- Sounds ---")]
    public AudioClip attackSound;

    protected override void Start()
    {
        maxHp = 120f; 
        base.Start();
        
        // Quái sẽ dừng lại và ném ngay khi Player lọt vào khoảng cách này
        attackRange = throwRange; 
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        if (attackSound != null) PlaySound(attackSound);
        
        if (spearPrefab != null && firePoint != null)
        {
            GameObject spear = Instantiate(spearPrefab, firePoint.position, firePoint.rotation);
            
            float facingDir = GetFacingDirection().x; 
            
            SpearProjectile script = spear.GetComponent<SpearProjectile>();
            if (script != null) script.Setup(facingDir);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Vẽ tầm ném lao
        Vector3 pos = transform.position;

        // 2. Vẽ tầm ném lao (Cyan) - Đường thẳng ngang đi qua tâm quái
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pos + new Vector3(-throwRange, 0, 0), pos + new Vector3(throwRange, 0, 0));
        // Gạch chặn 2 đầu
        Gizmos.DrawLine(pos + new Vector3(-throwRange, 0.2f, 0), pos + new Vector3(-throwRange, -0.2f, 0));
        Gizmos.DrawLine(pos + new Vector3(throwRange, 0.2f, 0), pos + new Vector3(throwRange, -0.2f, 0));

        // 3. Vẽ tầm phát hiện (Magenta) - Đường thẳng nhích lên trên đầu một chút để không đè nhau
        Gizmos.color = Color.magenta;
        Vector3 detectPos = pos + new Vector3(0, 0.5f, 0);
        Gizmos.DrawLine(detectPos + new Vector3(-detectionRange, 0, 0), detectPos + new Vector3(detectionRange, 0, 0));
        // Gạch chặn 2 đầu
        Gizmos.DrawLine(detectPos + new Vector3(-detectionRange, 0.2f, 0), detectPos + new Vector3(-detectionRange, -0.2f, 0));
        Gizmos.DrawLine(detectPos + new Vector3(detectionRange, 0.2f, 0), detectPos + new Vector3(detectionRange, -0.2f, 0));

        // 4. Vẽ lãnh thổ (Yellow) - Nằm ở dưới chân dựa vào startPos
        Gizmos.color = Color.yellow;
        Vector3 sPos = Application.isPlaying ? (Vector3)startPos : pos;
        Vector3 territoryPos = sPos - new Vector3(0, 0.5f, 0); 
        Gizmos.DrawLine(territoryPos + new Vector3(-territoryRadius, 0, 0), territoryPos + new Vector3(territoryRadius, 0, 0));
        // Gạch chặn 2 đầu
        Gizmos.DrawLine(territoryPos + new Vector3(-territoryRadius, 0.2f, 0), territoryPos + new Vector3(-territoryRadius, -0.2f, 0));
        Gizmos.DrawLine(territoryPos + new Vector3(territoryRadius, 0.2f, 0), territoryPos + new Vector3(territoryRadius, -0.2f, 0));
    }
}