using System;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

using TriDimensionalChess.Game.Boards;
using TriDimensionalChess.Game.ChessPieces;

namespace TriDimensionalChess.Game.Moves {
	public sealed class PieceMove : Move {
		//the piece being moved
		public ChessPiece PieceMoved {get; private set;}
		//the piece being captured
		public ChessPiece PieceCaptured {get; private set;}

		public PieceMove(Player player, Square start, Square end, MoveEvent moveEvents = MoveEvent.NONE) : base(player, start, end, moveEvents) {
			PieceMoved = start.GamePiece;
		}

		///<summary>
		///Executes the move
		///</summary>
		public override void Execute() {
			//if the move is a pawn promotion
			if (Promotion == PieceType.NONE && PieceMoved is Pawn && (PieceMoved as Pawn).CanBePromoted(EndSqr)) {
				//ask the user what piece to promote to, then wait
				Game.Instance.StartCoroutine(GetPromotionChoice());
				throw new Exception("Must wait for promotion choice");
			}

			//find the short algebraic move notion of the departure square
			DetermineDepartureNotation();

			//move the piece
			if (((PieceMoved is King && EndSqr.GamePiece is Rook) || (PieceMoved is Rook && EndSqr.GamePiece is King)) && PieceMoved.IsSameColor(EndSqr.GamePiece)) {  //castling move
				//perform castling
				Castle();
				return;
			}

			//if a piece is being captured
			if (EndSqr.HasPiece()) {
				//find the piece being captured
				PieceCaptured = EndSqr.GamePiece;
				AddMoveEvent(MoveEvent.CAPTURE);
			}

			//if the piece is moving to a neutral attackboard, claim it
			Board endBoard = EndSqr.Brd;
			if (endBoard is AttackBoard && endBoard.Owner == Ownership.NEUTRAL) {
				endBoard.Owner = Player.IsWhite ? Ownership.WHITE : Ownership.BLACK;
				AddMoveEvent(MoveEvent.ATTACK_BOARD_CLAIM);
			}

			//set the piece as moved
			PieceMoved.SetMoved(this);

			if (PieceMoved is Pawn) {  //if the piece being moved is a pawn
				//if the move is a double square move
				if (Math.Abs(StartSqr.Rank - EndSqr.Rank) == 2) AddMoveEvent(MoveEvent.PAWN_DOUBLE_SQUARE);
				else if (!EndSqr.HasPiece() && !StartSqr.FileMatch(EndSqr)) {  //if the move is an en passant
					//find the piece being captured
					PieceCaptured = Pawn.GetJMDSMPawnBehind(EndSqr, Player.IsWhite);

					//find the square of the piece being catured
					_enPassantCaptureSqr = PieceCaptured.GetSquare();

					//unassign piece captured in en passant from its square
					_enPassantCaptureSqr.GamePiece = null;

					AddMoveEvent(MoveEvent.EN_PASSANT);
				}
				(PieceMoved as Pawn).MovePiece(StartSqr, EndSqr);
			} else PieceMoved.MoveTo(EndSqr);  //non-pawn move

			//set the piece being captured as captured
			PieceCaptured?.SetCaptured();

			//move the piece from the starting square to the ending square
			EndSqr.GamePiece = PieceMoved;
			StartSqr.GamePiece = null;

			//if the piece moved between boards, make it a child of the board it landed on
			if (StartSqr.Brd != EndSqr.Brd) PieceMoved.transform.SetParent(EndSqr.Brd.transform);

			//if the piece is not a pawn or it cannot be promoted, finish the move execution
			if (Promotion == PieceType.NONE) return;

			//execute the promotion
			if (PromotionUndoRedoHolder == null) {
				PromotionUndoRedoHolder = PieceMoved;
				PieceMoved = (PieceMoved as Pawn).Promote(Promotion);
			} else {
				(PromotionUndoRedoHolder, PieceMoved) = (PieceMoved, PromotionUndoRedoHolder);
				PieceMoved.SetUncaptured();
				PromotionUndoRedoHolder.SetCaptured();
				EndSqr.GamePiece = PieceMoved;
				PieceMoved.MoveTo(EndSqr);
			}

			//mark the move as having made a promotion
			AddMoveEvent(MoveEvent.PROMOTION);

			//finish the turn
			if (Game.Instance.AtCurrentPosition()) Game.Instance.FinishTurn(this);
		}

