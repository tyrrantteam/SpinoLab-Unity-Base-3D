using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Common
{
    public class GroupTabButtonController : MonoBehaviour
    {
        [SerializeField] private Button[] groupTabBtn;
        [SerializeField] private ActiveBtn activeBtn;
        [SerializeField] private HomeUIManager homeUIManager;
        [SerializeField] private ShopManager shopManager;
        private void Awake()
        {
            Listener();
        }

        private void OnEnable()
        {
            OnClickTab2();
        }

        private void Listener()
        {
            groupTabBtn[0].onClick.AddListener(OnClickTab1);
            groupTabBtn[1].onClick.AddListener(OnClickTab2);
            groupTabBtn[2].onClick.AddListener(OnClickTab3);
        }

        private void OnClickTab1()
        {
            SetUpBtn(0);
        }

        private void OnClickTab2()
        {
            SetUpBtn(1);
        }

        private void OnClickTab3()
        {
            SetUpBtn(2);
        }

        private void SetUpBtn(int idIndex)
        {
            if (idIndex < 0 && groupTabBtn.Length <= 0)
            {
                return;
            }

            for (int i = 0; i < groupTabBtn.Length; i++)
            {
                if (i != idIndex)
                {
                    groupTabBtn[i].gameObject.SetActive(true);
                }
                else
                {
                    groupTabBtn[i].gameObject.SetActive(false);
                }
            }

            activeBtn.transform.SetSiblingIndex(idIndex);
            activeBtn.gameObject.SetActive(true);
            var sprite = groupTabBtn[idIndex].transform.GetChild(0).GetComponent<Image>().sprite;
            activeBtn.SetupBtn(sprite);
        }
    }
}