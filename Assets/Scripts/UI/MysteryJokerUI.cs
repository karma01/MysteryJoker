using System.Collections;
using DG.Tweening;
using Mechanics.Amount;
using Mechanics.Rules;
using Mechanics.Win;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class MysteryJokerUI : MonoBehaviour
    {
        [SerializeField] private Transform freeSpinsPanel;
        [SerializeField] private StartButtonBehaviour startButtonBehaviour;
        [SerializeField] private Transform mysteryWinPanel;
        [SerializeField] private Animator mysteryWinAnimator;

        [SerializeField] private DigitMapper digitSpriteMapping;

        [Header("Animating images")] [SerializeField]
        private Image onePlaceText;

        [SerializeField] private Image tenthPlaceText;
        [SerializeField] private Image hundredthPlaceText;
        [SerializeField] private Image thousandPlaceText;

        [Header(" Background for mystery joker occurance")] [SerializeField]
        private Sprite mysteryJokerBg;

        [SerializeField] private Sprite mysteryJokerSideOne;
        [SerializeField] private Sprite mysteryJokerSideTwo;

        [SerializeField] private Sprite mysteryJokerRedDisplayUI;


        [Header(" Background for normal spins")] [SerializeField]
        private Sprite normalBg;

        [SerializeField] private Sprite normalSideOne;
        [SerializeField] private Sprite normalSideTwo;

        [SerializeField] private Sprite normalDispayUI;

        [Header("Changing images")] [SerializeField]
        private Image background;

        [SerializeField] private Image sideOneBg;
        [SerializeField] private Image sideTwoBg;
        [SerializeField] private Image situationDisplayImage;

        private void Awake()
        {
            startButtonBehaviour.OnStartButtonClicked += DisableWonSpinsPanel;
        }


        public void ShowJokerAnimation(PaylineWinInfo winInfo)

        {
            _jokerRoutine ??= StartCoroutine(StartMysteryJokerAnimation(winInfo));

            _panelAnimationRoutine ??= StartCoroutine(ShowWonSpinsPanel());
        }


        private Coroutine _jokerRoutine;
        private Coroutine _panelAnimationRoutine;
        private static readonly int IsPlaying = Animator.StringToHash("isPlaying");


        private IEnumerator StartMysteryJokerAnimation(PaylineWinInfo winInfo)
        {
            {
                foreach (var mysterySlotBox in winInfo.slotBoxes)
                {
                    mysterySlotBox.RunMysteryBoxAnimation();
                }

                yield return null;
            }
        }

        private IEnumerator ShowWonSpinsPanel()
        {
            yield return new WaitForSeconds(3f);

            freeSpinsPanel.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
            ChangeToMysteryJokerUiTheme();
        }

        private void DisableWonSpinsPanel()
        {
            if (_panelAnimationRoutine != null)
            {
                StopCoroutine(_panelAnimationRoutine);
                _panelAnimationRoutine = null;
            }

            freeSpinsPanel.DOScale(Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
        }

        public void ShowMysteryWinUI(int amount)
        {
            mysteryWinPanel.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                mysteryWinAnimator.gameObject.SetActive(true);
                mysteryWinAnimator.SetBool(IsPlaying, true);

                DOVirtual.DelayedCall(3f, () => { AmountHandler.GetInstance().CalculateTotalWin(amount); });
                mysteryWinPanel.DOScale(Vector3.zero, 0.1f).SetDelay(5f).OnComplete(() =>
                {
                    mysteryWinAnimator.SetBool(IsPlaying, false);
                    mysteryWinAnimator.gameObject.SetActive(false);
                });
            });
            UpdateRowUI(amount);
        }

        private void UpdateRowUI(int amount)
        {
            int thousandPlace = (amount / 1000) % 10;
            int hundredPlace = (amount / 100) % 10;
            int tenthPlace = (amount / 10) % 10;
            int onePlace = amount % 10;

            if (thousandPlaceText)
            {
                thousandPlaceText.sprite = digitSpriteMapping.GetSpriteForNumber(thousandPlace);
                thousandPlaceText.enabled = thousandPlace > 0;
            }

            if (hundredthPlaceText)
            {
                hundredthPlaceText.sprite = digitSpriteMapping.GetSpriteForNumber(hundredPlace);
                hundredthPlaceText.enabled = thousandPlace > 0 || hundredPlace > 0;
            }

            if (tenthPlaceText)
            {
                tenthPlaceText.sprite = digitSpriteMapping.GetSpriteForNumber(tenthPlace);
                tenthPlaceText.enabled = thousandPlace > 0 || hundredPlace > 0 || tenthPlace > 0;
            }

            if (onePlaceText)
            {
                onePlaceText.sprite = digitSpriteMapping.GetSpriteForNumber(onePlace);
                onePlaceText.enabled = true;
            }
        }

        private void ChangeToMysteryJokerUiTheme()
        {
            background.sprite = mysteryJokerBg;
            sideOneBg.sprite = mysteryJokerSideOne;
            sideTwoBg.sprite = mysteryJokerSideTwo;
            situationDisplayImage.sprite = mysteryJokerRedDisplayUI;
        }

        public void ChangeToNormalTheme()
        {
            background.sprite = normalBg;
            sideOneBg.sprite = normalSideOne;
            sideTwoBg.sprite = normalSideTwo;
            situationDisplayImage.sprite = normalDispayUI;
        }
    }
}