		///<summary>
		///Undoes the move
		///</summary>
		public override void Undo() {
			//if move was king side castle
			if (HasMoveEventOf(MoveEvent.CASTLING)) {
				//get the starting squares for the king and rook
				Square kingLandSqr, rookStartSqr = null;
				if (HasMoveEvent(MoveEvent.CASTLING_KING_SIDE)) {
					kingLandSqr = ChessBoard.Instance.GetSquareAt(
						Player.IsWhite ?
						ChessBoard.WhiteKingSideRookCoords :
						ChessBoard.BlackKingSideRookCoords
					);
				} else {
					kingLandSqr = ChessBoard.Instance.GetSquareAt(
						Player.IsWhite ?
						ChessBoard.WhiteQueenSideKingLandingCoords :
						ChessBoard.BlackQueenSideKingLandingCoords
					);
					rookStartSqr = ChessBoard.Instance.GetSquareAt(
						Player.IsWhite ?
						ChessBoard.WhiteQueenSideRookCoords :
						ChessBoard.BlackQueenSideRookCoords
					);
				}
				Square rookLandSqr = ChessBoard.Instance.GetSquareAt(
					Player.IsWhite ?
					ChessBoard.WhiteKingCoords :
					ChessBoard.BlackKingCoords
				);

				//get the king and rook
				King king = kingLandSqr.GamePiece as King;
				Rook rook = rookLandSqr.GamePiece as Rook;

				//move the king and rook to their starting squares
				king.MoveTo(rookLandSqr);
				rook.MoveTo(rookStartSqr ?? kingLandSqr);

				//assign the king and the rook to their starting squares
				(rookStartSqr ?? kingLandSqr).GamePiece = rook;
				rookLandSqr.GamePiece = king;
				if (rookStartSqr != null) kingLandSqr.GamePiece = null;

				//if it was queen side castling, assign the pieces as children of the boards they landed on
				if (HasMoveEvent(MoveEvent.CASTLING_QUEEN_SIDE)) {
					king.transform.SetParent(rookLandSqr.Brd.transform);
					rook.transform.SetParent(rookStartSqr.Brd.transform);
				}

				//give the king and rook their castling rights back
				king.HasCastlingRights = true;
				rook.HasCastlingRights = true;
				return;
			}

			//move the piece back to its original position
			PieceMoved.MoveTo(StartSqr);
			StartSqr.GamePiece = PieceMoved;
			EndSqr.GamePiece = null;

			//if the piece moved between boards, make it a child of the board it landed on
			if (StartSqr.Brd != EndSqr.Brd) PieceMoved.transform.SetParent(StartSqr.Brd.transform);

			//if move was en passant
			if (HasMoveEvent(MoveEvent.EN_PASSANT)) {
				PieceCaptured.SetUncaptured();
				PieceCaptured.MoveTo(_enPassantCaptureSqr);
				_enPassantCaptureSqr.GamePiece = PieceCaptured;

				//update whether the king is in check
				ChessBoard.Instance.UpdateKingCheckState(Player.IsWhite);
				return;
			}

			switch (PieceMoved.Type) {
				case PieceType.PAWN:
					//if the move was a double square pawn move
					if (HasMoveEvent(MoveEvent.PAWN_DOUBLE_SQUARE)) (PieceMoved as Pawn).JustMadeDSMove = false;
					//if the pawn lost double square move rights, give them back
					if (HasMoveEvent(MoveEvent.LOST_DOUBLE_SQUARE_MOVE_RIGHTS)) (PieceMoved as Pawn).HasDSMoveRights = true;
					break;
				case PieceType.KING:
					//if the king lost castling rights, give them back
					if (HasMoveEvent(MoveEvent.LOST_CASTLING_RIGHTS)) (PieceMoved as King).HasCastlingRights = true;
					break;
				case PieceType.ROOK:
					//if the rook lost castling rights, give them back
					if (HasMoveEvent(MoveEvent.LOST_CASTLING_RIGHTS)) (PieceMoved as Rook).HasCastlingRights = true;
					break;
			}

			//if move was a capture
			if (HasMoveEvent(MoveEvent.CAPTURE)) {
				PieceCaptured.SetUncaptured();
				EndSqr.GamePiece = PieceCaptured;
			}

			//if the move claimed an attackboard, unclaim it
			if (HasMoveEvent(MoveEvent.ATTACK_BOARD_CLAIM)) EndSqr.Brd.Owner = Ownership.NEUTRAL;

			//if move was a promotion
			if (HasMoveEvent(MoveEvent.PROMOTION)) {
				(PieceMoved, PromotionUndoRedoHolder) = (PromotionUndoRedoHolder, PieceMoved);
				PieceMoved.SetUncaptured();
				PromotionUndoRedoHolder.SetCaptured();
				PromotionUndoRedoHolder.GetSquare().GamePiece = PieceMoved;
				PieceMoved.MoveTo(PieceMoved.GetSquare());
			}

			//update whether the king is in check
			ChessBoard.Instance.UpdateKingCheckState(Player.IsWhite);
		}

		///<summary>
		///Undoes the move
		///</summary>
		public override void Redo() => Execute();

