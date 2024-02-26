using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour {
	[SerializeField] private bool _isVertical;
	[SerializeField] private bool _circularize;
	public SelectionHandler[] Items;
	public SelectionHandler LastSelected {get; set;}
	public int LastSelectedIndex {get; set;}

	private void Update() {
		if (_isVertical) {
			bool up = Input.GetKeyDown(KeyCode.UpArrow);
			bool down = Input.GetKeyDown(KeyCode.DownArrow);
			if (up != down) HandleNextCardSelection(down);
			return;
		}
		bool left = Input.GetKeyDown(KeyCode.LeftArrow);
		bool right = Input.GetKeyDown(KeyCode.RightArrow);
		if (left != right) HandleNextCardSelection(right);
	}

	private void HandleNextCardSelection(bool toPositive) {
		if (EventSystem.current.currentSelectedGameObject != null || LastSelected == null) return;
		int index = LastSelectedIndex + (toPositive ? 1 : -1);
		if (!_circularize) index = Mathf.Clamp(index, 0, Items.Length - 1);
		else if (index < 0) index = Items.Length - 1;
		else if (index >= Items.Length) index = 0;
		EventSystem.current.SetSelectedGameObject(Items[index].gameObject);
	}
}