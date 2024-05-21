using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using DG.Tweening;

using TriDimensionalChess.Game.ChessPieces;
using TriDimensionalChess.Game.Moves;

namespace TriDimensionalChess.Game.Boards {
	[DisallowMultipleComponent]
	public sealed class AttackBoard : Board, IMovable {
		//tweening constants
		private const float _TWEEN_SPEED = 0.4f;
		private const float _UP_ARC_HEIGHT = 1.5f;
		private const float _DOWN_ARC_HEIGHT = 4f;

		//whether the attack board is inverted
		[HideInInspector] public bool IsInverted = false;

		//the attack board support pillar
		[SerializeField] private GameObject _supportPillar;

		//the square the attackboard is pinned to
		[field: SerializeField] public PinSquare PinnedSquare {get; private set;}

		///<summary>
		///Initialize the attack board
		///</summary>
		///<param name="pinSquare">The square the attack board is pinned to</param>
		public void Init(PinSquare pinSquare) {
			PinnedSquare = pinSquare;
			Init();
		}

        ///<summary>
        ///Clears the attackboard
        ///</summary>
        public override void Clear() {
			base.Clear();
			Destroy(gameObject);
		}

		///<summary>
		///Sets the square the attackboard is pinned to
		///</summary>
		///<param name="pin">The square the attackboard is pinned to</param>
		public void SetPinnedSquare(PinSquare pin) => PinnedSquare = pin;

		///<summary>
		///Returns a list of all the attackboard's available moves
		///</summary>
		///<param name="asWhite">Whether the attackboard is being moved by white</param>
		///<returns>A list of all the attackboard's available moves</returns>
		public List<Square> GetAvailableMoves(bool asWhite) {
			var moves = new List<Square>();
			if (!Owner.MatchesPlayerBool(asWhite)) return moves;
			List<ChessPiece> pieces = GetPieces();
			if (pieces.Count != 1) return moves;
			if (pieces[0].IsWhite != asWhite) return moves;
			foreach (Board brd in ChessBoard.Instance.MainBoards) {
				if (Math.Abs(brd.Y - PinnedSquare.BrdHeight) > 2) continue;
				foreach (PinSquare sqr in brd.PinSquares) {
					if (IsInverted ? sqr.IsBottomPinOccupied() : sqr.IsTopPinOccupied()) continue;
					int xDiff = sqr.FileIndex - PinnedSquare.FileIndex;
					int zDiff = sqr.Rank - PinnedSquare.Rank;
					if (pieces[0] is Pawn && Math.Sign(zDiff) == (asWhite ? -1 : 1)) continue;
					if (xDiff != 0 && zDiff != 0) continue;
					if (Math.Abs(zDiff) > 4) continue;
					if (!King.WillBeInCheck(new AttackBoardMove(Game.Instance.GetPlayer(asWhite), Squares[0], sqr))) moves.Add(sqr);
				}
			}
			return moves;
		}

		///<summary>
		///Returns whether an attack board rotation would be a legal move
		///</summary>
		///<param name="asWhite">Whether the move is being made by white</param>
		///<returns>Whether an attack board rotation would be a legal move</returns>
		public bool CanRotate(bool asWhite) {
			if (GetPieceCount() != 1 || GetSinglePiece().IsWhite != asWhite) return false;
			if (Owner == Ownership.NEUTRAL) return false;

			Square[] sortedSqrs = GetSortedSquares();
			(sortedSqrs[0].GamePiece, sortedSqrs[1].GamePiece, sortedSqrs[2].GamePiece, sortedSqrs[3].GamePiece) =
				(sortedSqrs[3].GamePiece, sortedSqrs[2].GamePiece, sortedSqrs[1].GamePiece, sortedSqrs[0].GamePiece);

			bool canRotate = !ChessBoard.Instance.GetKingCheckEvaluation(Owner == Ownership.WHITE);

			(sortedSqrs[0].GamePiece, sortedSqrs[1].GamePiece, sortedSqrs[2].GamePiece, sortedSqrs[3].GamePiece) =
				(sortedSqrs[3].GamePiece, sortedSqrs[2].GamePiece, sortedSqrs[1].GamePiece, sortedSqrs[0].GamePiece);

			return canRotate;
		}

