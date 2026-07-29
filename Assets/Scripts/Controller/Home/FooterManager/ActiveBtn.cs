using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveBtn : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void SetupBtn(Sprite iconSprite)
    {
        icon.sprite = iconSprite;
    }
}
