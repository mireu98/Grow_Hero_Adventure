using UnityEngine;
using TMPro;

public class UpgradeSlot : MonoBehaviour
{
    public int upgradeIndex; // 이 슬롯이 몇 번째 업그레이드인지 (0, 1, 2...)
    public UpgradeManager UM;
    public TextMeshProUGUI costText;

    public void OnClickUpgrade()
    {
        UM.BuyUpgrade(upgradeIndex);
    }

    public void UpdateUI(int level, long cost, int maxLevel)
    {
        if (costText == null) return;

        // 현재 레벨이 맥스 레벨에 도달했는지 체크
        if (maxLevel != -1 && level >= maxLevel)
        {
            costText.text = "MAX";
        }
        else
        {
            costText.text = FormatGold(cost);
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
}