		///<summary>
		///Returns whether the attack board can be inverted or uninverted
		///</summary>
		///<param name="asWhite"></param>
		///<returns>Whether the attack board can be inverted or uninverted</returns>
		public bool CanInvert(bool asWhite) {
			if (GetPieceCount() != 1 || GetSinglePiece().IsWhite != asWhite) return false;
			if (Owner == Ownership.NEUTRAL) return false;
			return !PinnedSquare.IsFullyOccupiedByABs();
		}

		///<summary>
		///Moves the attackboard
		///</summary>
		///<param name="move">The attackboard move</param>
		public void Move(AttackBoardMove move) {
			//if the move is a rotation
			if (move.HasMoveEvent(MoveEvent.ATTACK_BOARD_INVERSION)) {
				//invert the attack board
				Invert();

				//set the piece on the attack board as moved
				GetSinglePiece().SetMoved(move);

				//set the notation of the attackboard
				SetBoardNotation();
				return;
			}

			//get the pawn on the board, if there is one
			Pawn pawn = GetSinglePiece() as Pawn;

			//if the move is a rotation
			if (move.HasMoveEvent(MoveEvent.ATTACK_BOARD_ROTATE)) {
				//if the move is a pawn promotion, ask the user what piece to promote to if they haven't been asked already, then wait
				if (move.Promotion == PieceType.NONE && AtEndOfBoard(move.Player.IsWhite) && GetSinglePiece() is Pawn) {
					Game.Instance.StartCoroutine(move.GetPromotionChoice());
					throw new Exception("Must wait for promotion choice");
				}

				//rotate the board
				Rotate(move);

				//set the piece as moved
				GetSinglePiece().SetMoved(move);

				//if the move is not a promotion, return
				if (move.Promotion == PieceType.NONE) return;

				//execute the promotion
				if (move.PromotionUndoRedoHolder == null) {
					move.PromotionUndoRedoHolder = pawn;
					pawn.Promote(move.Promotion);
				} else {
					ChessPiece promotedPiece = move.PromotionUndoRedoHolder;
					move.PromotionUndoRedoHolder = pawn;
					pawn.SetCaptured();
					promotedPiece.SetUncaptured();
					pawn.GetSquare().GamePiece = promotedPiece;
				}

				//mark the move as having made a promotion
				move.AddMoveEvent(MoveEvent.PROMOTION);

				//finish the promotion move
				if (Game.Instance.AtCurrentPosition()) Game.Instance.FinishTurn(move);
				return;
			}

			//calculate the position change in x, y, and z directions
			int xDiff = 0, zDiff = 0, yDiff = IsInverted ? -1 : 1;
			if (Math.Abs(move.EndSqr.FileIndex - move.StartSqr.FileIndex) > 1) {  //if the board is moving in the x direction
				//calculate the change in x position
				xDiff = Math.Sign(move.EndSqr.FileIndex - move.StartSqr.FileIndex) * 4;
			} else {  //the board is moving in the z direction
				//calculate the change in z position
				zDiff = move.EndSqr.HeightMatch(PinnedSquare) ? 4 : 2;
				zDiff *= Math.Sign(move.EndSqr.Rank - move.StartSqr.Rank);
			}

			//if the user hasn't been asked what piece to promote a pawn to
			if (pawn != null && move.Promotion == PieceType.NONE) {
				//get the square the pawn will be on when the board moves
				Square newSquare = pawn.GetSquare().Clone() as Square;
				newSquare.Coords = new Vector3Int(newSquare.FileIndex + xDiff, move.EndSqr.BrdHeight + yDiff, newSquare.Rank + zDiff);
				//move all the squares on the attack board to their new positions
				foreach (Square sqr in Squares)
					sqr.Coords = new Vector3Int(sqr.FileIndex + xDiff, move.EndSqr.BrdHeight + yDiff, sqr.Rank + zDiff);

				bool canBePromoted = pawn.CanBePromoted(newSquare);

				//move all the squares on the attack board back to their current positions
				foreach (Square sqr in Squares)
					sqr.Coords = new Vector3Int(sqr.FileIndex - xDiff, move.StartPinSqr.BrdHeight + yDiff, sqr.Rank - zDiff);

				//if the move is a pawn promotion, ask the user what piece to promote to, then wait
				if (canBePromoted) {
					Game.Instance.StartCoroutine(move.GetPromotionChoice());
					throw new Exception("Must wait for promotion choice");
				}
			}

			//change the y value of the attack board
			Y = move.EndSqr.BrdHeight + yDiff;

			//move all the Squares on the attack board to their new positions
			foreach (Square sqr in Squares)
				sqr.Coords = new Vector3Int(sqr.FileIndex + xDiff, Y, sqr.Rank + zDiff);

			//calculate the displacement of the attack board from the end square in the x and z directions
			float xDisplace = 0.5f, zDisplace = 0.5f, yDisplace = yDiff * 1.5f;
			if (move.EndSqr.FileIndex == 1) xDisplace *= -1;
			if (move.EndSqr.Rank % 2 == 1) zDisplace *= -1;

			//change the position of the attackboard
			MoveTo(move.EndSqr.transform.position + new Vector3(xDisplace, yDisplace, zDisplace));

			//if there is a piece on the attack board, set it as moved
			GetSinglePiece()?.SetMoved(move);

			//set the current pinned square as unoccupied
			if (IsInverted) PinnedSquare.BottomPin = null;
			else PinnedSquare.TopPin = null;

			//set the end square as the new pinned square and set as occupied
			PinnedSquare = move.EndSqr as PinSquare;
			if (IsInverted) PinnedSquare.BottomPin = this;
			else PinnedSquare.TopPin = this;

			//set the notation of the attackboard
			SetBoardNotation();

			//if there is not a pawn promotion
			if (move.HasMoveEvent(MoveEvent.SECONDARY_PROMOTION) || move.Promotion == PieceType.NONE) return;

			//execute the promotion
			if (move.PromotionUndoRedoHolder == null) {
				move.PromotionUndoRedoHolder = pawn;
				pawn.Promote(move.Promotion);
			} else {
				ChessPiece promotedPiece = move.PromotionUndoRedoHolder;
				move.PromotionUndoRedoHolder = pawn;
				pawn.SetCaptured();
				promotedPiece.SetUncaptured();
				pawn.GetSquare().GamePiece = promotedPiece;
			}

			//mark the move as having made a promotion
			move.AddMoveEvent(MoveEvent.PROMOTION);

			//finish the promotion move
			if (Game.Instance.AtCurrentPosition()) Game.Instance.FinishTurn(move);
		}

