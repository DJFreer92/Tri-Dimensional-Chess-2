using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

[RequireComponent(typeof(Highlight))]
[DisallowMultipleComponent]
public sealed class Square : MonoBehaviour {
	//whether an attackboard can rest on this square
	[field: SerializeField] public bool HasAttackBoardPin {get; private set;}
	//whether the square is white
	[field: SerializeField] public bool IsWhite {get; private set;}
	//x, y, and z coordinates on the board
	[field: SerializeField] public Vector3Int Coords {get; set;}
	//the piece on the square
	[field: SerializeField] public ChessPiece GamePiece {get; set;}
	//whether the square has an attackboard on it
	[field: SerializeField] public bool IsOccupiedByAB {get; set;}
	//the highlight component
	private Highlight _highlight;

	private void Awake() {
		_highlight = GetComponent<Highlight>();
	}

	private void OnMouseUpAsButton() {
		//if the mouse is over a UI element, exit
		if (EventSystem.current.IsPointerOverGameObject()) return;

		//if the game is not active, exit
		if (Game.Instance.State.Is(GameState.INACTIVE)) return;

		//select this square
		Game.Instance.SelectSquare(this);
	}

	///<summary>
	///Clear the square
	///</summary>
	public void Clear() {
		if (HasPiece()) GameObject.Destroy(GamePiece.gameObject);
		IsOccupiedByAB = false;
	}

	///<summary>
	///Returns whether the square has a piece on it
	///</summary>
	///<returns>Whether the square has a piece on it</returns>
	public bool HasPiece() {
		return GamePiece != null;
	}

	///<summary>
	///Returns the board the square is a part of
	///</summary>
	///<returns>The board the square is a part of</returns>
	public Board GetBoard() {
		foreach (Board brd in ChessBoard.Instance) {
			if (brd.Y != Coords.y) continue;
			foreach (Square sqr in brd)
				if (sqr == this) return brd;
		}
		return null;
	}

	///<summary>
	///Returns the annotation of the square in long 3D chess algebraic notation
	///</summary>
	///<returns>The annotation of the square</returns>
	public string GetAnnotation() {
		return Coords.VectorToAnnotation();
	}

	///<summary>
	///Toggle whether the square is highlighted
	///</summary>
	///<param name="toggle">Whether to toggle the highlight on or off</param>
	public void ToggleHighlight(bool toggle) {
		_highlight.ToggleHighlight(toggle);
	}

	///<summary>
	///Returns a copy of the square
	///</summary>
	///<returns>A copy of the square</returns>
	public object Clone() {
		return this.MemberwiseClone();
	}

	///<summary>
	///Returns a string of data about the square
	///</summary>
	///<returns>A string of data about the square</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		str.Append("\nCoordinates: ").Append(Coords.ToString());
		str.Append("\nAnnotation: ").Append(GetAnnotation());
		str.Append("\nPiece:\n").Append(HasPiece() ? GamePiece.ToString() : "null");
		return str.ToString();
	}
}