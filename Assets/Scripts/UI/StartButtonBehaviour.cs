using System;
using Animations;
using DG.Tweening;
using Mechanics.Amount;
using Mechanics.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class StartButtonBehaviour : MonoBehaviour
    {
        public event Action OnStartButtonClicked;
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text currentText;

        [SerializeField] private ElementSlideAnimation elementSlideAnimation;
        [SerializeField] private MysteryJokerHandler mysteryJokerHandler;

        [FormerlySerializedAs("slotRule")] [SerializeField]
        private PayLineFormation payLineFormation;


        private void Start()
        {
            startButton.onClick.AddListener(() =>
            {
                OnStartButtonClicked?.Invoke();

                if (!mysteryJokerHandler.canMysterySpin && AmountHandler.GetInstance().IsAmountGreaterForSpin())
                {
                    ToggleSpinAnimation();
                    AmountHandler.GetInstance().OnGameInitiateAndNotFreeSpin();
                    TopVisualUIManager.GetInstance().SetNormalTexts("GOOD LUCK !!" );

                }
            });

            elementSlideAnimation.OnSlotChangeState += ElementSlideAnimationOnOnSlotChangeState;
        }

        private void ElementSlideAnimationOnOnSlotChangeState(bool obj)
        {
            currentText.text = obj ? "STOP" : "SPIN";
        }

        private void ToggleSpinAnimation()
        {
            bool isSlotSpinning = elementSlideAnimation.IsSlotSpinning();

            if (isSlotSpinning)
            {
         
            }
            else
            {
                elementSlideAnimation.InitiateMovement();
                DOVirtual.DelayedCall(0.4f, () => { payLineFormation.FindPayLineToGive(false); });
            }
        }
    }
}