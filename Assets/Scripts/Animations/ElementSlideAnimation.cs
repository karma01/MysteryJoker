using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UI;
using UnityEngine;

namespace Animations
{
    public class ElementSlideAnimation : MonoBehaviour
    {
        public event Action<bool> OnSlotChangeState; // true for moving and false when it stops

        [Header("Animation Elements")] [SerializeField]
        private List<Transform> leftElements = new List<Transform>();

        [SerializeField] private List<Transform> midElements = new List<Transform>();
        [SerializeField] private List<Transform> rightElements = new List<Transform>();

        [Header("Animation Container")] [SerializeField]
        private Transform leftElementContainer;

        [SerializeField] private Transform midElementContainer;
        [SerializeField] private Transform rightElementContainer;
        [Header("Positions")] [SerializeField] private Transform finalPos; // Bottom reset position
        [SerializeField] private Transform initialPosElement; // Top reset position
        [SerializeField] private Transform animationPositionContainer;

        [Header("Control Values")] [SerializeField]
        private float animationSpeed = 5f; // Normal movement speed

        [SerializeField] private float leftContainerDecelSpeed;
        [SerializeField] private float midContainerDecelSpeed;
        [SerializeField] private float rightContainerDecelSpeed;
        [SerializeField] private float standardStopTime = 2f;

        [SerializeField] private Ease containerEase;
        private Coroutine _movementRoutine;

        private float[] _initialLeftPositions;
        private float[] _initialMidPositions;
        private float[] _initialRightPositions;

        private Vector3 _initialLeftContainerPosition;

        private bool _isLeftMoving;
        private bool _isMedMoving;
        private bool _isRightMoving;

        private bool _isSlotSpinning;


        private void Start()
        {
            _initialLeftContainerPosition = leftElementContainer.localPosition;


            _initialLeftPositions = SaveInitialPositionsY(leftElements);
            _initialMidPositions = SaveInitialPositionsY(midElements);
            _initialRightPositions = SaveInitialPositionsY(rightElements);
        }

        private Coroutine _stopSpinRoutine;

        [Button]
        public void InitiateMovement()
        {
            if (_movementRoutine != null) return;
            
            //Show top text
            
            
            _movementRoutine = StartCoroutine(InitiateMovementRoutine());

            _isSlotSpinning = true;
            _stopSpinRoutine = StartCoroutine(StopMovementRoutine());

            OnSlotChangeState?.Invoke(true);
        }

        public bool IsSlotSpinning()
        {
            return _isSlotSpinning;
        }

        private IEnumerator StopMovementRoutine()
        {
            yield return new WaitForSeconds(standardStopTime);

            if (_isSlotSpinning)
            {
                StopMovement();
            }
        }


        [Button]
        public void StopMovement()
        {
            if (_stopSpinRoutine != null)
            {
                StopCoroutine(_stopSpinRoutine);
                _stopSpinRoutine = null;
            }


            DOVirtual.DelayedCall(leftContainerDecelSpeed, () =>
            {
                StartCoroutine(ResetElementsPosition(leftElements, _initialLeftPositions, leftElementContainer,
                    _initialLeftContainerPosition));
                _isLeftMoving = false;
            });

            DOVirtual.DelayedCall(midContainerDecelSpeed, () =>
            {
                StartCoroutine(ResetElementsPosition(midElements, _initialMidPositions, midElementContainer,
                    _initialLeftContainerPosition));

                _isMedMoving = false;
            });

            DOVirtual.DelayedCall(rightContainerDecelSpeed, () =>
            {
                StartCoroutine(ResetElementsPosition(rightElements, _initialRightPositions, rightElementContainer,
                    _initialLeftContainerPosition));
                _isRightMoving = false;

                if (_movementRoutine != null)
                {
                    StopCoroutine(_movementRoutine);
                    _movementRoutine = null;
                }

                _isSlotSpinning = false;
                TopVisualUIManager.GetInstance().SetNormalTexts("PRESS SPIN TO BEGIN" );

                OnSlotChangeState?.Invoke(false);
                //Show text
            });
        }


        private IEnumerator InitiateMovementRoutine()
        {
            leftElementContainer.DOMoveY(animationPositionContainer.position.y, 0.2f).SetEase(Ease.OutQuad);
            rightElementContainer.DOMoveY(animationPositionContainer.position.y, 0.2f).SetEase(Ease.OutQuad);

            midElementContainer.DOMoveY(animationPositionContainer.position.y, 0.2f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(0.3f);

            _isLeftMoving = true;
            _isMedMoving = true;
            _isRightMoving = true;


            while (_isLeftMoving || _isMedMoving || _isRightMoving)
            {
                MoveElements(leftElements, animationSpeed, ref _isLeftMoving);
                MoveElements(midElements, animationSpeed, ref _isMedMoving);
                MoveElements(rightElements, animationSpeed, ref _isRightMoving);

                yield return null;
            }
        }


        private IEnumerator ResetElementsPosition(List<Transform> elements, float[] initialPositions,
            Transform container, Vector3 containerInitialPos)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].localPosition = new Vector3(elements[i].localPosition.x, initialPositions[i],
                    elements[i].localPosition.z);
            }

            yield return null;

            BringContainerDown(container, containerInitialPos);
        }

        private void BringContainerDown(Transform container, Vector3 initialPos)
        {
            container.transform.DOLocalMoveY(initialPos.y, leftContainerDecelSpeed).SetEase(containerEase);
        }

        private void MoveElements(List<Transform> elements, float speed, ref bool isMoving)
        {
            if (!isMoving)
            {
                return;
            }

            foreach (var element in elements)
            {
                element.Translate(Vector3.down * (speed * Time.deltaTime));


                if (element.position.y <= finalPos.position.y)
                {
                    element.position = new Vector3(
                        element.position.x,
                        initialPosElement.position.y,
                        element.position.z
                    );
                }
            }
        }


        private float[] SaveInitialPositionsY(List<Transform> elements)
        {
            float[] positions = new float[elements.Count];
            for (int i = 0; i < elements.Count; i++)
            {
                positions[i] = elements[i].localPosition.y;
            }

            return positions;
        }
    }
}