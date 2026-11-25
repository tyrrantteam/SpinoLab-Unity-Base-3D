using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CustomVerticalLayoutItem : MonoBehaviour
{
    public                   HomeNode              kind = HomeNode.Small;
    public                   bool                    ignoreLayout;
    [SerializeField] private TMP_Text                textvalue;
    public                   Image                   _imageFloaty;
    private                  DataLoopLevelController _datagame;
    private                  GameConfig              gameConfig;
    public                   int                     displayedLevel { get; private set; }

    private void Awake()
    {
        _datagame = LoadResourceController.Instance != null
            ? LoadResourceController.Instance.LoadDataLoopLevel()
            : null;
        if (gameConfig == null && LoadResourceController.Instance != null)
            gameConfig = LoadResourceController.Instance.GameConfig();
        this.RegisterListener(EventID.updateLevelDifficulty, (sender, param) => InitData());
    }

    public void SetDisplayedLevel(int level)
    {
        displayedLevel = Mathf.Max(1, level);
        if (textvalue != null)
            textvalue.text = displayedLevel.ToString();
    }

    private void InitData()
    {
        if (kind == HomeNode.Small) return;
        // var currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        if (gameConfig == null || _imageFloaty == null) return;

        var difficulty = HomeUIManager.Instance.Difficulty;
        switch (difficulty)
        {
            case 5:
                _imageFloaty.sprite = gameConfig.hardLevelPreview;
                break;
            case 6:
                _imageFloaty.sprite = gameConfig.superHardPreview;
                break;
            default:
                _imageFloaty.sprite = gameConfig.normalLevelPreview;
                break;
        }

        _imageFloaty.SetNativeSize();
    }
}

public enum HomeNode
{
    Small,
    Large
}