using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Data/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Tooltip("Level hiển thị từ mốc này trở đi sẽ random level loop.")]
    public int levelRandom;
    public int levelMin;
    [Header("UI")]
    public bool usingResourceInGame;
    [Header("UsingMetaSys")]
    public bool usingMetaSys;
    public int levelUnlockMetaSystem;
    [Header("UsingHeart")]
    public bool usingHeart = true;
    [Header("UsingGem")]
    public bool usingGem = false;
}
