using UnityEngine;
using System.Text;

using TriDimensionalChess.Game.Boards;
using TriDimensionalChess.Game.ChessPieces;
using TriDimensionalChess.Game.Notation;

namespace TriDimensionalChess.Game.Moves {
	public sealed class AttackBoardMove : Move {
		//the attackboard being moved
		public readonly AttackBoard BoardMoved;
		//the square the board moved was pinned to at the start of the move
		public readonly Square StartPinSqr;
		//whether the the move execution should automatically re-executive a second time (should only be set to true when reading PGN)
		public bool AutoReExecute = false;
		//the notation of the attackboard being moved before it has moved
		private readonly string _boardMovedNotationAtStart;

		public AttackBoardMove(Player player, Square start, Square end) : base(player, start, end) {
			BoardMoved = ChessBoard.Instance.GetBoardWithSquare(start) as AttackBoard;
			StartPinSqr = BoardMoved.PinnedSquare;
			_boardMovedNotationAtStart = start.Coords.VectorToBoard();
		}

		///<summary>
		///Executes the move
		///</summary>
		public override void Execute() {
			//if there was a secondary promotion
			if (SecondaryPromotion != PieceType.NONE && !AutoReExecute) {
				//promote the pawn
				if (PromotionUndoRedoHolder == null) {
					PromotionUndoRedoHolder = StartPinSqr.GamePiece;
					(StartPinSqr.GamePiece as Pawn).Promote(SecondaryPromotion);
					Game.Instance.UpdateLastMoveNotation();
					return;
				}

				(PromotionUndoRedoHolder, StartPinSqr.GamePiece) = (StartPinSqr.GamePiece, PromotionUndoRedoHolder);
				StartPinSqr.GamePiece.SetUncaptured();
				PromotionUndoRedoHolder.SetCaptured();
			}

			//determine the departure notation
			DetermineDepartureNotation();

			//make board move
			BoardMoved.Move(this);

			if (AutoReExecute) {
				AutoReExecute = false;
				Execute();
				return;
			}

			if (SecondaryPromotion != PieceType.NONE || !CausesSecondaryPromotion()) return;

			//promote the pawn
			MoveEvents.Add(MoveEvent.SECONDARY_PROMOTION);
			Game.Instance.StartCoroutine(GetPromotionChoice(true));
		}

		///<summary>
		///Undoes the move
		///</summary>
		public override void Undo() {
			//if there was a secondary promotion
			if (MoveEvents.Contains(MoveEvent.SECONDARY_PROMOTION)) {
				//unpromote the piece
				(PromotionUndoRedoHolder, StartPinSqr.GamePiece) = (StartPinSqr.GamePiece, PromotionUndoRedoHolder);
				StartPinSqr.GamePiece.SetUncaptured();
				PromotionUndoRedoHolder.SetCaptured();
			}
			BoardMoved.Unmove(this);
		}

		///<summary>
		///Redoes the move
		///</summary>
		public override void Redo() => Execute();

		///<summary>
		///Determine the departure notation of the move
		///</summary>
		protected override void DetermineDepartureNotation() {
			if (ChessBoard.Instance.CanMultipleAttackBoardsMoveToPin(EndSqr, BoardMoved, Player.IsWhite))
				_departureNotation = _boardMovedNotationAtStart;
		}

		///<summary>
		///Returns the notation in long 3D chess algebraic notation
		///</summary>
		///<returns>The notation</returns>
		public override string GetLongNotation() {
			var move = new StringBuilder();

			//had a secondary promotion
			if (MoveEvents.Contains(MoveEvent.SECONDARY_PROMOTION))
				move.Append(StartPinSqr.GamePiece.GetCharacter(UseFigurineNotation)).Append('/');

			//the attack board position at the start of the move
			move.Append(_boardMovedNotationAtStart);

			//if the board was a rotation
			if (MoveEvents.Contains(MoveEvent.ATTACK_BOARD_ROTATE)) move.Append('⟳');
			else  //- the ending board level
				move.Append('-').Append(BoardMoved.Squares[0].Coords.VectorToBoard());

			//had a pawn promotion
			if (MoveEvents.Contains(MoveEvent.PROMOTION))
				move.Append('(').Append(BoardMoved.GetSinglePiece().GetCharacter(UseFigurineNotation)).Append(')');

			//add addtional move info and return
			return move.Append(GetEndingNotation()).ToString();
		}

		///<summary>
		///Returns the notation in short 3D chess algebraic notation
		///</summary>
		///<returns>The notation in short 3D chess algebraic notation</returns>
		public override string GetShortNotation() {
			var move = new StringBuilder();

			//had a secondary promotion
			if (MoveEvents.Contains(MoveEvent.SECONDARY_PROMOTION))
				move.Append(StartPinSqr.GamePiece.GetCharacter(UseFigurineNotation)).Append('/');

			//if the board was a rotation
			if (MoveEvents.Contains(MoveEvent.ATTACK_BOARD_ROTATE)) move.Append('⟳');
			else {  //not a rotation
				//if multiple attack boards can move to the pin, specify the departure level
				if (!string.IsNullOrEmpty(_departureNotation)) move.Append(_departureNotation).Append('-');
				//the new level of the board
				move.Append(BoardMoved.Squares[0].Coords.VectorToBoard());
			}

			//had a pawn promotion
			if (MoveEvents.Contains(MoveEvent.PROMOTION)) move.Append(BoardMoved.GetSinglePiece().GetCharacter(UseFigurineNotation));

			//add addtional move info and return
			return move.Append(GetEndingNotation()).ToString();
		}

		///<summary>
		///Returns whether the attack board move will cause a secondary promotion
		///</summary>
		///<returns>Whether the attack board move will cause a secondary promotion</returns>
		public bool CausesSecondaryPromotion() {
			return StartPinSqr.HasPiece() &&  //the square the attackboard is pinned to has a piece
				StartPinSqr.GamePiece.Type == PieceType.PAWN &&  //the piece is a pawn
				StartPinSqr.Coords.z == (StartPinSqr.GamePiece.IsWhite ? 8 : 1);  //the piece is at the opposite end of the board form its starting position
		}
	}
}
