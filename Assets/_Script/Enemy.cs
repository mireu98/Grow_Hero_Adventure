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
    
    // 이제 인스펙터에서 할당하지 않고 코드로 받아옵니다.
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
            // 1. 모델 생성 (내 자식으로 설정)
            GameObject model = Instantiate(data.modelPrefab, transform.position, Quaternion.identity, transform);
            
            // 2. 위치 초기화 (부모인 빈 오브젝트 위치에 딱 붙게)
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
        // 태그로 플레이어 찾기
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        // 사망했거나 플레이어를 못 찾았으면 중단
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > stopDistance)
        {
            // 왼쪽으로 이동 (플레이어가 왼쪽에 있을 때)
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
        
        // 자식으로 생성된 모델의 애니메이터를 찾아서 트리거 실행
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

        // 보스가 죽었을 때만 스패너에게 보고
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