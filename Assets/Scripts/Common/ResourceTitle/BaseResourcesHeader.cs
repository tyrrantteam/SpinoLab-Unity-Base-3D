using Base.Core.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Common.ResourcesHeader
{
    public class BaseResourcesHeader : MonoBehaviour
    {
        public Text valueTxt;
        [SerializeField] private Button claimMore;
        [HideInInspector] public int valueInformation;

        protected virtual void Awake()
        {
            UpdateValue();
            claimMore.onClick.AddListener(OnClickClaimMore);
        }

        protected virtual void UpdateValue()
        {
            valueTxt.text = valueInformation.ToString();
        }

        protected virtual void OnClickClaimMore()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ClickButton);
        }
    }
}