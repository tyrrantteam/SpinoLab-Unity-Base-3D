using JinGroup.Module.Resources;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[CreateAssetMenu(fileName = "DataPiggyBank", menuName = "DataPiggyBank")]

public class DataPiggyBankController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] ListPiggyBank content;
    public ListPiggyBank ContentContent => content;
}

[Serializable]
public class PiggyBank
{
    public int id;
    public string type;
    public int max_value;
    public int max_valueBonus;
    public float price;
}

[Serializable]
public class ListPiggyBank
{
    [SpreadsheetPage("PiggyBank")]
    public List<PiggyBank> piggyBank;

    public PiggyBank PiggyData(TypeResources type)
    {
        for(int i = 0; i < piggyBank.Count; i++)
        {
            var piggyElement = piggyBank[i];
            var typePiggy = piggyElement.type;
            TypeResources typeReward = (TypeResources)Enum.Parse(typeof(TypeResources), typePiggy, true);
            if (type == typeReward)
            {
                return piggyElement;
            }
        }

        return null;
    }
}