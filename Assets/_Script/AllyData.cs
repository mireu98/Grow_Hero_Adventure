using UnityEngine;

[CreateAssetMenu(fileName = "NewAllyData", menuName = "ScriptableObjects/AllyData")]
public class AllyData : ScriptableObject
{
    public string allyName;
    public GameObject modelPrefab; // SPUM 모델 프리팹
    public float damage;
    public float attackRate;
    public AudioClip AttackSound;

    [Header("Projectile Settings")]
    public bool isRanged;          // 원거리 여부 (체크하면 화살/마법 발사)
    public GameObject projectilePrefab; // 발사체 프리팹 (화살, 마법구 등)
}