using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mechanics.Win
{
    [CreateAssetMenu(menuName = "DigitMapper", fileName = "Digit Mapper")]
    public class DigitMapper : ScriptableObject
    {
        [Serializable]
        public class DigitData
        {
            public Sprite digitSprite;
            public int index;
        }

        public List<DigitData> digitData = new List<DigitData>();

        public Sprite GetSpriteForNumber(int index)
        {
            foreach (var data in digitData.Where(data => index == data.index))
            {
                return data.digitSprite;
            }

            return null;
        }
    }
}