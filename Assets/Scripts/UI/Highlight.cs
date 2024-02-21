using System.Collections.Generic;
using UnityEngine;

public sealed class Highlight : MonoBehaviour {
	[SerializeField] private Color _selectColor = Color.white, _hoverColor = Color.clear;
	private readonly Color _CLEAR = Color.clear;
	//helper list to cache all the materials of this object
	private List<Material> _materials = new();
	private bool _selected = false, _hoverEnabled = true;

	private void Awake() {
		//Gets all the materials from each renderer
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) {
			_materials.AddRange(renderer.materials);
		}
		foreach (Material material in _materials) {
			//enable the EMISSION
			material.EnableKeyword("_EMISSION");
		}
	}

	private void OnMouseEnter() {
		if (!_hoverEnabled || _selected) return;
		SetHighlight(_hoverColor);
	}

	private void OnMouseExit() {
		if (_selected) return;
		ClearHighlight();
	}

	public void ToggleHighlight(bool highlight) {
		if (_selected == highlight) return;

		if (highlight) SetHighlight(_selectColor);
		else ClearHighlight();

		_selected = highlight;
	}

	public void ToggleHover(bool toggle) {
		_hoverEnabled = toggle;
	}

	private void SetHighlight(Color color) {
		foreach (Material material in _materials) {
			//set the color
			material.SetColor("_EmissionColor", color);
		}
	}

	private void ClearHighlight() {
		SetHighlight(_CLEAR);
	}
}