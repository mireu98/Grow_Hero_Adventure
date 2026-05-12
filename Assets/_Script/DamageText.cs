using UnityEngine;
using TMPro; // TMP 필수

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    [Header("Move Settings")]
    public float moveSpeed = 1.5f;   // 텍스트가 올라가는 속도
    public float fadeSpeed = 2.0f;   // 사라지는 속도
    public float lifeTime = 0.8f;    // 텍스트가 유지되는 시간

    private Vector3 moveDirection;   // 각자 다른 사선 방향 저장
    private Color textColor;

    void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.worldCamera = Camera.main;
        }

        // 1. 랜덤한 사선 방향 결정 (오른쪽 위 또는 왼쪽 위)
        // x값에 랜덤을 주어 겹침을 방지.
        float randomX = Random.Range(-0.4f, 0.4f); // 좌우 범위
        moveDirection = new Vector3(randomX, 1f, 0f).normalized;

        textColor = textMesh.color;

        // 2. 수명(LifeTime) 뒤에 자동 파괴
        Destroy(gameObject, lifeTime);
    }
    public void SetData(float damage, bool isCritical)
    {
        textMesh.text = Mathf.RoundToInt(damage).ToString(); // 소수점 반올림

        if (isCritical)
        {
            // 크리티컬 연출: 빨간색, 큰 글씨
            textMesh.color = Color.red;
            textMesh.fontSize = 70;
        }
        else
        {
            // 일반 연출: 흰색, 기본 글씨
            textMesh.color = Color.white;
            textMesh.fontSize = 50;
        }

        textColor = textMesh.color; // 변경된 색상 저장
    }

    void Update()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;
    }
}