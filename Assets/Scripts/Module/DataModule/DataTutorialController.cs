using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataTutorial", menuName = "Data/Tutorial")]
public class DataTutorialController : ScriptableObject
{
   public List<TutorialData> listTutorial = new List<TutorialData>();

   public TutorialData GetTutorialDataByLevel(int level)
   {
       foreach (var t in listTutorial)
       {
           if (t.levelTut == level)
           {
               return t;
           }
       }
       return default;
   }
}

[Serializable]
public class TutorialData
{
    public int levelTut;
    public TutorialType typeTutorial;
    public Sprite previewTutorial;
    public string nameTut;
    public string descriptionTut;
    public int stepTut;
}

public enum TutorialType
{
    none = 0,
    handSingleTut = 1,
    handSlideTut = 2,
}