using System;
using System.Collections.Generic;
using Mechanics.Bet;
using Mechanics.Rules;
using Mechanics.Slot;
using UnityEngine;

namespace Mechanics.Amount
{
    public class AmountHandler : MonoBehaviour
    {
        public static event Action<float> OnTotalBalanceChange;
        public static event Action<float> OnTotalWinChange;
        private float _totalAmount;

        [SerializeField] private BetChangerUI betChangerUI;
        private BetData _chosenBetData;

        private static AmountHandler _instance;


        public static AmountHandler GetInstance()
        {
            return _instance;
        }

        private void Awake()
        {
            if (!_instance)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            betChangerUI.OnBetDataChanged += BetChangerUIOnOnBetDataChanged;
        }

        private void BetChangerUIOnOnBetDataChanged(BetData obj)
        {
            _chosenBetData = obj;
        }


        private const string BalanceStringKey = "TotalBalance";

        private void Start()
        {
            _totalAmount = PlayerPrefs.GetFloat(BalanceStringKey, 25000);
            OnTotalBalanceChange?.Invoke(_totalAmount);
        }


        public void OnGameInitiateAndNotFreeSpin()
        {
            _totalAmount -= _chosenBetData.betAmount;

            PlayerPrefs.SetFloat(BalanceStringKey, _totalAmount);

            OnTotalBalanceChange?.Invoke(_totalAmount);
        }

        public bool IsAmountGreaterForSpin()
        {
            return _totalAmount > _chosenBetData.betAmount;
        }

        private float _amountWonInASpin;

        public void CalculateTotalWin(List<PaylineWinInfo> payLineInfo)
        {
            foreach (var payLineWinInfo in payLineInfo)
            {
                if (payLineWinInfo.assignedSlotElement.elementData != BasicElementData.Mystery)
                {
                    _amountWonInASpin += payLineWinInfo.assignedSlotElement.prize * _chosenBetData.betAmount;
                }
            }

            OnTotalWinChange?.Invoke(_amountWonInASpin);
        }

        public void CalculateTotalWin(float amount)
        {
            _amountWonInASpin += amount;
            OnTotalWinChange?.Invoke(_amountWonInASpin);
        }

        public void ResetAmountWon()
        {
            _totalAmount += _amountWonInASpin;

            OnTotalBalanceChange?.Invoke(_totalAmount);
            _amountWonInASpin = 0;

            OnTotalWinChange?.Invoke(_amountWonInASpin);
        }
    }
}