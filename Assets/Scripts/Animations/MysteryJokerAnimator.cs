using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animations
{
    public class MysteryJokerAnimator : MonoBehaviour
    {
        [SerializeField] private List<string> animationStates = new List<string>();
        [SerializeField] private Animator mysteryJokerAnimator;

        private void Start()
        {
            StartCoroutine(StartMysteryJokerAnimations());
        }

        private IEnumerator StartMysteryJokerAnimations()
        {
            while (true)
            {
                int index = Random.Range(0, animationStates.Count);
                string chosenState = animationStates[index];

                mysteryJokerAnimator.Play(chosenState);

                yield return null;

                AnimatorStateInfo stateInfo = mysteryJokerAnimator.GetCurrentAnimatorStateInfo(0);
                float animationLength = stateInfo.length;

                float randomTimeMultiplier = Random.Range(1f, 4f);
                float timeToPlay = animationLength * randomTimeMultiplier;

                yield return new WaitForSeconds(timeToPlay);
            }
        }
    }
}