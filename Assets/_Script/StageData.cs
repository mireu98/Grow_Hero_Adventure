using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/StageData")]
public class StageData : ScriptableObject
{
    public int stageLevel;
    public EnemyData normalEnemies;
    public EnemyData bossEnemy;
}
