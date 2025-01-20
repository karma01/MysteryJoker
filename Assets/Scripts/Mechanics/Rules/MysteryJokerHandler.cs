using System.Collections;
using Animations;
using DG.Tweening;
using UI;
using UnityEngine;

namespace Mechanics.Rules
{
    public class MysteryJokerHandler : MonoBehaviour
    {
        public bool canMysterySpin => _canMysterySpin;
        [SerializeField] private PayLineFormation payLineFormation;

        [SerializeField] private MysteryJokerUI mysteryJokerUI;
        [SerializeField] private ElementSlideAnimation elementSlideAnimation;
        [SerializeField] private StartButtonBehaviour startButtonBehaviour;

        [SerializeField] private WinLinesDisplayWin winLinesDisplayWin;

        private int _currentSpinCount;
        private int _currentSpinCountAscend;
        private int _totalSpinCount;

        private bool _canMysterySpin = false;


        private PaylineWinInfo _twoMysteryPayLine = new PaylineWinInfo();

        private void Awake()
        {
            payLineFormation.OnMysteryJokerOccur += OnMysteryJokerOccur;


            startButtonBehaviour.OnStartButtonClicked += OnStartButtonClicked;


            winLinesDisplayWin.OnAllAnimationsCompleted += ElementSlideAnimationOnOnSlotChangeState;

            payLineFormation.OnGetTwoJokersOnMysteryWin += PayLineFormationOnOnGetTwoJokersOnMysteryWin;

            elementSlideAnimation.OnSlotChangeState += ElementSlideAnimationOnOnSlotChangeState;
        }

        private void PayLineFormationOnOnGetTwoJokersOnMysteryWin(PaylineWinInfo obj)
        {
            //  _canMysterySpin = false;
            _twoMysteryPayLine = obj;
        }

        private void ElementSlideAnimationOnOnSlotChangeState(bool obj)
        {
            _canMysterySpin = _currentSpinCount > 0;
            if (!_canMysterySpin)
            {
                _currentSpinCountAscend = 0;
                mysteryJokerUI.ChangeToNormalTheme();
            }

            if (_animationRoutine != null)
            {
                StopCoroutine(_animationRoutine);
                _animationRoutine = null;
            }

            _isAnimationPlaying = obj; // Mark animation state based on `obj`

            if (!obj) // Animation completed
            {
                if (_twoMysteryPayLine.slotBoxes.Count > 0)
                {
                    //_canMysterySpin = false;
                    GiveRandomMysteryLoot();
                }
            }
        }

        private bool _isAnimationPlaying = false;

        private void GiveRandomMysteryLoot()
        {
            _isAnimationPlaying = true; // Animation starts

            int amountToGive = Random.Range(10, 1000);
            mysteryJokerUI.ShowMysteryWinUI(amountToGive);

            DOVirtual.DelayedCall(6f, () =>
            {
                _isAnimationPlaying = false; // Animation ends
                if (!_hasJokerOccured)
                {
                    _canMysterySpin = true;
                    ElementSlideAnimationOnOnSlotChangeState();
                }
            });
        }


        private void OnStartButtonClicked()
        {
            _canMysterySpin = _currentSpinCount > 0;

            ElementSlideAnimationOnOnSlotChangeState();
        }

        private Coroutine _animationRoutine;

        private void ElementSlideAnimationOnOnSlotChangeState()
        {
            _canMysterySpin = _currentSpinCount > 0;


            if (_canMysterySpin)
            {
                TopVisualUIManager.GetInstance()
                    .SetNormalTexts("FREE SPIN " + _currentSpinCountAscend + " OF " + _totalSpinCount);
                if (_animationRoutine == null)
                {
                    _animationRoutine = StartCoroutine(InitiateSpin());
                }
            }
        }


        private bool _hasJokerOccured;

        private void OnMysteryJokerOccur(PaylineWinInfo paylineWinInfos)
        {
            // _canMysterySpin = false;

            _hasJokerOccured = true;

            mysteryJokerUI.ShowJokerAnimation(paylineWinInfos);
            //Give 10 spins
            _currentSpinCount += 10;
            _totalSpinCount += 10;

            // Generate a panel showing player has won 10 spins // after panel disappers generate animations and start spinning
            //Check for two mystery jokers and if they occur then give player mystery gift
        }

        private IEnumerator InitiateSpin()
        {
            if (_currentSpinCount <= 0 || _isAnimationPlaying)
            {
                yield break;
            }

            _currentSpinCount--;
            _currentSpinCountAscend++;
            
            if (_canMysterySpin)
            {
                TopVisualUIManager.GetInstance()
                    .SetNormalTexts("FREE SPIN " + _currentSpinCountAscend + " OF " + _totalSpinCount);

                elementSlideAnimation.InitiateMovement();
                DOVirtual.DelayedCall(0.4f, () => { payLineFormation.FindPayLineToGive(true); });

                _hasJokerOccured = false;
            }

            if (_currentSpinCount == 0)
            {
                _totalSpinCount = 0;
                _currentSpinCountAscend = 0;

                TopVisualUIManager.GetInstance().SetNormalTexts("GOOD LUCK");
            }
        }
    }
}