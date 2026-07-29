using JinGroup.Base.LoadData;
using UnityEngine;
using UnityEngine.UI;

public class RankingElement : MonoBehaviour
{

    [SerializeField] private Text indexRankingTxt;
    [SerializeField] private Text nameTxt;
    [SerializeField] private Text pointTxt;


    [SerializeField] Image ranking;
    [SerializeField] Image avatar;

    private int _idIndex;

    public void InitData(int id, string name, int point, string nation, int Idavatar)
    {
        _idIndex = id;  
        indexRankingTxt.text = _idIndex.ToString();
        nameTxt.text = name;
        pointTxt.text = point.ToString();

        avatar.sprite = LoadResourceController.Instance.LoadAvatar(Idavatar);
        avatar.SetNativeSize();

        if (_idIndex > 3)
        {
            ranking.gameObject.SetActive(false);
            indexRankingTxt.gameObject.SetActive(true);
        }
        else
        {
            ranking.sprite = LoadResourceController.Instance.LoadRanking(id);
            ranking.SetNativeSize();
            ranking.gameObject.SetActive(true);
            indexRankingTxt.gameObject.SetActive(false);
        }
    }
}
