using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string characterName;  // 캐릭터 이름
    public int level;             // 현재 단계
    public float damage;          // 데미지
    public float attackRate;      // 공격 속도
    public GameObject modelPrefab; // 이 단계에서 사용할 SPUM 모델링
    public GameObject projectilePrefab; // 파티클 프리팹
}