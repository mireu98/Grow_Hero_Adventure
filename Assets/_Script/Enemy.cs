using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private EnemyData data;
    public float moveSpeed = 3f;
    public float stopDistance = 2f;
    private float currentHp;

    private Transform player;
    private bool isDead = false;
    private bool isBoss = false;
 
    private EnemySpawner spawner;

    [Header("UI Reference")]
    public GameObject damageTextPrefab;
    public Transform textSpawnPoint;

    private GameObject hpUIInstance;
    private Image hpBar;
    private TMP_Text hpText;
    private bool isHPVisible = false;

    public void Setup(EnemyData newData, EnemySpawner ownerSpawner, bool bossFlag = false)
    {
        if (newData == null) return;

        data = newData;
        currentHp = data.maxHp;
        isBoss = bossFlag;
        spawner = ownerSpawner;

        // --- 모델 프리팹 생성 로직 ---
        if (data != null && data.modelPrefab != null)
        {
            GameObject model = Instantiate(data.modelPrefab, transform.position, Quaternion.identity, transform);
            model.transform.localPosition = Vector3.zero;
        }


        if (GameManager.Instance != null && GameManager.Instance.enemyHPUI != null)
        {
            hpUIInstance = GameManager.Instance.enemyHPUI;
            Transform hpTransform = hpUIInstance.transform.Find("HP");
            if (hpTransform != null)
            {
                hpBar = hpTransform.GetComponent<Image>();
            }
            hpText = hpUIInstance.GetComponentInChildren<TMP_Text>();

            // 몬스터가 새로 소환될 때마다 상태 초기화
            isHPVisible = false;
            hpUIInstance.SetActive(false);

            if (isBoss) ShowHPUI();
        }

    }

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > stopDistance)
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        else
        {
            // 플레이어 앞에 도착 시 전투 상태 돌입
            if (GameManager.Instance != null && !GameManager.Instance.isFighting)
            {
                GameManager.Instance.SetBattleState(true);
            }
        }
    }

    public void TakeDamage(float damage, bool isCritical)
    {
        if (isDead) return;
        if (!isHPVisible) ShowHPUI();
        currentHp -= damage;

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("3_Damaged");

        if (damageTextPrefab != null)
        {
            GameObject dmgTextObj = Instantiate(damageTextPrefab, textSpawnPoint.position, Quaternion.identity);
            DamageText dmgTextScript = dmgTextObj.GetComponent<DamageText>();
            if (dmgTextScript != null) dmgTextScript.SetData(damage, isCritical);
        }

        UpdateHPUI();

        if (currentHp <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("4_Death");

        if (data != null && PlayerController.Instance != null)
        {
            float bonus = PlayerController.Instance.goldBonus;
            long finalGold = (long)(data.dropGold * (1f + bonus));
            GameManager.Instance.AddGold((int)finalGold);
        }

        if (hpUIInstance != null)
        {
            Transform t = hpUIInstance.transform.Find("Timer");
            if (t != null) t.gameObject.SetActive(false);

            hpUIInstance.SetActive(false);
        }

        StartCoroutine(DieProcess());
    }

    IEnumerator DieProcess()
    {
        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance != null) GameManager.Instance.SetBattleState(false);

        if (isBoss && spawner != null)
        {
            spawner.OnBossDefeated();
        }
        if (hpUIInstance != null) hpUIInstance.SetActive(false);
        isHPVisible = false;
        Destroy(gameObject);
    }

    void ShowHPUI()
    {
        if (hpUIInstance == null) return;
        isHPVisible = true;
        hpUIInstance.SetActive(true);
        UpdateHPUI();
    }

    void UpdateHPUI()
    {
        if (hpBar != null) hpBar.fillAmount = currentHp / data.maxHp;
        if (hpText != null) hpText.text = $"{(int)currentHp} / {(int)data.maxHp}";
    }

}