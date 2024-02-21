using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour {
	[SerializeField] private Canvas canvas;
	
	public void DragHandler(BaseEventData data) {
		var pointerData = data as PointerEventData;

		Vector2 position;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			(RectTransform) canvas.transform,
			pointerData.position,
			canvas.worldCamera,
			out position
		);

		transform.position = canvas.transform.TransformPoint(position);
	}
}