		///<summary>
		///Unmoves the attackboard
		///</summary>
		///<param name="move">Attackboard move to undo</param>
		public void Unmove(AttackBoardMove move) {

			//get the piece on the board, if there is one
			ChessPiece piece = GetSinglePiece();

			//if there was a promotion
			if (move.HasMoveEvent(MoveEvent.PROMOTION)) {
				//unpromote the piece
				move.PromotionUndoRedoHolder.SetUncaptured();
				piece.SetCaptured();
				piece.GetSquare().GamePiece = move.PromotionUndoRedoHolder;
				(move.PromotionUndoRedoHolder, piece) = (piece, move.PromotionUndoRedoHolder);
			}

			//if the move was a rotation
			if (move.HasMoveEvent(MoveEvent.ATTACK_BOARD_ROTATE)) {
				//unrotate the attack board
				Rotate(move);

				//update the rights of the piece on the board
				UndoSinglePieceRights(move);

				//set the notation of the attackboard
				SetBoardNotation();

				//update whether the king is in check
				ChessBoard.Instance.UpdateKingCheckState(move.Player.IsWhite);
				return;
			}

			//calculate the change in x and z positions
			int xDiff = 0, zDiff = 0;
			if (Math.Abs(move.EndSqr.FileIndex - move.StartPinSqr.FileIndex) > 1) {  //if the board is moving in the x direction
				//calculate the change in x position
				xDiff = Math.Sign(move.StartPinSqr.FileIndex - move.EndSqr.FileIndex) * 4;
			} else {  //the board is moving in the z direction
				//calculate the change in z position
				zDiff = (move.StartPinSqr.BrdHeight == PinnedSquare.BrdHeight) ? 4 : 2;
				zDiff *= Math.Sign(move.StartPinSqr.Rank - move.EndSqr.Rank);
			}

			//change the y value of the attackboard
			Y = move.StartPinSqr.BrdHeight + (IsInverted ? -1 : 1);

			//move all the squares on the attackboard to their old positions
			foreach (Square sqr in Squares)
				sqr.Coords = new Vector3Int(sqr.FileIndex + xDiff, Y, sqr.Rank + zDiff);

			//calculate the displacement of the attackboard from the end square in the x, y, and z directions
			float xDisplace = 0.5f, zDisplace = 0.5f, yDisplace = IsInverted ? -1.5f : 1.5f;
			if (move.StartPinSqr.FileIndex == 1) xDisplace *= -1;
			if (move.StartPinSqr.Rank % 2 == 1) zDisplace *= -1;

			//change the position of the attackboard
			MoveTo(move.StartPinSqr.transform.position + new Vector3(xDisplace, yDisplace, zDisplace));

			//if there is a piece on the attackboard update its rights
			UndoSinglePieceRights(move);

			//set the current pinned square as unoccupied
			if (IsInverted) PinnedSquare.BottomPin = null;
			else PinnedSquare.TopPin = null;

			//set the end square as the new pinned square and set as occupied
			PinnedSquare = move.StartPinSqr as PinSquare;
			if (IsInverted) PinnedSquare.BottomPin = this;
			else PinnedSquare.TopPin = this;

			//set the notation of the attackboard
			SetBoardNotation();

			//update whether the king is in check
			ChessBoard.Instance.UpdateKingCheckState(move.Player.IsWhite);
		}

