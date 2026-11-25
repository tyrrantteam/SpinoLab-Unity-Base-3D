using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseFeatureHomeBtn : MonoBehaviour
{
    public NotifyType notiType;
    [SerializeField] private GameObject notiObject;

    private void Awake()
    {
        InitDataNoti();
    }

    private void OnEnable()
    {
        InitDataNoti();
    }
    
    public virtual void InitDataNoti()
    {
        if (NotifyManager.Instance != null)
        {
            var condition = NotifyManager.Instance.InitNotify(notiType);
            if (notiObject != null)
                notiObject.SetActive(condition);
        }
    }
}
