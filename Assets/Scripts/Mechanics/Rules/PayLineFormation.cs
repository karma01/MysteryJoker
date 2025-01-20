using System;
using System.Collections.Generic;
using System.Linq;
using Mechanics.Slot;
using NaughtyAttributes;
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

        [Header("Normal PayLines")]
        [Tooltip(
            "Notice:  dont set the combination list to joker payLines, if you do then they can also occur on that specified payLine")]
        [SerializeField]
        private List<PayLineData> payLineData = new List<PayLineData>();


        [SerializeField] private List<SlotElement> slotElements = new List<SlotElement>();
        [SerializeField] private List<SlotBox> slotBoxes;

        [SerializeField] private WinData winData;

        private List<PaylineWinInfo> _winningPaylines = new List<PaylineWinInfo>();
        readonly PaylineWinInfo _mysteryJokerLine = new PaylineWinInfo();

        private PaylineWinInfo _twoMysteryJokersSlotInfo = new PaylineWinInfo();

        private bool _isMysterySpin;

        public void FindPayLineToGive(bool isMysterySpin)
        {
            _isMysterySpin = isMysterySpin;

            _mysteryJokerLine.slotBoxes.Clear();

            _winningPaylines.Clear();
            _twoMysteryJokersSlotInfo.slotBoxes.Clear();
            
            int count = CalculateWinsToSelect();

            if (count == 0)
            {
                Debug.Log("No Win");
                ShuffleSlotElementsWithoutWins();
            }
            else
            {
                ShuffleSlotElementsWithoutWins();
                Debug.Log("Win");
                SelectSlotElement(count);
            }

            // AddAdditionalWinningPaylines();
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
                // Give mystery reward
            }

            //LogWinningPaylines();
        }


        #region NotWin

        /// <summary>
        /// Randomly assigns slot elements to slot boxes, ensuring no winning paylines.
        /// </summary>
        private void ShuffleSlotElementsWithoutWins()
        {
            bool hasWinningLine;
            int retryCount = 0;
            const int maxRetries = 2000;

            // Calculate cumulative probabilities for weighted random selection
            List<float> cumulativeProbabilities = new List<float>();
            float cumulativeSum = 0;
            foreach (var slotElement in slotElements)
            {
                cumulativeSum += slotElement.probabilityOfOccurrence;
                cumulativeProbabilities.Add(cumulativeSum);
            }

            do
            {
                hasWinningLine = false;
                int mysteryCount = 0;

                foreach (var slotBox in slotBoxes)
                {
                    SlotElement randomElement;

                    // Perform weighted random selection
                    do
                    {
                        randomElement = GetRandomSlotElement(cumulativeProbabilities, cumulativeSum);
                    } while (randomElement.elementData == BasicElementData.Mystery && mysteryCount >= 2);

                    if (randomElement.elementData == BasicElementData.Mystery)
                    {
                        mysteryCount++;
                    }

                    slotBox.SetSlotElement(randomElement);
                }

                // Check for any winning paylines
                foreach (var lineData in payLineData)
                {
                    if (IsWinningPayline(lineData))
                    {
                        retryCount -= 100; // Adjust retry counter if a winning line is formed
                        hasWinningLine = true;
                        break;
                    }
                }

                retryCount++;
                if (retryCount > maxRetries)
                {
                    Debug.LogWarning("Max retries reached while shuffling slot elements without forming a win.");
                    break;
                }
            } while (hasWinningLine); // Keep retrying until no winning paylines are formed.
        }

        /// <summary>
        /// Performs weighted random selection based on slot element probabilities.
        /// </summary>
        /// <param name="cumulativeProbabilities">Cumulative probabilities of the slot elements.</param>
        /// <param name="cumulativeSum">Total sum of all probabilities.</param>
        /// <returns>A randomly selected SlotElement based on its probability.</returns>
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


        /// <summary>
        /// Checks if the given payline is a winning payline based on current slot box data.
        /// </summary>
        /// <param name="payLine">The payline to check.</param>
        /// <returns>True if the payline is a winning payline, false otherwise.</returns>
        private bool IsWinningPayline(PayLineData payLine)
        {
            SlotElement firstElement = null;

            foreach (var slotBoxIdentifier in payLine.combinationsList)
            {
                SlotBox slotBox = GetSlotBoxAtPosition(slotBoxIdentifier.slotSlotBoxIdentifier.index,
                    slotBoxIdentifier.slotSlotBoxIdentifier.row);


                if (slotBox == null)
                {
                    return false;
                }

                SlotElement currentElement = slotBox.GetSlotElement();


                if (!firstElement)
                {
                    firstElement = currentElement;
                }
                else if (currentElement != firstElement)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Retrieves the slot box at the given index and row.
        /// </summary>
        /// <param name="index">The index of the slot box.</param>
        /// <param name="row">The row of the slot box.</param>
        /// <returns>The SlotBox at the given position, or null if not found.</returns>
        private SlotBox GetSlotBoxAtPosition(int index, int row)
        {
            foreach (var slotBox in slotBoxes)
            {
                SlotBoxData data = slotBox.GetSlotBoxData();
                if (data.slotSlotBoxIdentifier.index == index && data.slotSlotBoxIdentifier.row == row)
                {
                    return slotBox;
                }
            }

            return null;
        }

        #endregion

        #region Win

        private List<SlotElement> _chosenSlotElements = new List<SlotElement>();

        private void SelectSlotElement(int count)
        {
            _chosenSlotElements.Clear();

            List<float> cumulativeProbabilities = new List<float>();
            float cumulativeSum = 0;

            foreach (var element in slotElements)
            {
                cumulativeSum += element.probabilityOfOccurrence;
                cumulativeProbabilities.Add(cumulativeSum);
            }

            int retryCount = 0;
            const int maxRetries = 100;

            while (_chosenSlotElements.Count < count && retryCount < maxRetries)
            {
                float randomVal = Random.Range(0, cumulativeSum);

                for (int i = 0; i < slotElements.Count; i++)
                {
                    if (randomVal <= cumulativeProbabilities[i])
                    {
                        if (!_chosenSlotElements.Contains(slotElements[i]))
                        {
                            _chosenSlotElements.Add(slotElements[i]);
                        }

                        break;
                    }
                }

                retryCount++;
            }

            if (_chosenSlotElements.Count == 0)
            {
                SlotElement lowValueElement = null;
                float lowestProbability = float.MaxValue;

                // Find the element with the lowest probability
                foreach (var element in slotElements)
                {
                    if (element.probabilityOfOccurrence < lowestProbability)
                    {
                        lowValueElement = element;
                        lowestProbability = element.probabilityOfOccurrence;
                    }
                }

                if (lowValueElement )
                {
                    _chosenSlotElements.Add(lowValueElement);
                }
            }

            SelectPayLine(count, _chosenSlotElements);
        }


        private List<PayLineData> _chosenPayLines = new List<PayLineData>();

        private void SelectPayLine(int count, List<SlotElement> chosenSlotElements)
        {
            _chosenPayLines.Clear();
            List<float> cumulativeProbabilities = new List<float>();
            float cumulativeSum = 0;

            foreach (var lineData in payLineData)
            {
                cumulativeSum += lineData.probability;
                cumulativeProbabilities.Add(cumulativeSum);
            }

            while (_chosenPayLines.Count < count)
            {
                float randomValue = Random.Range(0, cumulativeSum);
                for (int j = 0; j < payLineData.Count; j++)
                {
                    if (randomValue <= cumulativeProbabilities[j])
                    {
                        if (!_chosenPayLines.Contains(payLineData[j]))
                        {
                            _chosenPayLines.Add(payLineData[j]);
                        }

                        break;
                    }
                }
            }

            GeneratePayLines(_chosenPayLines, chosenSlotElements);
        }


        private void GeneratePayLines(List<PayLineData> chosenPayLines, List<SlotElement> chosenSlotElements)
        {
            bool hasMysteryJoker = false;
            SlotElement mysteryJoker =
                chosenSlotElements.FirstOrDefault(e => e.elementData == BasicElementData.Mystery);

            if (mysteryJoker)
            {
                hasMysteryJoker = true;


                chosenSlotElements.RemoveAll(e => e.elementData == BasicElementData.Mystery);
            }

            if (chosenSlotElements.Count > 0)
            {
                GenerateOtherPayLines(chosenPayLines, chosenSlotElements);
            }

            if (hasMysteryJoker)
            {
                Debug.Log("Give free spins here and also if there are two jokers give mystery win ");
                HandleMysteryJoker(chosenPayLines, chosenSlotElements, mysteryJoker);
            }

          
        }


        private void GenerateOtherPayLines(List<PayLineData> chosenPayLines, List<SlotElement> chosenSlotElements)
        {
            List<SlotElement> chosenElements = new List<SlotElement>();
            HashSet<SlotBox> filledSlotBoxes = new HashSet<SlotBox>();

            foreach (var lineData in chosenPayLines)
            {
                SlotElement assignedElement = null;

                if (lineData.payLineType is PayLineData.PayLineType.DiagonalLeft
                    or PayLineData.PayLineType.DiagonalRight)
                {
                    if (chosenElements.Count > 0)
                    {
                        assignedElement = chosenElements[0];
                    }
                    else
                    {
                        assignedElement = chosenSlotElements[Random.Range(0, chosenSlotElements.Count)];
                        chosenElements.Add(assignedElement);
                    }
                }
                else
                {
                    bool elementAssigned = false;
                    int attemptCount = 0;

                    while (!elementAssigned && attemptCount < Int32.MaxValue)
                    {
                        assignedElement = chosenSlotElements[Random.Range(0, chosenSlotElements.Count)];
                        bool willFormWin = false;

                        foreach (var otherLineData in payLineData)
                        {
                            if (otherLineData != lineData)
                            {
                                if (WillAssignmentCauseWin(otherLineData, assignedElement, filledSlotBoxes))
                                {
                                    willFormWin = true;
                                    break;
                                }
                            }
                        }

                        if (!willFormWin && !chosenElements.Contains(assignedElement))
                        {
                            chosenElements.Add(assignedElement);
                            elementAssigned = true;
                        }

                        attemptCount++;
                    }

                    if (!elementAssigned)
                    {
                        throw new Exception("Failed to assign a valid element without causing unintended wins.");
                    }
                }

                var paylineInfo = new PaylineWinInfo
                {
                    payLine = lineData,
                    assignedSlotElement = assignedElement,
                    slotBoxes = new List<SlotBox>()
                };

                foreach (var combination in lineData.combinationsList)
                {
                    SlotBox slotBox = GetSlotBoxAtPosition(combination.slotSlotBoxIdentifier.index,
                        combination.slotSlotBoxIdentifier.row);

                    if (slotBox != null && !filledSlotBoxes.Contains(slotBox))
                    {
                        slotBox.SetSlotElement(assignedElement);
                        filledSlotBoxes.Add(slotBox);
                        paylineInfo.slotBoxes.Add(slotBox);
                    }
                }

                _winningPaylines.Add(paylineInfo);
            }
        }

        /// <summary>
        /// Simulates the assignment of an element and checks if it will cause a win in the given payline.
        /// </summary>
        /// <param name="payLine">The payline to validate.</param>
        /// <param name="element">The slot element to simulate assigning.</param>
        /// <param name="filledSlotBoxes">The currently filled slot boxes.</param>
        /// <returns>True if the assignment causes a win; otherwise, false.</returns>
        private bool WillAssignmentCauseWin(PayLineData payLine, SlotElement element, HashSet<SlotBox> filledSlotBoxes)
        {
            int matchingElementCount = 0;

            foreach (var combination in payLine.combinationsList)
            {
                SlotBox slotBox = GetSlotBoxAtPosition(combination.slotSlotBoxIdentifier.index,
                    combination.slotSlotBoxIdentifier.row);

                if (slotBox == null)
                {
                    return false; // If any slot box is null, it can't form a win.
                }

                if (filledSlotBoxes.Contains(slotBox))
                {
                    if (slotBox.GetSlotElement() == element)
                    {
                        matchingElementCount++;
                    }
                }
                else
                {
                    // Simulate the assignment
                    if (element == slotBox.GetSlotElement())
                    {
                        matchingElementCount++;
                    }
                }
            }

            return matchingElementCount >= 3; // Adjust the threshold based on your win conditions
        }


        private void HandleMysteryJoker(List<PayLineData> chosenPayLines, List<SlotElement> chosenSlotElements,
            SlotElement mysteryJoker)
        {
            if (chosenSlotElements.Count > 0)
            {
                HashSet<SlotBox> occupiedSlotBoxes = new HashSet<SlotBox>();


                foreach (var chosenPayLine in chosenPayLines)
                {
                    foreach (var cData in chosenPayLine.combinationsList)
                    {
                        SlotBox slotBox = GetSlotBoxAtPosition(cData.slotSlotBoxIdentifier.index,
                            cData.slotSlotBoxIdentifier.row);

                        if (slotBox)
                        {
                            occupiedSlotBoxes.Add(slotBox);
                        }
                    }
                }

                List<SlotBox> availableSlotBoxes =
                    slotBoxes.Where(slotBox => !occupiedSlotBoxes.Contains(slotBox)).ToList();

                for (int i = 0; i < 3; i++)
                {
                    int index = Random.Range(0, availableSlotBoxes.Count);
                    SlotBox slotBox = availableSlotBoxes[index];

                    slotBox.SetSlotElement(mysteryJoker);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                int index = Random.Range(0, slotBoxes.Count);
                SlotBox slotBox = slotBoxes[index];

                slotBox.SetSlotElement(mysteryJoker);

                _mysteryJokerLine.slotBoxes.Add(slotBox);
            }


        }

        private bool HasExactlyTwoMysteryJokers()
        {
            int mysteryJokerCount = 0;
            _twoMysteryJokersSlotInfo.slotBoxes.Clear();

            foreach (var slotBox in slotBoxes)
            {
                var slotElement = slotBox.GetSlotElement();
                if (slotElement != null && slotElement.elementData == BasicElementData.Mystery)
                {
                    mysteryJokerCount++;
                    if (mysteryJokerCount <= 2)
                    {
                        _twoMysteryJokersSlotInfo.slotBoxes.Add(slotBox);
                    }
                }
            }

            if (mysteryJokerCount == 2)
            {
                return true;
            }

            _twoMysteryJokersSlotInfo.slotBoxes.Clear();
            return false;
        }



        private int CalculateWinsToSelect()
        {
            if (!ShouldTriggerWin())
            {
                return 0;
            }

            int maxWins = Mathf.Min(winData.maxWinsInRound, payLineData.Count);

            return Random.Range(1, maxWins + 1);
        }

        private bool ShouldTriggerWin()
        {
            return Random.Range(0, 100 + 1) <= winData.probabilityForOneWin;
        }

        #endregion

        //test
        private void LogWinningPaylines()
        {
            foreach (var winInfo in _winningPaylines)
            {
                Debug.Log($"Payline: {winInfo.payLine.name}, Assigned Element: {winInfo.assignedSlotElement.name}");
                foreach (var slotBox in winInfo.slotBoxes)
                {
                    Debug.Log($"SlotBox: {slotBox.name}, Element: {slotBox.GetSlotElement().name}");
                }
            }
        }
    }
}