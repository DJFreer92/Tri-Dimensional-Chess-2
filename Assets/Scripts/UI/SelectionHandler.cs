using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class SelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler {
	[Header("References")]
	[SerializeField] private SelectionManager _selectionManager;
	[Header("Constants")]
	[SerializeField] private float _animationTime = 0.1f;
	[Range(0f, 2f), SerializeField] private float _scale = 1.1f;

	private Vector3 _startScale;

	private void Start() {
		_startScale = transform.localScale;
	}

	private IEnumerator MoveElement(bool startingAnimation) {
		Vector3 endScale;

		float elapsedTime = 0f;
		while (elapsedTime < _animationTime) {
			elapsedTime += Time.deltaTime;

			endScale = _startScale;
			if (startingAnimation) endScale *= _scale;

			//lerp the scale
			transform.localScale = Vector3.Lerp(transform.localScale, endScale, (elapsedTime / _animationTime));

			yield return null;
		}
	}

	public void OnPointerEnter(PointerEventData eventData) {
		//select the element
		eventData.selectedObject = gameObject;
	}

	public void OnPointerExit(PointerEventData eventData) {
		//deselect the element if it is the element selected
		if (eventData.selectedObject == gameObject) eventData.selectedObject = null;
	}

	public void OnSelect(BaseEventData eventData) {
		StartCoroutine(MoveElement(true));
		if (_selectionManager == null) return;
		_selectionManager.LastSelected = gameObject;
		for (int i = 0; i < _selectionManager.Items.Length; i++) {
			if (_selectionManager.Items[i] != gameObject) continue;
			_selectionManager.LastSelectedIndex = i;
			return;
		}
	}

	public void OnDeselect(BaseEventData eventData) {
		Deselect();
	}

	public void Deselect() {
		StartCoroutine(MoveElement(false));
	}
}