using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mechanics.Slot
{
    [CreateAssetMenu(menuName = "Slotdata/PayLineData", fileName = "PayLine")]
    public class PayLineData : ScriptableObject
    {
        public enum PayLineType
        {
            Diagonal, Linear
        }
        public List<SlotBoxData> combinationsList;
        public float probability;
        public PayLineType payLineType;
    }

    [Serializable]
    public class SlotBoxIdentifier
    {
        public int row;
        public int index;
    }
}