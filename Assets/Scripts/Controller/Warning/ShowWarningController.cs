using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowWarningController : MonoBehaviour
{
    [SerializeField] private GameObject warningHardLevel;
    private void Awake()
    {
        this.RegisterListener(EventID.ShowWarningHardLevel, (sender, param) => ShowWarning());
    }
    private void ShowWarning()
    {
        warningHardLevel.SetActive(true);
    }
}
