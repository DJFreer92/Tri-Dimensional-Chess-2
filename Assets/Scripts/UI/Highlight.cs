using System.Collections.Generic;
using UnityEngine;

namespace TriDimensionalChess.UI {
	[DisallowMultipleComponent]
	public class Highlight : MonoBehaviour {
		protected bool _selected = false, _hoverEnabled = true;
		
		private static readonly Color _CLEAR = Color.clear;

		//helper list to cache all the materials of this object
		private readonly List<Material> _materials = new();

		[SerializeField] private Color _selectColor = Color.white;
		[SerializeField] private Color _hoverColor = Color.clear;

		private void Awake() {
			//gets all the materials from each renderer
			foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) _materials.AddRange(renderer.materials);

			//enable the EMISSION for each material
			foreach (Material material in _materials) material.EnableKeyword("_EMISSION");
		}

		protected virtual void OnMouseEnter() {
			if (!_hoverEnabled || _selected) return;
			SetHighlight(_hoverColor);
		}

		protected virtual void OnMouseExit() {
			if (_selected) return;
			ClearHighlight();
		}

		///<summary>
		///Toggle whether the selected highlight is enabled
		///</summary>
		///<param name="toggle">Whether to enable the selected highlight</param>
		public void ToggleSelectedHighlight(bool toggle) {
			if (_selected == toggle) return;

			if (toggle) SetHighlight(_selectColor);
			else ClearHighlight();

			_selected = toggle;
		}

		///<summary>
		///Toggle whether the hover is enabled
		///</summary>
		///<param name="toggle">Whether to enable the hover</param>
		public void ToggleHover(bool toggle) => _hoverEnabled = toggle;

		///<summary>
		///Set the highlight of the gameobject
		///</summary>
		///<param name="color">The color to highlight the gameobject</param>
		protected void SetHighlight(Color color) {
			//set the color for each material
			foreach (Material material in _materials) material.SetColor("_EmissionColor", color);
		}

		///<summary>
		///Clear the highlight of the gameobject
		///</summary>
		protected void ClearHighlight() => SetHighlight(_CLEAR);
	}
}
