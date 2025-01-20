using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class ToggleButtonInteraction : MonoBehaviour
    {
        [SerializeField] private List<Button> changeableButtons = new List<Button>();


        public void ToggleButtonInteractions(bool areInteractable)
        {
            changeableButtons.ForEach(obj => { obj.interactable = areInteractable; });
        }
        
    }
}