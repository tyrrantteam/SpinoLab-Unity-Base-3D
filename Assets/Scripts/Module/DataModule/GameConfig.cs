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
    [Header("UsingContinuePopup")]
    public bool usingContinuePopup = true;
    public int priceContinuePopup = 190;
    [Header("UsingBooster")]
    public bool usingBooster = true;
    public bool showPriceUnderBoosterButton = false;
    [Header("Level Difficulty Sprites")]
    public Sprite normalLevel;
    public Sprite hardLevel;
    public Sprite superHardLevel;
    [Header("Level Difficulty Preview Sprites")]
    public Sprite normalLevelPreview;
    public Sprite hardLevelPreview;
    public Sprite superHardPreview;
}