		///<summary>
		///Determine the departure notation of the move
		///</summary>
		protected override void DetermineDepartureNotation() {
			List<Square> attackingSqrs = ChessBoard.Instance.GetAttackingSquares(EndSqr, StartSqr, PieceMoved.Type, Player.IsWhite);
			bool includeFile, includeRank, includeBoard;
			includeFile = includeRank = includeBoard = false;

			foreach (Square attackingSqr in attackingSqrs) {
				if (!attackingSqr.FileMatch(StartSqr)) {
					includeFile = true;
					continue;
				}

				if (attackingSqr.RankMatch(StartSqr)) {
					if (PieceMoved is Pawn) includeFile = true;
					includeBoard = true;
					continue;
				}

				includeRank = true;
			}

			_departureNotation = "";
			if (includeFile) _departureNotation += StartSqr.File;
			if (includeRank) _departureNotation += StartSqr.Rank;
			if (includeBoard) _departureNotation += StartSqr.BrdNotation;
		}

		///<summary>
		///Executes a castling move
		///</summary>
		private void Castle() {
			//find whether the king is the piece that was selected to move
			bool kingIsPrimary = PieceMoved is King;

			//get the king and rook being moved
			King king = (kingIsPrimary ? PieceMoved : EndSqr.GamePiece) as King;
			Rook rook = (kingIsPrimary ? EndSqr.GamePiece : PieceMoved) as Rook;

			//get the squares of the king and rook being moved
			Square kingSqr = kingIsPrimary ? StartSqr : EndSqr;
			Square rookSqr = kingIsPrimary ? EndSqr : StartSqr;

			//remove the future castling rights of the king and rook
			king.HasCastlingRights = false;
			rook.HasCastlingRights = false;

			//move the rook to the king's position
			rook.MoveTo(kingSqr);

			//if king side castling
			if (rook.IsKingSide) {
				//move the king to the rook's position
				king.MoveTo(rookSqr);

				//assign the pieces to their new squares
				kingSqr.GamePiece = rook;
				rookSqr.GamePiece = king;

				AddMoveEvent(MoveEvent.CASTLING_KING_SIDE);
				return;
			}

			//queen side castling
			//get the king's landing square
			Square kingLandingSqr = ChessBoard.Instance.GetSquareAt(rookSqr.Coords + Vector3Int.right);

			//move the king to its landing square
			king.MoveTo(kingLandingSqr);

			//assign the pieces to their new squares
			kingLandingSqr.GamePiece = king;
			kingSqr.GamePiece = rook;
			rookSqr.GamePiece = null;

			//assign the pieces as children of the boards they landed on
			king.transform.SetParent(kingLandingSqr.Brd.transform);
			rook.transform.SetParent(kingSqr.Brd.transform);

			AddMoveEvent(MoveEvent.CASTLING_QUEEN_SIDE);
		}

		///<summary>
		///Returns the notation of the move in short 3D chess algebraic notation
		///</summary>
		///<returns>The short 3D chess algebraic notation of the move</returns>
		public override string GetNotation() {
			//if the move was castling, O-O king side, O-O-O queen side
			if (HasMoveEvent(MoveEvent.CASTLING_KING_SIDE)) return "O-O" + GetEndingNotation();
			if (HasMoveEvent(MoveEvent.CASTLING_QUEEN_SIDE)) return "O-O-O" + GetEndingNotation();

			//piece character
			var move = new StringBuilder();
			if (!HasMoveEvent(MoveEvent.PROMOTION)) move.Append(PieceMoved.GetCharacter(SettingsManager.Instance.FigurineNotation));
			move.Append(_departureNotation);
			//a piece was captured
			if (HasMoveEvent(MoveEvent.CAPTURE)) move.Append('x');
			//ending position
			move.Append(EndSqr.Notation);

			if (HasMoveEvent(MoveEvent.PROMOTION)) move.Append('=').Append(PieceMoved.GetCharacter(SettingsManager.Instance.FigurineNotation));  //had a pawn promotion
			else if (HasMoveEvent(MoveEvent.EN_PASSANT)) move.Append("e.p.");  //en passant
			else if (HasMoveEvent(MoveEvent.ATTACK_BOARD_CLAIM)) move.Append('(').Append(EndSqr.Brd.Notation).Append(')');  //attack board claim

			//set the move notation
			return move.Append(GetEndingNotation()).ToString();
		}

		///<summary>
		///Returns data about the piece move
		///</summary>
		///<returns>Data about the piece move</returns>
		public override string ToString() {
			var str = new StringBuilder(base.ToString());
			str.Append("\nPiece Moved: " + PieceMoved.ToString()).Append(PieceMoved.ToString());
			str.Append("\nPiece Captured: ").Append(PieceCaptured != null ? PieceCaptured.ToString() : "null");
			return str.ToString();
		}
	}
}
