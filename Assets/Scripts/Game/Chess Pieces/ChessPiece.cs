using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

[RequireComponent(typeof(Highlight))]
[DisallowMultipleComponent]
public abstract class ChessPiece : MonoBehaviour, IMovable {
	[field: SerializeField] public bool IsWhite {get; private set;}
	public PieceType Type {get; private set;}
	public bool HasBeenCaptured {get; private set;}
	//the highlight component
	private Highlight _highlight;

	private void Awake() {
		//set the piece type
		if (this is King) Type = PieceType.KING;
		else if (this is Queen) Type = PieceType.QUEEN;
		else if (this is Rook) Type = PieceType.ROOK;
		else if (this is Bishop) Type = PieceType.BISHOP;
		else if (this is Knight) Type = PieceType.KNIGHT;
		else if (this is Pawn) Type = PieceType.PAWN;

		//get the highlight component
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

		//if testing mode is not enabled, exit
		if (!SettingsManager.Instance.TestMode) return;

		//if the right mouse button has not been released, exit
		if (!Input.GetMouseButtonUp(1)) return;

		//if the mouse is over a UI element, exit
		if (EventSystem.current.IsPointerOverGameObject()) return;

		//if the piece has been captured, exit
		if (HasBeenCaptured) return;

		//set the piece as captured and destroy the gameobject
		SetCaptured();
		GetSquare().GamePiece = null;
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
	///Set the piece as having not been captured
	///</summary>
	public void SetUncaptured() {
		HasBeenCaptured = false;
		CapturedPiecesController.Instance.RemovePiece(this);
	}

	///<summary>
	///Returns the square the piece is on
	///</summary>
	///<returns>The square the piece is on</returns>
	public Square GetSquare() {
		return ChessBoard.Instance.GetSquareWithPiece(this);
	}

	///<summary>
	///Returns the prefab of the given piece and color
	///</summary>
	///<returns>Prefab of the given piece and color</returns>
	public static GameObject GetPrefab(PieceTypeColor ptc) {
		switch (ptc) {
			case PieceTypeColor.WHITE_KING: return King.WhitePrefab;
			case PieceTypeColor.BLACK_KING: return King.BlackPrefab;
			case PieceTypeColor.WHITE_QUEEN: return Queen.WhitePrefab;
			case PieceTypeColor.BLACK_QUEEN: return Queen.BlackPrefab;
			case PieceTypeColor.WHITE_ROOK: return Rook.WhitePrefab;
			case PieceTypeColor.BLACK_ROOK: return Rook.BlackPrefab;
			case PieceTypeColor.WHITE_BISHOP: return Bishop.WhitePrefab;
			case PieceTypeColor.BLACK_BISHOP: return Bishop.BlackPrefab;
			case PieceTypeColor.WHITE_KNIGHT: return Knight.WhitePrefab;
			case PieceTypeColor.BLACK_KNIGHT: return Knight.BlackPrefab;
			case PieceTypeColor.WHITE_PAWN: return Pawn.WhitePrefab;
			case PieceTypeColor.BLACK_PAWN: return Pawn.BlackPrefab;
		}
		return null;
	}

	///<summary>
	///Move the position of the piece to the given square
	///</summary>
	///<param name="sqr">The square to move the piece to</param>
	public void MoveTo(Square sqr) {
		transform.position = sqr.transform.position;
	}

	///<summary>
	///Converts the piece to a different piece
	///</summary>
	///<param name="type">The type of piece to convert to</param>
	public ChessPiece ConvertTo(PieceType type) {
		//if the desired type is the same as this piece's type, return this piece
		if (Type == type) return this;

		Square sqr = GetSquare();
		GameObject newGameObj = CreateGameObject(ChessPiece.GetPrefab(type.GetPieceTypeColor(IsWhite)));
		ChessPiece newPiece = newGameObj.GetComponent<ChessPiece>();
		switch (type) {
			case PieceType.ROOK: (newPiece as Rook).HasCastlingRights = false;
			break;
			case PieceType.BISHOP: (newPiece as Bishop).SquareColorIsWhite = GetSquare().IsWhite;
			break;
			case PieceType.PAWN: (newPiece as Pawn).HasDSMoveRights = false;
			break;
		}
		sqr.GamePiece = newPiece;

		return newPiece;

		//inner functions
		//create a new gameobject
		GameObject CreateGameObject(GameObject prefab) {
			//instaniate the gameobject of the new piece
			GameObject newGameObj = GameObject.Instantiate(
				prefab,  //gameobject prefab
				sqr.transform.position,  //vector3 position
				transform.rotation,  //quarternion rotation
				IsWhite ? ChessBoard.Instance.whitePiecesParent : ChessBoard.Instance.blackPieceParent  //parent transform
			);

			//enable the mesh collider
			newGameObj.GetComponent<MeshCollider>().enabled = true;

			//return the new gameobject
			return newGameObj;
		}
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