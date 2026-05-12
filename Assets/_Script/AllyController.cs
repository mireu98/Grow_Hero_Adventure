using System.Collections.Generic;
using UnityEngine;

public class AllyController : MonoBehaviour
{
    public AllyData data;
    private Animator anim;
    private float nextAttackTime;
    public Transform firePoint;
    private List<GameObject> activeEffects = new List<GameObject>();
    private AudioSource _as;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (anim == null) return;

        bool isFighting = GameManager.Instance.isFighting;
        anim.SetBool("1_Move", !isFighting);

        if (!isFighting)
        {
            ClearEffects();
        }

        if (GameManager.Instance.isFighting && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + data.attackRate;
        }
    }

    void Attack()
    {
        anim.SetTrigger("2_Attack");
        _as.PlayOneShot(data.AttackSound);
        // 1. 동료의 데이터에서 기본 데미지를 가져옴
        float baseDmg = data.damage;

        // 2. 플레이어가 "업그레이드만으로" 올린 추가 데미지 수치를 가져옴
        // (PlayerController에 별도로 저장해두는 것이 편합니다)
        float playerBonusDmg = PlayerController.Instance.data.damage;

        // 3. 플레이어 보너스 데미지에 동료 각자의 비율(예: 0.2f)을 곱함
        float allyBonus = playerBonusDmg * 0.3f;

        // 4. 최종 합산
        float finalDamage = baseDmg + allyBonus;

        bool isCritical = Random.value <= (PlayerController.Instance.critChance / 100f);
        if (isCritical)
        {
            finalDamage *= PlayerController.Instance.critDamageMultiplier;
        }
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
            enemy.GetComponent<Enemy>().TakeDamage(finalDamage, isCritical);
        }

        if (data.isRanged && data.projectilePrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up;

            GameObject effect = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
            activeEffects.Add(effect);
            Destroy(effect, 2f);
        }
    }

    void ClearEffects()
    {
        if (activeEffects.Count == 0) return;

        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] != null)
            {
                Destroy(activeEffects[i]);
            }
        }
        activeEffects.Clear();
    }
}