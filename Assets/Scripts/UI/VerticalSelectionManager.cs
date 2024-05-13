using UnityEngine;

public sealed class VerticalSelectionManager : SelectionManager {
	protected override void Update() {
		base.Update();

		bool down = Input.GetKeyDown(KeyCode.DownArrow);
		if (Input.GetKeyDown(KeyCode.UpArrow) != down) base.HandleNextCardSelection(down);
	}
}