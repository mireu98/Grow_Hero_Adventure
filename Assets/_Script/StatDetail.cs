using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatDetail : MonoBehaviour
{
    public RawImage playerPreviewImage; // UI의 Raw Image 연결
    public Transform spawnPoint;        // PreviewCamera 앞의 소환 위치 (PreviewScene 내부)
    private GameObject currentPreview;
    public TMP_Text Damage;
    public TMP_Text AttackSpeed;
    public TMP_Text GoldBonus;
    public TMP_Text CriticalChance;
    public TMP_Text CriticalDamage;
    private GameObject lastPrefab;

    public void ToggleWindow()
    {
        // 1. 현재 상태의 반대로 설정 (켜져 있으면 끄고, 꺼져 있으면 킴)
        bool isActive = !gameObject.activeSelf;
        gameObject.SetActive(isActive);

        // 2. 창이 켜지는 순간에만 정보와 모델 갱신
        if (isActive)
        {
            UpdateStatDetail();
        }
    }

    public void UpdateStatDetail()
    {
        if (!gameObject.activeInHierarchy) return;

        var player = PlayerController.Instance;
        GameObject currentPrefab = player.data.modelPrefab;

        // 1. 모델 생성 및 갱신 로직
        // 모델이 없거나, 현재 플레이어의 프리팹이 이전에 생성한 프리팹과 다를 때 (레벨업 등)
        if (currentPreview == null || lastPrefab != currentPrefab)
        {
            // 기존 모델이 있다면 제거
            if (currentPreview != null) Destroy(currentPreview);

            if (currentPrefab != null)
            {
                lastPrefab = currentPrefab; // 현재 프리팹 기억
                currentPreview = Instantiate(currentPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

                // 방향과 크기 설정 (기존 설정 유지)
                currentPreview.transform.localScale = new Vector3(-10, 10, 1);
                SetLayerRecursively(currentPreview, LayerMask.NameToLayer("UIPlayer"));

                if (currentPreview.TryGetComponent(out Rigidbody2D rb)) rb.simulated = false;
                if (currentPreview.TryGetComponent(out PlayerController pc)) pc.enabled = false;
            }
        }

        // 2. 스텟 텍스트 정보 실시간 갱신 (기존 코드와 동일)
        Damage.text = "Damage : " + player.data.damage;

        float calculatedCooldown = player.data.attackRate / (1f + player.attackSpeedBonus);
        float finalCooldown = Mathf.Max(calculatedCooldown, 1f);
        AttackSpeed.text = "DPS : " + finalCooldown.ToString("F2");

        GoldBonus.text = "G.Bonus : " + player.goldBonus;
        CriticalChance.text = "C.Chance : " + player.critChance + "%";
        CriticalDamage.text = "C.Damage : " + (int)(player.critDamageMultiplier * 100f) + "%";
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
    }
}