using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using System.Collections.Generic;
using UnityEngine;

public class RankingController : PopupBaseController
{
    [SerializeField] private RankingElement rankingElement;
    [SerializeField] private Transform neoPos;

    [SerializeField] private RankingElement currentRanking;
    private DataRankingController _dataRankingController;
    private List<RankingElement> _rankingElements = new List<RankingElement>();

    protected override void InitData()
    {
        _dataRankingController = LoadResourceController.Instance.DataRankingController();
        base.InitData();
        CreateRankingElement();
    }

    private void CreateRankingElement()
    {
        var listRanking = _dataRankingController.ContentContent.rankinglist;
        for (int i = 0; i < listRanking.Count; i++)
        {
           var id = listRanking[i].id;//int
           var name = listRanking[i].name;//string
           var point = listRanking[i].point;//int
           var nation = listRanking[i].nation;//string
           var avatar = listRanking[i].avatar;//string
           var element = Instantiate(rankingElement,neoPos);
            element.InitData(id , name , point, nation, avatar);
            _rankingElements.Add(element);
        }
    }

    private void InitDataPlayer()
    {

    }
}
