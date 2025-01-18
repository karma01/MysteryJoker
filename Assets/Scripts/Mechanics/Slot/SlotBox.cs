using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mechanics.Slot
{
    public class SlotBox : MonoBehaviour
    {
        [SerializeField] private SlotBoxData slotBoxData;
        [SerializeField] private SlotElement slotElement;

        [SerializeField] private Image elementIdentifierImage;

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
    }
}