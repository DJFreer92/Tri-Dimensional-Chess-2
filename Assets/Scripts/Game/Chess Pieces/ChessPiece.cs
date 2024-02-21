using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

[RequireComponent(typeof(Highlight))]
[DisallowMultipleComponent]
public abstract class ChessPiece : MonoBehaviour, IMovable {
	[field: SerializeField] public bool IsWhite {get; private set;}
	public bool HasBeenCaptured {get; private set;}
	//the highlight component
	private Highlight _highlight;

	private void Awake() {
		_highlight = GetComponent<Highlight>();
	}

	private void OnEnable() {
		//register to events
		RegisterToEvents();
	}

	private void OnDisable() {
		//unregister from events
		UnregisterToEvents();
	}

	private void OnMouseUpAsButton() {
		//if the mouse is over a UI element, exit
		if (EventSystem.current.IsPointerOverGameObject()) return;

		//if the game is not active, exit
		if (Game.Instance.State.Is(GameState.INACTIVE)) return;

		//if the piece has been captured, exit
		if (HasBeenCaptured) return;

		//select the square the piece is on
		Game.Instance.SelectPiece(this);
	}

	private void OnMouseOver() {
		//if the game is not active, exit
		if (Game.Instance.State.Is(GameState.INACTIVE)) return;

		//if the right mouse button has not been released, exit
		if (!Input.GetMouseButtonUp(1)) return;

		//if the mouse is over a UI element, exit
		if (EventSystem.current.IsPointerOverGameObject()) return;

		//if the piece has been captured, exit
		if (HasBeenCaptured) return;

		//set the piece as captured and destroy the gameobject
		SetCaptured();
		ChessBoard.Instance.GetSquareWithPiece(this).GamePiece = null;
	}

	///<summary>
	///Adds local methods to listeners
	///</summary>
	protected virtual void RegisterToEvents() {}

	///<summary>
	///Removes local methods from listeners
	///</summary>
	protected virtual void UnregisterToEvents() {}

	///<summary>
	///Returns a list of all the piece's available moves
	///</summary>
	///<param name="asWhite">Whether the piece is being moved by white</param>
	///<returns>A list of all the moves available to the piece</returns>
	public abstract List<Square> GetAvailableMoves(bool asWhite);

	///<summary>
	///Returns the notation character of the piece
	///</summary>
	///<param name="wantFigurine">Whether the figurine character is desired</param>
	///<returns>The notation character of the piece</returns>
	public abstract string GetCharacter(bool wantFigurine);

	///<summary>
	///Set the piece as white
	///</summary>
	public void SetWhite(bool isWhite) {
		IsWhite = isWhite;
	}

	///<summary>
	///Set whether the piece had been captured
	///</summary>
	public void SetCaptured() {
		HasBeenCaptured = true;
		_highlight.ToggleHighlight(false);
		_highlight.ToggleHover(false);
		Game.Instance.GetComponent<CapturedPiecesController>().AddPiece(this);
	}

	///<summary>
	///Returns the square the piece is on
	///</summary>
	///<returns>The square the piece is on</returns>
	public Square GetSquare() {
		return ChessBoard.Instance.GetSquareWithPiece(this);
	}

	///<summary>
	///Move the position of the piece to the given square
	///</summary>
	///<param name="sqr">The square to move the piece to</param>
	public void MoveTo(Square sqr) {
		transform.position = sqr.transform.position;
	}

	///<summary>
	///Toggle whether the square is highlighted
	///</summary>
	///<param name="toggle">Whether to toggle the highlight on or off</param>
	public void ToggleHighlight(bool toggle) {
		_highlight.ToggleHighlight(toggle);
	}

	///<summary>
	///Returns a string of data about the object
	///</summary>
	///<returns>A string of data about the object</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		str.Append("\nOwner: ").Append(IsWhite ? "White" : "Black");
		str.Append("\nHas Been Captured? ").Append(HasBeenCaptured);
		return str.ToString();
	}
}

public interface ICastlingRights {
	bool HasCastlingRights {get;}

	///<summary>
	///Revokes the pieces castling rights
	///</summary>
	void RevokeCastlingRights();
}