using System.Collections.Generic;
using System.Linq;
using Mechanics.Slot;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics.Rules
{
    public class SlotRule : MonoBehaviour
    {
        [Header("Normal PayLines")]
        [Tooltip(
            "Notice:  dont set the combination list to joker payLines, if you do then they can also occur on that specified payLine")]
        [SerializeField]
        private List<PayLineData> payLineData = new List<PayLineData>();


        [SerializeField] private List<SlotElement> slotElements = new List<SlotElement>();
        [SerializeField] private List<SlotBox> slotBoxes;

        [SerializeField] private WinData winData;

        private float _totalProbabilityPayLine;
        private float _totalProbabilityOfOccuranceElements;


        private List<PayLineData> _choosenPayLineData = new List<PayLineData>();

        private void Start()
        {
            foreach (var lineData in payLineData)
            {
                _totalProbabilityPayLine += lineData.probability;
            }

            foreach (var slotElement in slotElements)
            {
                _totalProbabilityOfOccuranceElements += slotElement.probabilityOfOccurrence;
            }
        }

        [Button]
        public void FindPayLineToGive()
        {
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

                if (lowValueElement != null)
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

            // Calculate cumulative probabilities for the paylines.
            foreach (var lineData in payLineData)
            {
                cumulativeSum += lineData.probability;
                cumulativeProbabilities.Add(cumulativeSum);
            }

            // Select only 'count' number of paylines.
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
            HashSet<SlotBox> filledSlotBoxes = new HashSet<SlotBox>(); // Track filled slot boxes

            Debug.Log("Chosen payLines are: " + chosenPayLines.Count);

            foreach (var lineData in chosenPayLines)
            {
                SlotElement assignedElement = null;

                if (lineData.payLineType == PayLineData.PayLineType.Diagonal)
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
                else // Handle non-diagonal payline
                {
                    bool elementAssigned = false;
                    int attemptCount = 0;

                    while (!elementAssigned && attemptCount < 10) // Limit attempts to avoid infinite loop
                    {
                        assignedElement = chosenSlotElements[Random.Range(0, chosenSlotElements.Count)];

                        // Check if placing this element would form a winning payline in any other payline
                        bool willFormWin = false;
                        foreach (var otherLineData in payLineData)
                        {
                            if (otherLineData != lineData && IsWinningPayline(otherLineData))
                            {
                                willFormWin = true;
                                break;
                            }
                        }

                        // Only assign the element if it won't form an unintended win
                        if (!willFormWin && !chosenElements.Contains(assignedElement))
                        {
                            chosenElements.Add(assignedElement);
                            elementAssigned = true;
                        }

                        attemptCount++;
                    }
                }

                // Now assign the element to the slot boxes in the payline
                foreach (var combination in lineData.combinationsList)
                {
                    SlotBox slotBox = GetSlotBoxAtPosition(combination.slotSlotBoxIdentifier.index,
                        combination.slotSlotBoxIdentifier.row);

                    if (slotBox != null && !filledSlotBoxes.Contains(slotBox))
                    {
                        slotBox.SetSlotElement(assignedElement);
                        filledSlotBoxes.Add(slotBox); // Mark this slot as filled
                    }
                }
            }
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
            }

            //give free 10 spins and also when there are two mystery jokers give mystery win
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
    }
}