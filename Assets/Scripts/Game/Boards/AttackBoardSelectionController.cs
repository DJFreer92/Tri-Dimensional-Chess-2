using UnityEngine;

namespace TriDimensionalChess.Game.Boards {
	[DisallowMultipleComponent]
	public sealed class AttackBoardSelectionController : MonoBehaviour {
		private AttackBoard _attackBoard;

		private void Awake() => _attackBoard = GetComponentInParent<AttackBoard>();

		private void OnMouseOver() {
			if (Input.GetMouseButtonDown(0)) _attackBoard.Select();
		}
	}
}
