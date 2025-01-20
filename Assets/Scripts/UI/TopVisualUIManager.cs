using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TopVisualUIManager : MonoBehaviour
    {
        private static TopVisualUIManager _instance;

        [SerializeField] private TMP_Text visualText;


        public static TopVisualUIManager GetInstance()
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

            SetNormalTexts("");
        }


        public void SetNormalTexts(string text)
        {
            visualText.text = text;
        }
    }
}