using JinGroup.Controller.Feature;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinAnimationController : MonoBehaviour
{
    [SerializeField] private GameObject popupWin;
    [SerializeField] private FadeEffectUI fadeEffect;
    public void DisableAnimation()
    {
        fadeEffect.Play();
        popupWin.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
