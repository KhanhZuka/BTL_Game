using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Stats")]
    public string enemyName;
    public int maxHealth;
    public int damage;
    public float speed;
    public float chaseDistance;
    public float attackDistance;
    public float attackCooldown;
    public float soundInterval;
}
