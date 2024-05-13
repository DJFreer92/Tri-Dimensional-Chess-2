using UnityEngine;
using UnityEngine.EventSystems;

namespace TriDimensionalChess.UI {
	[DisallowMultipleComponent]
	public sealed class Page : MonoBehaviour {
		[SerializeField] private GameObject _firstFocusItem;
		[SerializeField] private GameObject _popFocusItem;

		public void SetFirstFocusItem(GameObject item) => _firstFocusItem = item;

		///<summary>
		///Activate the page
		///</summary>
		public void Enter() {
			gameObject.SetActive(true);
			Focus(_firstFocusItem);
		}

		///<summary>
		///Deactivate the page
		///</summary>
		public void Exit() => gameObject.SetActive(false);

		///<summary>
		///Focuses the pop item
		///</summary>
		public void FocusPopItem() => Focus(_popFocusItem);

		///<summary>
		///Focuses the given item
		///</summary>
		///<param name="item">The item to focus</param>
		private void Focus(GameObject item) {
			if (item != null) EventSystem.current.SetSelectedGameObject(item);
		}
	}
}
