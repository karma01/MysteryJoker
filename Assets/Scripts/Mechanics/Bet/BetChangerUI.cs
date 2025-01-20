using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mechanics.Bet
{
    public class BetChangerUI : MonoBehaviour
    {
        public event Action<BetData> OnBetDataChanged;
        [SerializeField] private List<BetDataContainer> betaDataContainers = new List<BetDataContainer>();
        [SerializeField] private List<BetData> betsData = new List<BetData>();
        [SerializeField] private TMP_Text betText;

        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;

        private BetData _currentBetData;
        private int _currentBetDataContainerIndex = 2;

        private void Start()
        {
            increaseButton.onClick.AddListener(() => ChangeBetUI(1));
            decreaseButton.onClick.AddListener(() => ChangeBetUI(-1));

            InitiateBetData();
        }

        private void ChangeBetUI(int direction)
        {
            int newBetIndex = _currentBetData.index + direction;

            increaseButton.interactable = newBetIndex < betsData.Count - 1;
            decreaseButton.interactable = newBetIndex > 0;

            if (newBetIndex < 0 || newBetIndex >= betsData.Count)
            {
                return;
            }


            _currentBetDataContainerIndex += direction;

            foreach (var container in betaDataContainers)
            {
                container.DeselectThisContainer();
            }

            if (_currentBetDataContainerIndex < 0 || _currentBetDataContainerIndex >= betaDataContainers.Count)
            {
                UpdateDisplayedBets(direction);
                _currentBetDataContainerIndex =
                    Mathf.Clamp(_currentBetDataContainerIndex, 0, betaDataContainers.Count - 1);
            }

            var selectedContainer = betaDataContainers[_currentBetDataContainerIndex];
            ChangeBet(selectedContainer.GetContainerBetData());
            selectedContainer.SelectThisContainer();
        }

        private void UpdateDisplayedBets(int direction)
        {
            int shift = direction > 0 ? 1 : -1;
            int startIndex = _currentBetData.index + shift;

            startIndex = Mathf.Clamp(startIndex, 0, betsData.Count);


            if (direction == -1)

            {
                for (int i = 0; i < betaDataContainers.Count; i++)
                {
                    int betIndex = startIndex + i;
                    if (betIndex < betsData.Count)
                    {
                        betaDataContainers[i].SetContainerBetData(betsData[betIndex]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < betaDataContainers.Count; i++)
                {
                    int betIndex = startIndex + i - 4;
                    if (betIndex < betsData.Count)
                    {
                        betaDataContainers[i].SetContainerBetData(betsData[betIndex]);
                    }
                }
            }

            ChangeBet(betsData[startIndex]);
        }

        private void InitiateBetData()
        {
            int centerIndex = 7;
            int startIndex = centerIndex - 2;

            for (int i = 0; i < betaDataContainers.Count; i++)
            {
                int betIndex = startIndex + i;
                if (betIndex < betsData.Count)
                {
                    betaDataContainers[i].SetContainerBetData(betsData[betIndex]);

                    if (betIndex == centerIndex)
                    {
                        betaDataContainers[i].SelectThisContainer();
                        ChangeBet(betsData[betIndex]);
                        _currentBetDataContainerIndex = i;
                    }
                }
            }

            decreaseButton.interactable = _currentBetData.index > 0;
            increaseButton.interactable = _currentBetData.index < betsData.Count - 1;
        }


        public void ChangeBetThroughContainer(BetDataContainer container)
        {
            _currentBetDataContainerIndex = container.ContainerIndexIdentifier;

            foreach (var dataContainer in betaDataContainers)
            {
                dataContainer.DeselectThisContainer();
            }

            container.SelectThisContainer();

            ChangeBet(container.GetContainerBetData());
        }

        private void ChangeBet(BetData betData)
        {
            OnBetDataChanged?.Invoke(betData);

            _currentBetData = betData;
            UpdateBetText(betData);
        }

        private void UpdateBetText(BetData betData)
        {
            betText.text = betData.betAmount.ToString("F2");
        }
    }
}