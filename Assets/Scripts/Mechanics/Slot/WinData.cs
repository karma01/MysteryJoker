using UnityEngine;

namespace Mechanics.Slot
{
    [CreateAssetMenu(menuName = "Slotdata/WinData", fileName = "Win Data")]
    public class WinData : ScriptableObject
    {
        public int maxWinsInRound;
        [Range(0, 100)] public float probabilityForOneWin;
    }
}