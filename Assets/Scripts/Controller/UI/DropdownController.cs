using JinGroup.Base.LoadData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private DataImageHand dataImageHand;
    [SerializeField] private HandUIFollowController handController;
    private List<ImageHandData> cacheData = new List<ImageHandData>();
    private void Awake()
    {
        dataImageHand = LoadResourceController.Instance.DataImageHand();
    }
    private void Start()
    {
        SetupDropdown();
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    void SetupDropdown()
    {
        cacheData = new List<ImageHandData>(dataImageHand.GetAllData());

        List<string> options = new List<string>();

        foreach (var item in cacheData)
        {
            options.Add($"{item.ID}");
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    void OnDropdownChanged(int index)
    {
        ImageHandData data = cacheData[index];

        Debug.Log("ID chọn: " + data.ID);
        Debug.Log("Sprite chọn: " + data.Image.name);
        handController.SetHandID(data.ID);
    }
}