		///<summary>
		///Moves the attack board to the given position
		///</summary>
		///<param name="pos">The position to move the attackboard to</param>
		private void MoveTo(Vector3 pos) {
			if (!Game.Instance.AllowMoves) {
				MoveInstant(pos);
				return;
			}

			if (pos.y != transform.position.y) {
				MoveInArc(pos);
				return;
			}

			MoveInLine(pos);
		}

		///<summary>
		///Moves the attack board instantly to the given position
		///</summary>
		///<param name="pos">The position to move the piece to</param>
		private void MoveInstant(Vector3 pos) => transform.position = pos;

		///<summary>
		///Moves the attack board in a straight line to the given position
		///</summary>
		///<param name="pos">The position to move the piece to</param>
		private void MoveInLine(Vector3 pos) => transform.DOMove(pos, _TWEEN_SPEED);

		///<summary>
		///Moves the attack board in an arc to the given position
		///</summary>
		///<param name="pos">The position to move the piece to</param>
		private void MoveInArc(Vector3 pos) {
			float height = pos.y > transform.position.y ? _UP_ARC_HEIGHT : _DOWN_ARC_HEIGHT;
			transform.DOJump(pos, height, 1, _TWEEN_SPEED);
		}

		///<summary>
		///Rotates the attack board and a piece on it 180 degrees
		///</summary>
		///<param name="move">The attack board move</param>
		public void Rotate(AttackBoardMove move) {
			//get the piece on the attack board
			ChessPiece piece = GetSinglePiece();

			//update the coordinates of the squares on the attack board
			foreach (Square sqr in Squares)
				sqr.Coords += (sqr.FileIndex % 2 == 0 ? Vector3Int.right : Vector3Int.left) +
					(sqr.Rank % 2 == 0 ? Vector3Int.forward : Vector3Int.back);

			//instantly rotate the attack board
			if (!Game.Instance.AtCurrentPosition()) {
				RotateInstant();
				return;
			}

			//smoothly rotate the attack board
			RotateSmooth(piece);
		}

		///<summary>
		///Rotates the attack board instantly
		///</summary>
		private void RotateInstant() => transform.Rotate(Vector3.up * 180);

