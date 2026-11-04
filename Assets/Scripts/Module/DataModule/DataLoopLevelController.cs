using DataAccount;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataCurveLevel", menuName = "DaTa/DataCurveLevel")]
public class DataLoopLevelController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] ListDataCurve content;
    public ListDataCurve ContentContent => content;

    public int levelStartRandom => (content?.listCurveData != null && content.listCurveData.Count > 0)
        ? content.listCurveData[0].levelStartRandom
        : 0;

    public int startRandom => (content?.listCurveData != null && content.listCurveData.Count > 0)
        ? content.listCurveData[0].startRandom
        : 0;

    public int maximumLevelCurve => (content?.listCurveData != null && content.listCurveData.Count > 0)
        ? content.listCurveData[content.listCurveData.Count - 1].level
        : 0;

    public int levelStartLoop => (content?.listCurveData != null && content.listCurveData.Count > 0)
        ? content.listCurveData[0].levelStartLoop
        : 0;

    // Cache lookup để đạt hiệu năng O(1) và 0 GC Alloc khi random
    [NonSerialized] private bool _isInitialized;
    [NonSerialized] private int _totalRandomLevelsCount;
    [NonSerialized] private readonly Dictionary<int, int> _levelToDifficulty = new Dictionary<int, int>();
    [NonSerialized] private readonly Dictionary<int, List<int>> _difficultyToLevels = new Dictionary<int, List<int>>();
    [NonSerialized] private List<int> _internalHistory;

    private void OnEnable()
    {
        _isInitialized = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _isInitialized = false;
    }
