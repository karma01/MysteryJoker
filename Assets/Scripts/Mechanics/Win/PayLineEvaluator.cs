using System;
using System.Collections.Generic;
using Animations;
using Mechanics.Amount;
using Mechanics.Bet;
using Mechanics.Rules;
using Mechanics.Slot;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mechanics.Win
{
    public class PayLineEvaluator : MonoBehaviour
    {
        private BetData _betData;

        private List<PayLineData> _currentWinPayLineData = new List<PayLineData>();
        private List<SlotElement> _currentSlotElement = new List<SlotElement>();

        [FormerlySerializedAs("slotRule")] [SerializeField]
        private PayLineFormation payLineFormation;

        [SerializeField] private ElementSlideAnimation elementAnimation;
        [SerializeField] private WinLinesDisplayWin linesDisplayWin;
        [SerializeField] private BetChangerUI betChangerUI;


        private List<PaylineWinInfo> _currentPayLineInfo = new List<PaylineWinInfo>();

        private void Awake()
        {
            payLineFormation.OnLinesWon += PayLineFormationOnOnLinesWon;
            elementAnimation.OnSlotChangeState += ElementAnimationOnOnSlotChangeState;
            betChangerUI.OnBetDataChanged += SetBetData;
        }

        private void ElementAnimationOnOnSlotChangeState(bool obj)
        {
            if (_currentPayLineInfo.Count <= 0)
            {
                return;
            }
            // Send Data to calculate Amount
            if (!obj)
            {
                TopVisualUIManager.GetInstance().SetNormalTexts("CONGRATULATIONS !!!");

            }

            if (obj)
            {
                linesDisplayWin.StopSequentialAnimation();

                //Reset the amount
                AmountHandler.GetInstance().ResetAmountWon();
            }
            else
            {
                AmountHandler.GetInstance().CalculateTotalWin(_currentPayLineInfo);

                foreach (var paylineWinInfo in _currentPayLineInfo)
                {
                    switch (paylineWinInfo.payLine.payLineType)
                    {
                        case PayLineData.PayLineType.DiagonalLeft:
                            linesDisplayWin.SetLDiagonalToAnimate(paylineWinInfo.assignedSlotElement.prize *
                                                                  (int)_betData.betAmount);
                            break;

                        case PayLineData.PayLineType.DiagonalRight:
                            linesDisplayWin.SetRDiagonalToAnimate(paylineWinInfo.assignedSlotElement.prize *
                                                                  (int)_betData.betAmount);
                            break;
                        case PayLineData.PayLineType.TopRow:
                            linesDisplayWin.SetTopToAnimate(paylineWinInfo.assignedSlotElement.prize *
                                                            (int)_betData.betAmount);
                            break;
                        case PayLineData.PayLineType.MidRow:
                            linesDisplayWin.SetMidToAnimate(paylineWinInfo.assignedSlotElement.prize *
                                                            (int)_betData.betAmount);
                            break;
                        case PayLineData.PayLineType.LastRow:
                            linesDisplayWin.SetLastToAnimate(paylineWinInfo.assignedSlotElement.prize *
                                                             (int)_betData.betAmount);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                linesDisplayWin.AnimateWinLines();
            }
        }

        private void PayLineFormationOnOnLinesWon(List<PaylineWinInfo> obj)
        {
            _currentPayLineInfo = obj;
        }

        private void SetBetData(BetData data)
        {
            _betData = data;
        }


        private void FindPayLinePosition()
        {
            //Animate them with values
        }

        public void SetSlotElementData(List<SlotElement> slotElements)
        {
            _currentSlotElement = slotElements;
            CalculateAmountWon();
        }

        private void CalculateAmountWon()
        {
            float amountWon = 0;

            foreach (var slotElement in _currentSlotElement)
            {
                amountWon += _betData.betAmount * slotElement.prize;
            }

            //Send this to the amount Handler
        }
    }
}