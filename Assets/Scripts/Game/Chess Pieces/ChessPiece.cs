using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;
using DG.Tweening;

using TriDimensionalChess.Game.Boards;
using TriDimensionalChess.Game.Moves;
using TriDimensionalChess.UI;

namespace TriDimensionalChess.Game.ChessPieces {
	[RequireComponent(typeof(Highlight))]
	public abstract class ChessPiece : MonoBehaviour, IMovable {
		private const float _TWEEN_SPEED = 0.4f;
		private const float _UP_ARC_HEIGHT = 1.5f;
		private const float _DOWN_ARC_HEIGHT = 4f;

		public PieceType Type;

		//the highlight component
		private Highlight _highlight;

		[field: SerializeField] public bool IsWhite {get; private set;}
		public bool HasBeenCaptured {get; private set;}

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
		///Update the piece rights that are lost when the piece moves
		///</summary>
		///<param name="move">The move of the piece</param>
		public abstract void SetMoved(Move move);

		///<summary>
		///Set the piece as white
		///</summary>
		public void SetWhite(bool isWhite) => IsWhite = isWhite;

		///<summary>
		///Sets the state of the piece so that it is consistent with whether it is at its starting position
		///<paramref name="atStart"/>Whether the piece is at its starting position</param>
		///</summary>
		public void SetAtStart(bool atStart) {
			switch (Type) {
				case PieceType.KING: (this as King).HasCastlingRights = atStart;
				return;
				case PieceType.ROOK: (this as Rook).HasCastlingRights = atStart;
				return;
				case PieceType.PAWN: (this as Pawn).HasDSMoveRights = atStart;
				return;
			}
		}

		///<summary>
		///Sets the piece as captured
		///</summary>
		public void SetCaptured() {
			HasBeenCaptured = true;
			_highlight.ToggleSelectedHighlight(false);
			_highlight.ToggleHover(false);
			CapturedPiecesController.Instance.AddPieceOfType(Type, IsWhite);
			gameObject.SetActive(false);
		}

		///<summary>
		///Sets the piece as not captured
		///</summary>
		public void SetUncaptured() {
			gameObject.SetActive(true);
			HasBeenCaptured = false;
			CapturedPiecesController.Instance.RemovePieceOfType(Type, IsWhite);
		}

		///<summary>
		///Returns the square the piece is on
		///</summary>
		///<returns>The square the piece is on</returns>
		public Square GetSquare() => ChessBoard.Instance.GetSquareWithPiece(this);

		///<summary>
		///Returns the player that owns the piece
		///</summary>
		///<returns>The player that owns the piece</returns>
		public Player GetOwner() => Game.Instance.GetPlayer(IsWhite);

		///<summary>
		///Returns whether the given piece is of the same color
		///</summary>
		///<param name="piece">Piece to compare colors with</param>
		///<returns>Whether the pieces are of the same color</returns>
		public bool IsSameColor(ChessPiece piece) => IsWhite == piece.IsWhite;

		///<summary>
		///Returns whether the piece belongs to the given player
		///</summary>
		///<param name="player">The player to check if the piece belongs to</param>
		///<returns>Whether the piece belongs to the given player</returns>
		public bool BelongsTo(Player player) => IsWhite == player.IsWhite;

		///<summary>
		///Move the position of the piece to the given square
		///</summary>
		///<param name="sqr">The square to move the piece to</param>
		public void MoveTo(Square sqr) {
			if (!Game.Instance.AllowMoves) {
				MoveInstant(sqr);
				return;
			}

			if (!sqr.HeightMatch(GetSquare())) {
				MoveInArc(sqr);
				return;
			}

			if (this is Knight) {
				if ((this as Knight).IsPathClear(sqr, true)) MoveInPath(sqr, true);
				else if ((this as Knight).IsPathClear(sqr, false)) MoveInPath(sqr, false);
				else MoveInArc(sqr);
				return;
			}

			MoveInLine(sqr);
		}

		///<summary>
		///Moves the piece instantly to the given square
		///</summary>
		///<param name="sqr">The square to move the piece to</param>
		private void MoveInstant(Square sqr) => transform.position = sqr.transform.position;

		///<summary>
		///Moves the piece in a straight line to the given square
		///</summary>
		///<param name="sqr">The square to move the piece to</param>
		private void MoveInLine(Square sqr) => transform.DOMove(sqr.transform.position, _TWEEN_SPEED);

		///<summary>
		///Moves the piece in an arc to the given square
		///</summary>
		///<param name="sqr">The square to move the piece to</param>
		private void MoveInArc(Square sqr) {
			float height = sqr.BrdHeight >= GetSquare().BrdHeight ? _UP_ARC_HEIGHT : _DOWN_ARC_HEIGHT;
			transform.DOJump(sqr.transform.position, height, 1, _TWEEN_SPEED);
		}

		///<summary>
		///Moves the piece in a path to the given square
		///</summary>
		///<param name="sqr">The square to move the piece to</param>
		///<param name="straightFirst">Whether to move straight first</param>
		private void MoveInPath(Square sqr, bool straightFirst) {
			Vector3 waypoint = transform.position;
			if (straightFirst) waypoint.z = sqr.transform.position.z;
			else waypoint.x = sqr.transform.position.x;
			transform.DOPath(new Vector3[] {waypoint, sqr.transform.position}, _TWEEN_SPEED);
		}

		///<summary>
		///Unpromotes a piece, converts it back into a pawn
		///</summary>
		///<returns>The pawn the piece was converted to</returns>
		public Pawn Unpromote() {
			CapturedPiecesController.Instance.RemovePieceOfType(PieceType.PAWN, IsWhite);
			CapturedPiecesController.Instance.AddPieceOfType(Type, !IsWhite);

			return PieceCreator.Instance.ConvertPiece(this, PieceType.PAWN) as Pawn;
		}

		///<summary>
		///Toggle whether the square is highlighted
		///</summary>
		///<param name="toggle">Whether to toggle the highlight on or off</param>
		public void ToggleHighlight(bool toggle) => _highlight.ToggleSelectedHighlight(toggle);

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
}
