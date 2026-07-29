using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DataPoolFactory", menuName = "Data/FactoryPoolData")]


public class FactoryPoolData : ScriptableObject
{
    public List<FactoryPoolDataItem> listFactoryPoolData;
    public List<FactoryPoolDataItemUI> listFactoryPoolDataUI;
    
    public FactoryPoolDataItem GetElementByNameID(string id)
    {
        return listFactoryPoolData.Find(item => item.idObject == id);
    }

    public FactoryPoolDataItemUI GetElementByNameIDUI(string id)
    {
        return listFactoryPoolDataUI.Find(item => item.idObject == id);
    }
}

[Serializable]
public class FactoryPoolDataItem
{
    public GameObject typeObject;
    public bool isPreSpawn;
    public int valuePreSpawn;
    public string idObject;
}

[Serializable]
public class FactoryPoolDataItemUI
{
    public GameObject typeObject;
    public bool isPreSpawn;
    public int valuePreSpawn;
    public string idObject;
}
