using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TriDimensionalChess.UI {
	[DisallowMultipleComponent]
	public sealed class SelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler {
		#region References
		[Header("References")]
		[SerializeField] private VerticalSelectionManager _verticalSelectionManager;
		[SerializeField] private HorizontalSelectionManager _horizontalSelectionManager;
		#endregion

		#region Constants
		[Header("Constants")]
		[SerializeField] private float _animationTime = 0.1f;
		[Range(0f, 2f), SerializeField] private float _scale = 1.1f;
		[SerializeField] private bool _deselectOnPointerExit = true;
		#endregion

		private Selectable _selectableComponent;
		private Vector3 _startScale;

		private void Start() {
			_selectableComponent = GetComponent<Selectable>();
			_startScale = transform.localScale;
		}

		///<summary>
		///Animates the element
		///</summary>
		///<param name="startingAnimation">Whether the animation is the starting or ending animation</param>
		private IEnumerator AnimateElement(bool startingAnimation) {
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
			if (_selectableComponent.interactable) eventData.selectedObject = gameObject;
		}

		public void OnPointerExit(PointerEventData eventData) {
			if (!_deselectOnPointerExit) return;
			//deselect the element if it is the element selected
			if (eventData.selectedObject == gameObject) eventData.selectedObject = null;
		}

		public void OnSelect(BaseEventData eventData) {
			StartCoroutine(AnimateElement(true));
			if (_verticalSelectionManager != null) _verticalSelectionManager.LastSelected = this;
			if (_horizontalSelectionManager != null) _horizontalSelectionManager.LastSelected = this;
		}

		public void OnDeselect(BaseEventData eventData) {
			Deselect();
		}

		///<summary>
		///Deselects the element
		///</summary>
		public void Deselect() {
			StartCoroutine(AnimateElement(false));
		}
	}
}
