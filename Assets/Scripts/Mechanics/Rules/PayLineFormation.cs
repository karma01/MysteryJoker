using System;
using System.Collections.Generic;
using System.Linq;
using Mechanics.Slot;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics.Rules
{
    [System.Serializable]
    public class PaylineWinInfo
    {
        public PayLineData payLine;
        public List<SlotBox> slotBoxes = new List<SlotBox>();
        public SlotElement assignedSlotElement;
    }

    public class PayLineFormation : MonoBehaviour
    {
        public event Action<List<PaylineWinInfo>> OnLinesWon;
        public event Action<PaylineWinInfo> OnGetTwoJokersOnMysteryWin;
        public event Action<PaylineWinInfo> OnMysteryJokerOccur;

        [SerializeField] private List<PayLineData> payLineData = new List<PayLineData>();
        [SerializeField] private List<SlotElement> slotElements = new List<SlotElement>();
        [SerializeField] private List<SlotBox> slotBoxes;
        [SerializeField] private WinData winData;

        private List<PaylineWinInfo> _winningPaylines = new List<PaylineWinInfo>();
        private readonly PaylineWinInfo _mysteryJokerLine = new PaylineWinInfo();
        private PaylineWinInfo _twoMysteryJokersSlotInfo = new PaylineWinInfo();

        private bool _isMysterySpin;

        public void FindPayLineToGive(bool isMysterySpin)
        {
            _isMysterySpin = isMysterySpin;

            _winningPaylines.Clear();
            _mysteryJokerLine.slotBoxes.Clear();
            _twoMysteryJokersSlotInfo.slotBoxes.Clear();

            AssignRandomElements();

            CheckPaylines();

            if (_winningPaylines.Count > 0)
            {
                OnLinesWon?.Invoke(_winningPaylines);
            }

            if (_mysteryJokerLine.slotBoxes.Count > 0)
            {
                OnMysteryJokerOccur?.Invoke(_mysteryJokerLine);
            }

            if (_isMysterySpin && HasExactlyTwoMysteryJokers())
            {
                OnGetTwoJokersOnMysteryWin?.Invoke(_twoMysteryJokersSlotInfo);
            }
        }

        /// <summary>
        /// Randomly assigns slot elements to all slot boxes.
        /// </summary>
        private void AssignRandomElements()
        {
            List<float> cumulativeProbabilities = new List<float>();
            float cumulativeSum = 0;

            foreach (var element in slotElements)
            {
                cumulativeSum += element.probabilityOfOccurrence;
                cumulativeProbabilities.Add(cumulativeSum);
            }

            foreach (var slotBox in slotBoxes)
            {
                SlotElement randomElement = GetRandomSlotElement(cumulativeProbabilities, cumulativeSum);
                slotBox.SetSlotElement(randomElement);
            }
        }

        /// <summary>
        /// Checks all paylines to see if they form a win with the current slot box elements.
        /// </summary>
        private void CheckPaylines()
        {
            foreach (var lineData in payLineData)
            {
                if (IsWinningPayline(lineData, out PaylineWinInfo winInfo))
                {
                    _winningPaylines.Add(winInfo);
                }
            }
        }

        /// <summary>
        /// Checks if the given payline forms a winning combination.
        /// </summary>
        private bool IsWinningPayline(PayLineData payLine, out PaylineWinInfo winInfo)
        {
            winInfo = new PaylineWinInfo { payLine = payLine, slotBoxes = new List<SlotBox>() };

            SlotElement firstElement = null;

            foreach (var slotBoxIdentifier in payLine.combinationsList)
            {
                var slotBox = GetSlotBoxAtPosition(slotBoxIdentifier.slotSlotBoxIdentifier.index,
                    slotBoxIdentifier.slotSlotBoxIdentifier.row);

                if (slotBox == null)
                    return false;

                var currentElement = slotBox.GetSlotElement();

                if (firstElement == null)
                {
                    firstElement = currentElement;
                }
                else if (currentElement != firstElement)
                {
                    return false;
                }

                winInfo.slotBoxes.Add(slotBox);
            }

            winInfo.assignedSlotElement = firstElement;

            if (firstElement != null && firstElement.elementData == BasicElementData.Mystery)
            {
                _mysteryJokerLine.slotBoxes.AddRange(winInfo.slotBoxes);
            }

            return true;
        }

        /// <summary>
        /// Retrieves a slot box at a specific position.
        /// </summary>
        private SlotBox GetSlotBoxAtPosition(int index, int row)
        {
            return slotBoxes.FirstOrDefault(box =>
            {
                var data = box.GetSlotBoxData();
                return data.slotSlotBoxIdentifier.index == index && data.slotSlotBoxIdentifier.row == row;
            });
        }

        /// <summary>
        /// Checks if exactly two mystery jokers exist in the current setup.
        /// </summary>
        private bool HasExactlyTwoMysteryJokers()
        {
            int mysteryCount = 0;
            _twoMysteryJokersSlotInfo.slotBoxes.Clear();

            foreach (var slotBox in slotBoxes)
            {
                if (slotBox.GetSlotElement()?.elementData == BasicElementData.Mystery)
                {
                    _twoMysteryJokersSlotInfo.slotBoxes.Add(slotBox);
                    mysteryCount++;
                }
            }

            return mysteryCount == 2;
        }

        /// <summary>
        /// Performs weighted random selection of slot elements.
        /// </summary>
        private SlotElement GetRandomSlotElement(List<float> cumulativeProbabilities, float cumulativeSum)
        {
            float randomValue = Random.Range(0, cumulativeSum);

            for (int i = 0; i < slotElements.Count; i++)
            {
                if (randomValue <= cumulativeProbabilities[i])
                {
                    return slotElements[i];
                }
            }

            return slotElements[0];
        }
    }
}