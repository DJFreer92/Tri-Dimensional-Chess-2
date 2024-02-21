using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class Page : MonoBehaviour {
	[SerializeField] private GameObject _firstFocusItem;

	///<summary>
	///Activate the page
	///</summary>
	public void Enter() {
		gameObject.SetActive(true);
		if (_firstFocusItem != null) EventSystem.current.SetSelectedGameObject(_firstFocusItem);
	}

	///<summary>
	///Deactivate the page
	///</summary>
	public void Exit() {
		gameObject.SetActive(false);
	}
}