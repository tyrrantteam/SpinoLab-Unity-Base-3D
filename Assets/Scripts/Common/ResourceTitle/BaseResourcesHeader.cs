using Base.Core.Sound;
using System.Collections;
using System.Collections.Generic;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Common.ResourcesHeader
{
    public class BaseResourcesHeader : MonoBehaviour
    {
        public                    Text   valueTxt;
        [SerializeField]  private Button claimMore;
        [HideInInspector] public  int    valueInformation;

        private                  MotionHandle _countUpHandle;
        [SerializeField] private float        countUpDuration = 0.5f;

        protected virtual void Awake()
        {
            UpdateValue();
            claimMore.onClick.AddListener(OnClickClaimMore);
        }

        protected virtual void UpdateValue()
        {
            valueTxt.text = valueInformation.ToString();
        }
        
        
        [Button]
        protected void ChangeValueWithAnimation()
        {
            var oldValue = int.TryParse(valueTxt.text, out var currentDisplay) ? currentDisplay : valueInformation;
            // valueInformation = value;

            _countUpHandle.TryCancel();
            _countUpHandle = LMotion.Create((float)oldValue, (float)valueInformation, countUpDuration)
                                    .WithEase(Ease.OutCubic)
                                    .Bind(x => { valueTxt.text = Mathf.RoundToInt(x).ToString(); });
        }

        protected virtual void OnClickClaimMore()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ClickButton);
        }

        private void OnDestroy()
        {
            _countUpHandle.TryCancel();
        }
        
        #if UNITY_EDITOR

        [Button]
        public void TestAnimationGold(int value)
        {
            valueInformation = value;
            ChangeValueWithAnimation();
        }
        
        #endif
    }
}