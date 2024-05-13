using UnityEngine;
using UnityEngine.EventSystems;

namespace TriDimensionalChess.UI {
	public class Drag : MonoBehaviour {
		[SerializeField] private Canvas canvas;

		public void DragHandler(BaseEventData data) {
			var pointerData = data as PointerEventData;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                pointerData.position,
                canvas.worldCamera,
                out Vector2 position
            );

            transform.position = canvas.transform.TransformPoint(position);
		}
	}
}
