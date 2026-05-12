using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    [Header("데이터 설정")]
    public static EnemySpawner Instance;
    public List<StageData> stageDataList;
    public StageData currentStage;        // 현재 활성화된 데이터
    public float spawnInterval = 3f; // 잡몹 소환 간격
    public Button bossEntryButton;   // 보스 입장 버튼
    public GameObject EnemyPrefab;
    private bool isBossMode = false; // 보스전 진행 중인지 확인

    [Header("보스전 설정")]
    public float bossTimeLimit = 30f; // 제한 시간
    private Coroutine bossTimerCoroutine;

    [Header("보스전 타이머 UI")]
    private TMP_Text timerText;
    private GameObject timerObject;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    IEnumerator Start()
    {
        if (stageDataList == null || stageDataList.Count == 0)
        {

            StageData[] loadedData = Resources.LoadAll<StageData>("");
            stageDataList = new List<StageData>(loadedData);
        }

        yield return new WaitForSeconds(0.6f);

        // 2. 현재 스테이지 할당
        if (currentStage == null)
        {
            if (MapManager.Instance != null && stageDataList.Count > 0)
            {
                int idx = Mathf.Clamp(MapManager.Instance.currentIdx, 0, stageDataList.Count - 1);
                currentStage = stageDataList[idx];
            }
        }

        StartCoroutine(SpawnRoutine());

        // 3. 보스 버튼 연결 (기존 코드 유지)
        if (bossEntryButton == null)
        {
            GameObject btnObj = GameObject.Find("BossButton");
            if (btnObj != null)
            {
                bossEntryButton = btnObj.GetComponent<Button>();
            }
        }

        if (bossEntryButton != null)
        {
            bossEntryButton.onClick.AddListener(SpawnBoss);
        }
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            SpawnMonster();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SetStage(int stageIndex)
    {
        // 리스트가 비어있는지 먼저 확인
        if (stageDataList == null || stageDataList.Count == 0)
        {
            return;
        }

        // 인덱스 범위 초과 방지 및 할당
        int safeIndex = Mathf.Clamp(stageIndex, 0, stageDataList.Count - 1);
        currentStage = stageDataList[safeIndex];

    }

    void InitTimerUI()
    {
        if (timerObject == null)
        {
            GameObject hpUI = GameObject.FindWithTag("HPUI");
            if (hpUI != null)
            {
                Transform t = hpUI.transform.Find("Timer");
                if (t != null)
                {
                    timerObject = t.gameObject;
                    timerText = t.GetComponent<TMP_Text>();
                }
            }
        }
    }

    IEnumerator BossTimerRoutine()
    {
        InitTimerUI();

        float timer = bossTimeLimit;
        if (timerObject != null) timerObject.SetActive(true); // 타이머 켜기

        while (timer > -1)
        {
            if (timerText != null) timerText.text = $"{timer:0}s"; // 초 단위 표시

            yield return new WaitForSeconds(1f);
            timer -= 1f;

            if (!isBossMode)
            {
                if (timerObject != null) timerObject.SetActive(false);
                yield break;
            }
        }

        if (isBossMode)
        {
            if (timerObject != null) timerObject.SetActive(false);
            FailBossBattle();
        }
    }

    void SpawnMonster()
    {
        if (currentStage == null)
        {
            Debug.LogWarning("Current Stage is Null! 복구를 시작합니다.");

            // 리스트 자체가 비어있는지 확인
            if (stageDataList == null || stageDataList.Count == 0)
            {
                Debug.LogError("FATAL ERROR: EnemySpawner의 stageDataList가 인스펙터에서 비어있습니다!");
                return;
            }

            // MapManager에서 현재 인덱스를 가져와서 할당
            if (MapManager.Instance != null)
            {
                int idx = Mathf.Clamp(MapManager.Instance.currentIdx, 0, stageDataList.Count - 1);
                currentStage = stageDataList[idx];
                Debug.Log($"[Spawner] {idx}번 스테이지 데이터로 긴급 복구 완료.");
            }
        }

        if (currentStage == null) return;

        if (!isBossMode && GameManager.Instance != null && !GameManager.Instance.isFighting)
        {
            Spawn(currentStage.normalEnemies);
        }
    }

    // 보스 입장 버튼을 눌렀을 때 실행될 함수
    public void SpawnBoss()
    {
        if (isBossMode) return;

        isBossMode = true;
        if (bossEntryButton != null) bossEntryButton.interactable = false;

        ClearNormalEnemies();

        if (currentStage != null && currentStage.bossEnemy != null)
        {
            Spawn(currentStage.bossEnemy, true);

            if (bossTimerCoroutine != null) StopCoroutine(bossTimerCoroutine);
            bossTimerCoroutine = StartCoroutine(BossTimerRoutine());
        }
    }


    void FailBossBattle()
    {
        // 1. 필드에 있는 보스 제거
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        // 2. 상태 초기화
        isBossMode = false;
        if (bossEntryButton != null) bossEntryButton.interactable = true;

        // 3. 전투 상태 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetBattleState(false);
        }

        GameObject hpUI = GameObject.Find("EnemyHPUI");
        if (hpUI != null) hpUI.SetActive(false);
    }

    void ClearNormalEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetBattleState(false);
        }
    }

    // 공통 소환 로직
    void Spawn(EnemyData data, bool isBoss = false)
    {
        // 1. 빈 오브젝트 껍데기(Enemy 프리팹) 소환
        GameObject enemyObj = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);

        // 2. Enemy 스크립트 가져오기
        Enemy enemyScript = enemyObj.GetComponent<Enemy>();

        if (enemyScript != null)
        {
            enemyScript.Setup(data, this, isBoss);
        }
    }

    public void OnBossDefeated()
    {
        if (bossTimerCoroutine != null) StopCoroutine(bossTimerCoroutine);

        isBossMode = false;
        if (bossEntryButton != null) bossEntryButton.interactable = true;

        // 1. 맵 매니저 스테이지 이동
        if (MapManager.Instance != null)
        {
            MapManager.Instance.NextStage();

            // 2. 맵 매니저의 현재 인덱스에 맞춰 내 데이터도 업데이트
            int currentIdx = MapManager.Instance.currentIdx;
            if (currentIdx < stageDataList.Count)
            {
                currentStage = stageDataList[currentIdx];
            }
        }
    }
}