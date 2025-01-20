using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Mechanics.Slot
{
    public class SlotBox : MonoBehaviour
    {
        [SerializeField] private SlotBoxData slotBoxData;
        [SerializeField] private SlotElement slotElement;

        [SerializeField] private Image elementIdentifierImage;

        [SerializeField] private Animator mysteryBoxAnimator;
        private static readonly int IsPlaying = Animator.StringToHash("isPlaying");

        private void Start()
        {
            SetElementIdentifierImage(slotElement.elementSprite);
        }

        public void SetSlotElement(SlotElement element)
        {
            slotElement = element;
            SetElementIdentifierImage(slotElement.elementSprite);
        }

        public SlotElement GetSlotElement()
        {
            return slotElement;
        }

        public SlotBoxData GetSlotBoxData()
        {
            return slotBoxData;
        }


        private void SetElementIdentifierImage(Sprite image)
        {
            elementIdentifierImage.sprite = image;
        }

        public void RunMysteryBoxAnimation()
        {
            mysteryBoxAnimator.gameObject.SetActive(true);
            mysteryBoxAnimator.SetBool(IsPlaying, true);

            DOVirtual.DelayedCall(3, () =>
            {
                DisableMysteryBoxAnimation();
            });
        }

        private void DisableMysteryBoxAnimation()
        {
            mysteryBoxAnimator.SetBool(IsPlaying, false);

            mysteryBoxAnimator.gameObject.SetActive(false);
        }
    }
}