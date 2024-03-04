using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectionManager : MonoBehaviour {
	[SerializeField] protected bool _circularize;
	public SelectionHandler[] Items;
	[HideInInspector] public SelectionHandler LastSelected;

	protected virtual void Update() {
		if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject != LastSelected.gameObject)
			LastSelected = null;
	}

	protected void HandleNextCardSelection(bool toPositive) {
		if (EventSystem.current.currentSelectedGameObject != null || LastSelected == null) return;
		int index = Array.IndexOf(Items, LastSelected) + (toPositive ? 1 : -1);
		if (!_circularize) index = Mathf.Clamp(index, 0, Items.Length - 1);
		else if (index < 0) index = Items.Length - 1;
		else if (index >= Items.Length) index = 0;
		EventSystem.current.SetSelectedGameObject(Items[index].gameObject);
	}
}