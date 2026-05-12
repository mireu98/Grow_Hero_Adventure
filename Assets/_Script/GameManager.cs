using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SaveData
{
    // GameManager 데이터
    public long gold;
    public int stageIndex;
    // PlayerController 데이터
    public int currentLevelIndex; // 현재 진화 단계
    public float attackSpeedBonus;
    public float goldBonus;
    public float critChance;
    public float critDamageMultiplier;

    // UpgradeManager 데이터 (리스트 형태로 레벨들 저장)
    public List<int> upgradeLevels = new List<int>();
    public List<int> spawnedAllyIndices = new List<int>();

    public string lastSaveTime;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isFighting = false;
    public long gold = 0;
    public TextMeshProUGUI goldText;
    public GameObject enemyHPUI;

    [Header("치트 시스템")]
    public GameObject cheatInputPanel; 
    public TMP_InputField cheatInputField; 
    private int clickCount = 0;

    public Image buttonImage;     
    public Sprite speed1xSprite;   
    public Sprite speed2xSprite;   
    private bool isDoubleSpeed = false;

    public int goldPerTenMinutes = 100;

    void Awake() => Instance = this;

    private void Start()
    {
        LoadGame();
        isFighting = false;
    }

    // 강제 종료해도 저장
    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveGame();
    }

    public void OnGoldIconClick()
    {
        clickCount++;
        if (clickCount >= 5)
        {
            cheatInputPanel.SetActive(true);
            cheatInputField.text = ""; 
            clickCount = 0;   
        }
    }
    public void OnClickSubmitCheat()
    {
        if (cheatInputField.text.Trim() == "ShowMeTheMoney")
        {
            gold += 100000000;
            UpdateGoldUI();
        }
        else
        {
            Debug.Log("잘못된 코드입니다.");
        }

        cheatInputPanel.SetActive(false);
    }

    public void SetBattleState(bool fighting)
    {
        isFighting = fighting;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = FormatGold(gold);
        }
    }

    string FormatGold(long amount)
    {
        if (amount >= 1000000000) // 10억 이상
        {
            return (amount / 1000000000f).ToString("F1") + "B";
        }
        if (amount >= 1000000) // 100만 이상
        {
            return (amount / 1000000f).ToString("F1") + "M";
        }
        if (amount >= 1000) // 1000 이상
        {
            return (amount / 1000f).ToString("F1") + "K";
        }

        return amount.ToString(); // 1000 미만은 그냥 숫자 표시
    }

    public void ToggleSpeed()
    {
        isDoubleSpeed = !isDoubleSpeed;

        if (isDoubleSpeed)
        {
            Time.timeScale = 2.0f; // 게임 속도 2배
            buttonImage.sprite = speed2xSprite;
        }
        else
        {
            Time.timeScale = 1.0f; // 게임 속도 1배 (정상)
            buttonImage.sprite = speed1xSprite;
        }

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 1. 데이터 채우기
        data.gold = this.gold;
        if (MapManager.Instance != null)
        {
            data.stageIndex = MapManager.Instance.currentIdx;
        }
        var player = PlayerController.Instance;
        data.currentLevelIndex = UpgradeManager.Instance.GetCurrentLevelIndex();
        data.attackSpeedBonus = player.attackSpeedBonus;
        data.goldBonus = player.goldBonus;
        data.critChance = player.critChance;
        data.critDamageMultiplier = player.critDamageMultiplier;
        data.spawnedAllyIndices = UpgradeManager.Instance.GetCurrentAllyIndices();
        // 업그레이드 레벨들 저장
        foreach (var upgrade in UpgradeManager.Instance.upgradeList)
        {
            data.upgradeLevels.Add(upgrade.level);
        }

        data.lastSaveTime = DateTime.Now.ToString();

        // 2. JSON으로 변환 후 저장
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveSlot_1", json);
        PlayerPrefs.Save();

    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SaveSlot_1")) return;

        // 1. 데이터 불러와서 클래스로 변환
        string json = PlayerPrefs.GetString("SaveSlot_1");
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        // 2. 각 매니저에 데이터 분배
        this.gold = data.gold;
        UpdateGoldUI();

        var player = PlayerController.Instance;
        player.attackSpeedBonus = data.attackSpeedBonus;
        player.goldBonus = data.goldBonus;
        player.critChance = data.critChance;
        player.critDamageMultiplier = data.critDamageMultiplier;

        // 업그레이드 레벨 및 플레이어 외형 복구
        UpgradeManager.Instance.LoadLevels(data.upgradeLevels, data.currentLevelIndex, data.stageIndex, data.spawnedAllyIndices);

        if (!string.IsNullOrEmpty(data.lastSaveTime))
        {
            DateTime lastTime = DateTime.Parse(data.lastSaveTime);
            TimeSpan timeDiff = DateTime.Now - lastTime;

            int totalMinutes = (int)timeDiff.TotalMinutes;
            if (totalMinutes >= 10)
            {
                int rewardGold = (totalMinutes / 10) * goldPerTenMinutes;

                rewardGold = Mathf.Min(rewardGold, (1440 / 10) * goldPerTenMinutes);

                AddGold(rewardGold);
            }
        }
    }

    public void OnClickExitBtn()
    {
        // 1. 데이터 저장 실행
        SaveGame();

        // 2. 게임 종료 실행
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

