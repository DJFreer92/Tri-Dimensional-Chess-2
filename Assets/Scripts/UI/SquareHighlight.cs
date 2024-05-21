using UnityEngine;

namespace TriDimensionalChess.UI {
	[DisallowMultipleComponent]
	public sealed class SquareHighlight : Highlight {
		[SerializeField] private Color _captureColor = Color.red;
		[SerializeField] private Color _availableColor = Color.green;
		private bool _available = false;

        protected override void OnMouseEnter() {
			if (!_available) base.OnMouseEnter();
        }

        protected override void OnMouseExit() {
			if (!_available) base.OnMouseExit();
		}

		///<summary>
		///Toggle whether the available highlight is enabled
		///</summary>
		///<param name="toggle">Whether to enable the available highlight</param>
		public void ToggleAvailableHighlight(bool toggle) {
			if (_available == toggle) return;

			if (toggle) SetHighlight(_availableColor);
			else ClearHighlight();

			_available = toggle;
		}

		///<summary>
		///Toggle whether the capture highlight is enabled
		///</summary>
		///<param name="toggle">Whether to enable the capture highlight</param>
		public void ToggleCaptureHighlight(bool toggle) {
			if (_available == toggle) return;

			if (toggle) SetHighlight(_captureColor);
			else ClearHighlight();

			_available = toggle;
		}
	}
}
