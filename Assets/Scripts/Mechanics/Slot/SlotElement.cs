using UnityEngine;
using UnityEngine.Serialization;

namespace Mechanics.Slot
{
    public enum BasicElementData
    {
        Star,
        Lemon,
        Bell,
        Mystery,
        Cherry,
        Grape
    }


    [CreateAssetMenu(menuName = "Slotdata/SlotElement", fileName = "SlotElement")]
    public class SlotElement : ScriptableObject
    {
        public BasicElementData elementData;
        public float probabilityOfOccurrence;
        public int prize;

        public Sprite elementSprite;
    }
}