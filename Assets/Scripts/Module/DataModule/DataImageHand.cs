using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DataImageHand", menuName = "Data/DataImageHand")]
public class DataImageHand : ScriptableObject
{
    [SerializeField] private List<ImageHandData> listImageHand;

    public ImageHandData GetDataByID(int id)
    {
        return listImageHand.Find(x => x.ID == id);
    }
    public List<ImageHandData> GetAllData()
    {
        return listImageHand;
    }
}
[Serializable]
public class ImageHandData
{
    public int ID;
    public Sprite Image;
}