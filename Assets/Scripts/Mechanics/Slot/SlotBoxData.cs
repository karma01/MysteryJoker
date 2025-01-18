using UnityEngine;
using UnityEngine.Serialization;

namespace Mechanics.Slot
{
    [CreateAssetMenu(menuName = "Slotdata/SlotBox", fileName = "SLotData")]
    public class SlotBoxData : ScriptableObject
    {
          public SlotBoxIdentifier slotSlotBoxIdentifier;
    }
}