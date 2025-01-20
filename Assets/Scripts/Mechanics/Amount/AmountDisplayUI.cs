using System;
using TMPro;
using UnityEngine;

namespace Mechanics.Amount
{
    public class AmountDisplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text totalBalanceText;
        [SerializeField] private TMP_Text totalWinText;

        private void Awake()
        {
            AmountHandler.OnTotalWinChange += OnTotalWinAmountChange;
            AmountHandler.OnTotalBalanceChange += OnTotalBalanceChange;
        }

        private void OnTotalBalanceChange(float amount)
        {
            totalBalanceText.text = amount.ToString("F2");
        }

        private void OnTotalWinAmountChange(float amount)
        {
            totalWinText.text = amount.ToString("F2");
        }
    }
}