using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class UpgradeData
{
    public string name;
    public int level = 1;
    public long baseCost = 100;    // 시작 가격
    public float costMultiplier = 1.15f; // 레벨당 가격 상승률 (15%씩 상승)
    public float upgradeStep = 10f; // 레벨당 상승할 능력치 양
    public int maxLevel = -1; 

    // 현재 레벨에 맞는 가격 계산식
    public long GetCurrentCost()
    {
        return (long)(baseCost * Mathf.Pow(costMultiplier, level - 1));
    }
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;
    public GameObject GoldPanel;
    [Header("업그레이드 데이터 리스트")]
    public List<UpgradeData> upgradeList = new List<UpgradeData>();

    [Header("연결된 슬롯 UI들")]
    public List<UpgradeSlot> slotUIList = new List<UpgradeSlot>();

    [Header("Gacha Settings")]
    public GameObject[] allyPrefabs; // 3종류의 동료 프리팹 (인스펙터에서 등록)
    public Transform slot1;          // 동료가 생성될 첫 번째 위치
    public Transform slot2;          // 동료가 생성될 두 번째 위치

    private int allyCount = 0;       // 현재 뽑은 동료 수

    [Header("Player Levels")]
    public PlayerData[] levelDatas; // 인스펙터에서 LV1~LV6을 순서대로 드래그
    private int currentLevelIndex = 0; // 현재 0번(LV1) 데이터 사용 중
    private List<int> currentAllyIndices = new List<int>();

    public StatDetail StatDetail;
    public int GetCurrentLevelIndex() => currentLevelIndex;

    void Awake() => Instance = this;

    void Start()
    {
        for (int i = 0; i < upgradeList.Count; i++)
        {
           slotUIList[i].UpdateUI(upgradeList[i].level, upgradeList[i].GetCurrentCost(), upgradeList[i].maxLevel);
        }
    }

    public void BuyUpgrade(int index)
    {
        if (index < 0 || index >= upgradeList.Count) return;

        UpgradeData data = upgradeList[index];
        var player = PlayerController.Instance;

        if (index == 0 && currentLevelIndex >= levelDatas.Length - 1) return; // 진화 만렙
        if (index == 1) // 공속 만렙 체크
        {
            float currentCooldown = player.data.attackRate / (1f + player.attackSpeedBonus);
            if (currentCooldown <= 1f) return;
        }
        if (index == 3 && player.critChance >= 100.0f) return;
        if (index == 5 && allyCount >= 2) return; // 동료 만렙

        long cost = data.GetCurrentCost();

        // --- [2단계] 골드 체크 및 결제 ---
        if (GameManager.Instance.gold >= cost)
        {
            GameManager.Instance.gold -= (int)cost;
            GameManager.Instance.UpdateGoldUI();

            data.level++;
            ApplyUpgradeEffect(index, data.level, data.upgradeStep);

            int maxLvl = -1;

            if (index == 0) maxLvl = 6; // 진화

            if (index == 3) // 치명타 확률
            {
                if (player.critChance >= 100.0f)
                {
                    maxLvl = data.level;
                }
            }

            if (index == 5) maxLvl = 3; // 동료 가차

            slotUIList[index].UpdateUI(data.level, data.GetCurrentCost(), maxLvl);
        }
        else
        {
            StartCoroutine(NotEnoughGold());
        }
    }

    IEnumerator NotEnoughGold()
    {
        GoldPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        GoldPanel.SetActive(false);
    }

    public void LoadLevels(List<int> savedLevels, int playerLevel, int stageIndex, List<int> savedAllyIndices)
    {
        // 1. 진화 단계 복구 및 인덱스 방어
        currentLevelIndex = playerLevel;
        PlayerController.Instance.ChangeData(levelDatas[currentLevelIndex]);

        if (MapManager.Instance != null)
        {
            MapManager.Instance.ChangeStage(stageIndex);
        }

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.SetStage(stageIndex);
        }

        // 3. 각 업그레이드 레벨 복구 및 UI 갱신
        for (int i = 0; i < upgradeList.Count; i++)
        {
            if (i < savedLevels.Count)
            {
                upgradeList[i].level = savedLevels[i];
                slotUIList[i].UpdateUI(
                    upgradeList[i].level,
                    upgradeList[i].GetCurrentCost(),
                    upgradeList[i].maxLevel
                );
            }
        }

        if (slot1.childCount > 0) foreach (Transform child in slot1) Destroy(child.gameObject);
        if (slot2.childCount > 0) foreach (Transform child in slot2) Destroy(child.gameObject);

        currentAllyIndices = new List<int>(savedAllyIndices);
        allyCount = 0;

        foreach (int index in currentAllyIndices)
        {
            if (index < allyPrefabs.Length)
            {
                Transform targetSlot = (allyCount == 0) ? slot1 : slot2;
                Instantiate(allyPrefabs[index], targetSlot.position, targetSlot.rotation, targetSlot);
                allyCount++;
            }
        }

        // 5. 스텟창 갱신
        StatDetail.UpdateStatDetail();
    }

    public List<int> GetCurrentAllyIndices()
    {
        return currentAllyIndices;
    }

    void ApplyUpgradeEffect(int index, int level, float step)
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        switch (index)
        {
            case 0: // 플레이어 진화
                if (currentLevelIndex < levelDatas.Length - 1)
                {
                    currentLevelIndex++;
                    player.ChangeData(levelDatas[currentLevelIndex]);

                    // 데이터 상의 level도 올려줍니다 (upgradeList의 데이터)
                    upgradeList[0].level = currentLevelIndex + 1;
                    StatDetail.UpdateStatDetail();
                }
                break;

            case 1: // Attack Speed (공격 속도)
                player.attackSpeedBonus += step;
                StatDetail.UpdateStatDetail();
                break;

            case 2: // Gold Bonus (골드 보너스)
                player.goldBonus += step;
                StatDetail.UpdateStatDetail();
                break;

            case 3:
                player.critChance = Mathf.Min(100.0f, player.critChance + step);
                StatDetail.UpdateStatDetail();
                break;

            case 4: // Critical Damage (치명타 데미지)
                player.critDamageMultiplier += step;
                StatDetail.UpdateStatDetail();
                break;

            case 5: // Gacha (동료 뽑기)
                    // 1. 랜덤하게 동료 선택
                int randomIndex = Random.Range(0, allyPrefabs.Length);
                currentAllyIndices.Add(randomIndex);
                GameObject selectedAlly = allyPrefabs[randomIndex];
                Transform targetSlot = (allyCount == 0) ? slot1 : slot2;

                Instantiate(selectedAlly, targetSlot.position, targetSlot.rotation, targetSlot);

                allyCount++; 
                break;
        }
    }
}