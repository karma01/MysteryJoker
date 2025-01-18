using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mechanics.Bet
{
    public class BetDataContainer : MonoBehaviour
    {

        public int ContainerIndexIdentifier => containerIndex;
        
        [SerializeField] private int containerIndex;
        [SerializeField] private BetData containerBetData;

        [SerializeField] private Button betButton;
        [SerializeField] private TMP_Text betDisplayText;

        [SerializeField] private Image selectedImage;
        [SerializeField] private Image deselectImage;

        [SerializeField] private BetChangerUI betChangerUI;


        private void Start()
        {
            betButton.onClick.AddListener(() =>
            {
                betChangerUI.ChangeBetThroughContainer(this);
            });
        }

        public void SetContainerBetData(BetData data)
        {
            containerBetData = data;

            betDisplayText.text = data.betAmount.ToString(NumberFormatInfo.CurrentInfo);
        }

        public BetData GetContainerBetData()
        {
            return containerBetData;
        }

        public void SelectThisContainer()
        {
            deselectImage.enabled = false;
            selectedImage.enabled = true;
        }

        public void DeselectThisContainer()
        {
            selectedImage.enabled = false;
            deselectImage.enabled = true;
        }
    }
}