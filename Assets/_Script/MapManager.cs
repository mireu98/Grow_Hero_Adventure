using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Stage Settings")]
    public List<GameObject> stages; // Stage1, 2, 3, 4
    public int currentIdx = 0;

    [Header("Scroll Settings")]
    public float scrollSpeed = 3f;
    public float bgWidth = 10f; // 배경 한 장의 가로 길이

    private Transform[] currentBGTransforms; // 현재 스테이지의 배경들

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateCurrentStage(currentIdx);
    }


    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isFighting) return;

        if (currentBGTransforms != null)
        {
            foreach (Transform bg in currentBGTransforms)
            {
                bg.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

                if (bg.position.x <= -bgWidth)
                {
                    Vector3 pos = bg.position;
                    pos.x += bgWidth * currentBGTransforms.Length;
                    bg.position = pos;
                }
            }
        }
    }

    public void ChangeStage(int index)
    {
        if (index >= 0 && index < stages.Count)
        {
            UpdateCurrentStage(index);
        }
    }

    // 스테이지 변경 함수
    public void NextStage()
    {
        if (currentIdx + 1 < stages.Count)
        {
            UpdateCurrentStage(currentIdx + 1);
        }
    }

    void UpdateCurrentStage(int index)
    {
        // 기존 맵 끄기
        for (int i = 0; i < stages.Count; i++) stages[i].SetActive(false);

        currentIdx = index;
        GameObject activeStage = stages[currentIdx];
        activeStage.SetActive(true);

        // 현재 스테이지의 자식 오브젝트(배경들)를 배열로 가져옴
        currentBGTransforms = new Transform[activeStage.transform.childCount];
        for (int i = 0; i < activeStage.transform.childCount; i++)
        {
            currentBGTransforms[i] = activeStage.transform.GetChild(i);
        }

        // 이동 상태로 복귀
        if (GameManager.Instance != null) GameManager.Instance.SetBattleState(false);
    }
}