		///<summary>
		///Rotates the attackboard smoothly 180 degrees
		///</summary>
		///<param name="piece">The piece on the board</param>
		private void RotateSmooth(ChessPiece piece) {
			//rotate the attackboard gameobject
			transform.DORotate((Quaternion.Euler(0f, 180f, 0f) * transform.rotation).eulerAngles, _TWEEN_SPEED);

			//counter-rotate the piece
			piece.transform.DORotate(piece.transform.rotation.eulerAngles, _TWEEN_SPEED);
		}

		///<summary>
		///Inverts or uninverts the attack board
		///</summary>
		private void Invert() {
			IsInverted = !IsInverted;

			int direction = IsInverted ? -1 : 1;

			(PinnedSquare.TopPin, PinnedSquare.BottomPin) = (PinnedSquare.BottomPin, PinnedSquare.TopPin);

			Y += 2 * direction;

			Vector3Int sqrDiff = Vector3Int.up * (direction * 2);
			foreach(Square sqr in this) sqr.Coords += sqrDiff;

			if (Game.Instance.AtCurrentPosition()) InvertSmooth(direction);
			else InvertInstant(direction);
		}

		///<summary>
		///Inverts the attack board instantly
		///</summary>
		private void InvertInstant(int direction) {
			transform.position += direction * 3f * Vector3.up;
			_supportPillar.transform.position -= direction * 1.5f * Vector3.up;
		}

		///<summary>
		///Inverts the attack board smoothly
		///</summary>
		private void InvertSmooth(int direction) {
			transform.DOMove(transform.position + (direction * 3f * Vector3.up), _TWEEN_SPEED);
			_supportPillar.transform.DOMove(transform.position + (direction * 2.25f * Vector3.up), _TWEEN_SPEED);
		}

		///<summary>
		///Returns the first piece found on the attack board
		///</summary>
		///<returns>The first piece found on the attack board</returns>
		public ChessPiece GetSinglePiece() {
			//if there is a piece on the attack board, return the piece
			foreach (Square sqr in Squares)
				if (sqr.HasPiece()) return sqr.GamePiece;

			return null;
		}

		///<summary>
		///Undoes the removal of rights from the piece on the board, if there is one
		///</summary>
		///<param name="move">The move which removed the piece's rights</param>
		private void UndoSinglePieceRights(AttackBoardMove move) {
			ChessPiece piece = GetSinglePiece();

			if (piece == null) return;

			if (move.HasMoveEvent(MoveEvent.LOST_CASTLING_RIGHTS)) {
				if (piece is King) (piece as King).HasCastlingRights = true;
				else (piece as Rook).HasCastlingRights = true;
				return;
			}

			if (move.HasMoveEvent(MoveEvent.LOST_DOUBLE_SQUARE_MOVE_RIGHTS)) (piece as Pawn).HasDSMoveRights = true;
		}

		///<summary>
		///Returns whether the attack board is at the end of the board
		///</summary>
		///<returns>Whether the attack board is at the end of the board</returns>
		///<param name="asWhite">Whether the end of the board is in relation to the white or black player</param>
		private bool AtEndOfBoard(bool asWhite) => asWhite ? PinnedSquare.Rank == 8 : PinnedSquare.Rank == 1;

		///<summary>
		///Toggles whether the attack boards squares are highlighted
		///</summary>
		///<param name="toggle">Whether to highlight the squares</param>
		public void ToggleSquaresHighlight(bool toggle) {
			foreach (Square sqr in this) sqr.ToggleSelectedHighlight(toggle);
		}

		///<summary>
		///Sets the notation of the board
		///</summary>
		public override void SetBoardNotation() {
			Notation = PinnedSquare.Level;
			if (IsInverted) Notation += 'I';
		}

		///<summary>
		///Selects the attackboard
		///</summary>
		public void Select() {
			Square square = null;
			foreach (Square sqr in this)
				if (!sqr.HasPiece()) square = sqr;
			if (square != null) Game.Instance.SelectSquare(square);
		}

		///<summary>
		///Returns a string of data about the object
		///</summary>
		///<returns>A string of data about the object</returns>
		public override string ToString() {
			var str = new StringBuilder(base.ToString());
			str.Append("\nOwner: ").Append(Owner);
			return str.ToString();
		}
	}
}
