using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerController : MonoBehaviour
{
    public PlayerData data;
    public static PlayerController Instance;
    private Animator anim;
    private float nextAttackTime = 0f;
    public Transform firePoint;
    private List<GameObject> activeEffects = new List<GameObject>();

    public float attackSpeedBonus = 0f;
    public float goldBonus = 0f;
    public float critChance = 20f;
    public float critDamageMultiplier = 2.0f;

    public AudioSource Amixer;
    public AudioClip AttackSound;

    void Awake() => Instance = this;

    void Start()
    {
        InitPlayer();
    }

    // 데이터 기반으로 플레이어 초기화 (외형 생성 등)
    public void InitPlayer()
    {
        // 1. 기존에 혹시 있을지 모를 모델링 자식 삭제
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 2. SO 데이터에 등록된 모델 프리팹 소환
        if (data != null && data.modelPrefab != null)
        {
            GameObject model = Instantiate(data.modelPrefab, transform);
            model.transform.localPosition = Vector3.zero; // 부모 위치에 딱 맞춤

            // 3. 새로 생성된 모델의 애니메이터 연결
            anim = model.GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
            return;
        }

        anim.SetBool("1_Move", !GameManager.Instance.isFighting);

        if (GameManager.Instance.isFighting)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();

                float calculatedCooldown = data.attackRate / (1f + attackSpeedBonus);

                float finalCooldown = Mathf.Max(calculatedCooldown, 1f);

                nextAttackTime = Time.time + finalCooldown;

                anim.speed = data.attackRate / finalCooldown;
            }
        }
    }

    void Attack()
    {
        anim.SetTrigger("2_Attack");
        Amixer.PlayOneShot(AttackSound);
        GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObj != null)
        {
            float finalDamage = data.damage;

            bool isCritical = Random.value <= (critChance / 100f);

            if (isCritical)
            {
                finalDamage *= critDamageMultiplier;
                Debug.Log($"<color=red>Critical Hit!</color> 데미지: {finalDamage}");
            }

            enemyObj.GetComponent<Enemy>().TakeDamage(finalDamage, isCritical);
        }
        if (data.projectilePrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up;

            GameObject effect = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
            activeEffects.Add(effect);
            Destroy(effect, 2f);
        }
    }

    public void ChangeData(PlayerData newData)
    {
        data = newData;   // 새로운 SO 할당
        InitPlayer();     // 모델링 및 애니메이터 재설정
        Debug.Log($"플레이어가 {newData.name} 데이터로 변경되었습니다.");
    }
}