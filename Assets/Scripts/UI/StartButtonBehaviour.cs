using System;
using Animations;
using DG.Tweening;
using Mechanics.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartButtonBehaviour : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text currentText;

        [SerializeField] private ElementSlideAnimation elementSlideAnimation;
        [SerializeField] private SlotRule slotRule;


        private void Start()
        {
            startButton.onClick.AddListener(() => { ToggleSpinAnimation(); });

            elementSlideAnimation.OnSlotChangeState += ElementSlideAnimationOnOnSlotChangeState;
        }

        private void ElementSlideAnimationOnOnSlotChangeState(bool obj)
        {
            currentText.text = obj ? "STOP" : "START";
        }

        private void ToggleSpinAnimation()
        {
            bool isSlotSpinning = elementSlideAnimation.IsSlotSpinning();

            if (isSlotSpinning)
            {
                DOVirtual.DelayedCall(0.5f, () => { elementSlideAnimation.StopMovement(); });
            }
            else
            {
                elementSlideAnimation.InitiateMovement();
                DOVirtual.DelayedCall(0.4f, () => { slotRule.FindPayLineToGive(); });
            }
        }
    }
}