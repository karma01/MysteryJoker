using UnityEngine;

namespace Mechanics.Bet
{
    [CreateAssetMenu(menuName = "BetRelated/BetData", fileName = "BetDataSO")]
    public class BetData : ScriptableObject
    {
        public int index;
        public float betAmount;
    }
}