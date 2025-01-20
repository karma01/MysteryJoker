using System.Collections;
using System.Collections.Generic;
using Mechanics.Win;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WinLinesDisplayWin : MonoBehaviour
    {
        [System.Serializable]
        private class WinLineAnimationData
        {
            public Animator topRowAnimator;

            public int amount;
            public Image topOnePlaceText;
            public Image topTenthPlaceText;
            public Image topHundredthPlaceText;
            public Image topThousandPlaceText;
        }

        public event System.Action OnAllAnimationsCompleted;

        
        [SerializeField] private WinLineAnimationData topRowAnimationData;
        [SerializeField] private WinLineAnimationData midRowAnimationData;
        [SerializeField] private WinLineAnimationData lastRowAnimationData;
        [SerializeField] private WinLineAnimationData lDiagonalAnimationData;
        [SerializeField] private WinLineAnimationData rDiagonalAnimationData;

        [SerializeField] private DigitMapper digitSpriteMapping;


        private static readonly int IsPlaying = Animator.StringToHash("isPlaying");

        private bool _isAnimating = false;


        private List<WinLineAnimationData> _animatingDataList = new List<WinLineAnimationData>();


        public void SetMidToAnimate(int amount)
        {
            midRowAnimationData.amount = amount;
            _animatingDataList.Add(midRowAnimationData);
        }

        public void SetTopToAnimate(int amount)
        {
            topRowAnimationData.amount = amount;
            _animatingDataList.Add(topRowAnimationData);
        }

        public void SetLastToAnimate(int amount)
        {
            lastRowAnimationData.amount = amount;
            _animatingDataList.Add(lastRowAnimationData);
        }

        public void SetRDiagonalToAnimate(int amount)
        {
            lDiagonalAnimationData.amount = amount;
            _animatingDataList.Add(lDiagonalAnimationData);
        }

        public void SetLDiagonalToAnimate(int amount)
        {
            rDiagonalAnimationData.amount = amount;
            _animatingDataList.Add(rDiagonalAnimationData);
        }

        public void AnimateWinLines()
        {
            StartSequentialAnimation(_animatingDataList);
        }

        private Coroutine _animationRoutine;

        private void StartSequentialAnimation(List<WinLineAnimationData> animatingDataList)
        {
            if (!_isAnimating)
            {
                _isAnimating = true;
                _animationRoutine=  StartCoroutine(AnimateSequentially(animatingDataList));
            }
        }

        public void StopSequentialAnimation()
        {
            _isAnimating = false;

            if (_animatingDataList.Count > 0)
            {
                foreach (var animationData in _animatingDataList)
                {
                    animationData.topRowAnimator.SetBool(IsPlaying, false);

                }
                _animatingDataList.Clear();
            }

            if (_animationRoutine != null)
            {
                StopCoroutine(_animationRoutine);
                _animationRoutine = null;
            }
        }


        private IEnumerator AnimateSequentially(List<WinLineAnimationData> animatingDataList)
        {
            while (_isAnimating)
            {
                foreach (var data in animatingDataList)
                {
                    if (!_isAnimating) yield break; 

                    data.topRowAnimator.SetBool(IsPlaying, true);
                    UpdateRowUI(data, data.amount);

                    yield return new WaitForSeconds(0.5f);

                    data.topRowAnimator.SetBool(IsPlaying, false);
                }
                if (!_isAnimating) yield break; 

            }

            OnAllAnimationsCompleted?.Invoke();
        }




        private void UpdateRowUI(WinLineAnimationData data, int amount)
        {
            int thousandPlace = (amount / 1000) % 10;
            int hundredPlace = (amount / 100) % 10;
            int tenthPlace = (amount / 10) % 10;
            int onePlace = amount % 10;

            if (data.topThousandPlaceText)
            {
                data.topThousandPlaceText.sprite = digitSpriteMapping.GetSpriteForNumber(thousandPlace);
                data.topThousandPlaceText.enabled = thousandPlace > 0;
            }

            if (data.topHundredthPlaceText)
            {
                data.topHundredthPlaceText.sprite = digitSpriteMapping.GetSpriteForNumber(hundredPlace);
                data.topHundredthPlaceText.enabled = thousandPlace > 0 || hundredPlace > 0;
            }

            if (data.topTenthPlaceText)
            {
                data.topTenthPlaceText.sprite = digitSpriteMapping.GetSpriteForNumber(tenthPlace);
                data.topTenthPlaceText.enabled = thousandPlace > 0 || hundredPlace > 0 || tenthPlace > 0;
            }

            if (data.topOnePlaceText)
            {
                data.topOnePlaceText.sprite = digitSpriteMapping.GetSpriteForNumber(onePlace);
                data.topOnePlaceText.enabled = true;
            }
        }
    }
}