using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Data/GameConfig")]
public class GameConfig : ScriptableObject
{
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
