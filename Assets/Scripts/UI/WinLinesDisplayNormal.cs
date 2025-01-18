using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class WinLinesDisplayNormal : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        [SerializeField] private Transform winLineToShow;
        public void OnPointerEnter(PointerEventData eventData)
        {
            winLineToShow.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            winLineToShow.gameObject.SetActive(false);

        }
    }
}