#endif

    /// <summary>
    /// Khởi tạo trước bảng tra cứu độ khó và danh sách level theo độ khó (chỉ chạy 1 lần).
    /// </summary>
    public void InitCache(bool forceRefresh = false)
    {
        if (_isInitialized && !forceRefresh) return;

        _levelToDifficulty.Clear();
        _difficultyToLevels.Clear();
        _totalRandomLevelsCount = 0;

        var listCurve = content?.listCurveData;
        if (listCurve == null || listCurve.Count == 0) return;

        int minStartRandom = startRandom;

        for (int i = 0; i < listCurve.Count; i++)
        {
            var item = listCurve[i];
            _levelToDifficulty[item.level] = item.difficulty;

            // Chỉ đưa vào pool random các level > startRandom
            if (item.level > minStartRandom)
            {
                if (!_difficultyToLevels.TryGetValue(item.difficulty, out var levelList))
                {
                    levelList = new List<int>();
                    _difficultyToLevels[item.difficulty] = levelList;
                }
                levelList.Add(item.level);
                _totalRandomLevelsCount++;
            }
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Lấy level ngẫu nhiên theo difficulty curve tương ứng với currentLevel mà KHÔNG bị trùng lặp.
    /// Tự động quản lý lịch sử thông qua DataAccountPlayer.PlayerPointProcessData.listLvLoop.
    /// </summary>
    /// <param name="currentLevel">Level hiện tại của người chơi</param>
    /// <returns>Level ID được chọn</returns>
    public int levelRandom(int currentLevel)
    {
        var listCurve = content?.listCurveData;
        if (listCurve == null || listCurve.Count == 0)
        {
            return currentLevel;
        }

        int maxCurve = maximumLevelCurve;
        int startRand = levelStartRandom;
        int startLoop = levelStartLoop;

        // Nếu chưa đến mốc bắt đầu random, giữ nguyên level hiện tại
        if (currentLevel < startRand)
        {
            return currentLevel;
        }

        if (!_isInitialized)
        {
            InitCache();
        }

        // Tính curve level tương ứng theo chu kỳ loop nếu vượt quá maximumLevelCurve
        int mappedCurveLevel = currentLevel;
        if (mappedCurveLevel > maxCurve)
        {
            int loopLength = maxCurve - startLoop + 1;
            if (loopLength > 0)
            {
                mappedCurveLevel = startLoop + ((mappedCurveLevel - startLoop) % loopLength);
            }
            else
            {
                mappedCurveLevel = startLoop;
            }
        }

        // Tìm độ khó của curve level này (O(1))
        if (!_levelToDifficulty.TryGetValue(mappedCurveLevel, out int currentDifficulty))
        {
            return mappedCurveLevel;
        }

        // Lấy danh sách level có cùng độ khó (O(1))
        if (!_difficultyToLevels.TryGetValue(currentDifficulty, out var validLevels) || validLevels == null || validLevels.Count == 0)
        {
            return mappedCurveLevel;
        }

        // Lấy danh sách lịch sử level đã chơi từ DataAccountPlayer (hoặc fallback _internalHistory)
        var history = DataAccountPlayer.PlayerPointProcessData?.listLvLoop;
        if (history == null)
        {
            history = _internalHistory ?? (_internalHistory = new List<int>());
        }

        // Nếu chỉ có duy nhất 1 level với độ khó này
        if (validLevels.Count == 1)
        {
            int onlyLevel = validLevels[0];
            RecordLevel(history, onlyLevel);
            return onlyLevel;
        }

        // Đếm các level hợp lệ chưa chơi trong validLevels
        int unplayedCount = 0;
        for (int i = 0; i < validLevels.Count; i++)
        {
            if (!history.Contains(validLevels[i]))
            {
                unplayedCount++;
            }
        }

        // Nếu tất cả level của độ khó này đều đã được chơi hết trong chu kỳ hiện tại
        if (unplayedCount == 0)
        {
            // Reset các level thuộc độ khó này trong history
            for (int i = 0; i < validLevels.Count; i++)
            {
                history.Remove(validLevels[i]);
            }

            // Nếu toàn bộ level random trong game đều đã được chơi hết, clear toàn bộ history
            if (_totalRandomLevelsCount > 0 && history.Count >= _totalRandomLevelsCount)
            {
                history.Clear();
            }

            // Cập nhật lại số lượng unplayed (bây giờ tất cả validLevels đều khả dụng)
            unplayedCount = validLevels.Count;
        }

        // Level vừa chơi gần nhất (để tránh lặp lại ngay lập tức nếu reset pool)
        int lastPlayed = DataAccountPlayer.PlayerPointProcessData != null
            ? DataAccountPlayer.PlayerPointProcessData.currentlevel
            : -1;

        // Nếu có nhiều hơn 1 candidate và có candidate trùng lastPlayed khi reset, ưu tiên tránh lastPlayed
        bool avoidLastPlayed = validLevels.Count > 1 && lastPlayed > 0 && !history.Contains(lastPlayed);
        int effectiveCandidateCount = avoidLastPlayed && unplayedCount > 1 ? unplayedCount - 1 : unplayedCount;

        int targetIndex = UnityEngine.Random.Range(0, effectiveCandidateCount);
        int currentIndex = 0;
        int selectedLevel = validLevels[0];

        for (int i = 0; i < validLevels.Count; i++)
        {
            int candidate = validLevels[i];
            if (!history.Contains(candidate))
            {
                // Bỏ qua lastPlayed nếu cần tránh
                if (avoidLastPlayed && unplayedCount > 1 && candidate == lastPlayed)
                {
                    continue;
                }

                if (currentIndex == targetIndex)
                {
                    selectedLevel = candidate;
                    break;
                }
                currentIndex++;
            }
        }

        RecordLevel(history, selectedLevel);
        return selectedLevel;
    }

    private void RecordLevel(List<int> history, int level)
    {
        if (history != null && !history.Contains(level))
        {
            history.Add(level);
            DataAccountPlayer.SavePlayerPointProcessData();
        }
    }
}

[Serializable]
public class ListDataCurve
{
    [SpreadsheetPage("ListCurveLevel")]
    public List<ProcessCurveData> listCurveData = new List<ProcessCurveData>();
}

[Serializable]
public struct ProcessCurveData
{
    public int level;
    public int difficulty;
    public int startRandom;
    public int levelStartRandom;
    public int levelStartLoop;
}
