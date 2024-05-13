using UnityEngine;

namespace TriDimensionalChess.UI {
	public sealed class HorizontalSelectionManager : SelectionManager {
		protected override void Update() {
			base.Update();

			bool right = Input.GetKeyDown(KeyCode.RightArrow);
			if (Input.GetKeyDown(KeyCode.LeftArrow) != right) HandleNextCardSelection(right);
		}
	}
}
