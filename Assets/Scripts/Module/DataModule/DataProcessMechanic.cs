using Base.Core.Debug;
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DataProcess", menuName = "DaTa/DataProcess")]
public class DataProcessMechanic : ScriptableObject
{
    public List<ProcessMechanicData> listTut = new List<ProcessMechanicData>();


    /// <summary>
    /// Returns the next mechanic that is not unlocked yet (smallest entry level greater than current level).
    /// Example: unlocks at 16 (x2) and 35, current level 20 → returns data for level 35.
    /// </summary>
    public ProcessMechanicData GetDataProcesByCurrentLevel(int level)
    {
        if (listTut == null || listTut.Count == 0)
            return default;

        int nextUnlockLevel = int.MaxValue;
        for (int i = 0; i < listTut.Count; i++)
        {
            int entryLevel = listTut[i].level;
            if (entryLevel > level)
                nextUnlockLevel = Mathf.Min(nextUnlockLevel, entryLevel);
        }

        if (nextUnlockLevel == int.MaxValue)
            return default;

        for (int i = 0; i < listTut.Count; i++)
        {
            if (listTut[i].level == nextUnlockLevel)
                return listTut[i];
        }

        return default;
    }

    public bool TryGetUnshownMechanicAtLevel(int level, List<int> shownIndices, out ProcessMechanicData data, out int listIndex)
    {
        data = default;
        listIndex = -1;

        if (listTut == null || listTut.Count == 0)
            return false;

        shownIndices ??= new List<int>();

        for (int i = 0; i < listTut.Count; i++)
        {
            if (listTut[i].level != level || shownIndices.Contains(i))
                continue;

            data = listTut[i];
            listIndex = i;
            return true;
        }

        return false;
    }

    public bool HasRemainingMechanicUnlock(int level)
    {
        if (listTut == null || listTut.Count == 0)
            return false;

        for (int i = 0; i < listTut.Count; i++)
        {
            if (listTut[i].level > level)
                return true;
        }

        return false;
    }

    public int CalculatePercentByList(int currentLevel)
    {
        if (listTut == null || listTut.Count == 0)
        {
            GameDebug.LogWarning("List tutorial rỗng!");
            return 0;
        }

        var milestones = GetSortedDistinctUnlockLevels();
        if (milestones.Count == 0)
            return 0;

        int levelStart = 0;
        int levelEnd = 0;

        for (int i = 0; i < milestones.Count; i++)
        {
            if (currentLevel < milestones[i])
            {
                levelStart = i > 0 ? milestones[i - 1] : 0;
                levelEnd = milestones[i] - 1;
                break;
            }

            levelStart = milestones[i];
            if (i < milestones.Count - 1)
                levelEnd = milestones[i + 1] - 1;
            else
                levelEnd = milestones[i];
        }

        if (levelEnd < levelStart)
        {
            return 100;
        }
        int totalLevels = levelEnd - levelStart + 1;      // tổng số level trong step
        int completed = currentLevel - levelStart + 1;    // đã hoàn thành bao nhiêu
        float percent = (float)completed / totalLevels;
        percent = Mathf.Clamp01(percent);
        int percentDisplay = Mathf.RoundToInt(percent * 100f);
        GameDebug.Log($"[Tutorial Percent] Level: {currentLevel} | Start: {levelStart} | End: {levelEnd} | Completed: {completed}/{totalLevels} | Percent: {percentDisplay}%");
        return percentDisplay;
    }

    private List<int> GetSortedDistinctUnlockLevels()
    {
        var levels = new List<int>();
        for (int i = 0; i < listTut.Count; i++)
        {
            int lv = listTut[i].level;
            if (!levels.Contains(lv))
                levels.Add(lv);
        }

        levels.Sort();
        return levels;
    }

    [Serializable]
    public struct ProcessMechanicData
    {
        public int level;
        public Sprite imgMechanic;
        public string nameMechanic;
        public string description;
    }
}
