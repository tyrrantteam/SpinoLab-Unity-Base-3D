using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Data/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Tooltip("Level hiển thị từ mốc này trở đi sẽ random level loop.")]
    public int levelRandom;
    public int levelMin;
    [Header("UI")]
    public bool isProduction;
}
