using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

using TriDimensionalChess.Game.Boards;
using TriDimensionalChess.Game.Moves;

namespace TriDimensionalChess.Game.ChessPieces {
	public sealed class King : ChessPiece {
		//the notation and figurine characters of the king
		private const string _STANDARD_CHARACTER = "K", _FIGURINE_CHARACTER = "♔";

		//whether the king is in check
		public bool IsInCheck;
		//has castling rights
		public bool HasCastlingRights = true;

		///<summary>
		///Returns whether the king will be in check if the given piece move is executed
		///</summary>
		///<param name"move">The potential piece move</param>
		///<returns>Whether the king will be in check if the given piece move is executed</returns>
		public static bool WillBeInCheck(PieceMove move) {
			bool willBeInCheck;

			//if the move is castling
			if (move.HasMoveEventOf(MoveEvent.CASTLING)) {
				//if king side castling
				if (Math.Abs(move.EndSqr.FileIndex - move.StartSqr.FileIndex) == 1) {
					//assign the pieces to their new Squares
					move.StartSqr.GamePiece = move.EndSqr.GamePiece;
					move.EndSqr.GamePiece = move.PieceMoved;

					willBeInCheck = ChessBoard.Instance.GetKingCheckEvaluation(move.Player.IsWhite);

					//assign the pieces back to their old Squares
					move.EndSqr.GamePiece = move.StartSqr.GamePiece;
					move.StartSqr.GamePiece = move.PieceMoved;

					//return whether the move put the king is check
					return willBeInCheck;
				}

				//queen side castling
				//get the squares of the king and the rook
				Square kingSqr = move.StartSqr.GamePiece is King ? move.StartSqr : move.EndSqr;
				Square rookSqr = move.StartSqr.GamePiece is Rook ? move.StartSqr : move.EndSqr;

				//get the king and the rook
				King king = kingSqr.GamePiece as King;
				Rook rook = rookSqr.GamePiece as Rook;

				//get the king's landing square
				Square kingLandingSqr = ChessBoard.Instance.GetSquareAt(
					new Vector3Int(rookSqr.FileIndex + 1, rookSqr.BrdHeight, rookSqr.Rank)
				);

				//assign the pieces to their new Squares
				kingSqr.GamePiece = rook;
				rookSqr.GamePiece = null;
				kingLandingSqr.GamePiece = king;

				willBeInCheck = ChessBoard.Instance.GetKingCheckEvaluation(move.Player.IsWhite);

				//assign the pieces back to their old Squares
				kingSqr.GamePiece = king;
				rookSqr.GamePiece = rook;
				kingLandingSqr.GamePiece = null;

				//return whether the move put the king is check
				return willBeInCheck;
			}

			//move is not castling
			//move the piece from the starting square to the ending square
			ChessPiece endPiece = move.EndSqr.GamePiece;
			move.EndSqr.GamePiece = move.PieceMoved;
			move.StartSqr.GamePiece = null;

			//find if the king is in check
			willBeInCheck = ChessBoard.Instance.GetKingCheckEvaluation(move.Player.IsWhite);

			//move the piece back to the starting square
			move.StartSqr.GamePiece = move.PieceMoved;
			move.EndSqr.GamePiece = endPiece;

			//return whether the king was found to be in check
			return willBeInCheck;
		}

		///<summary>
		///Returns whether the king will be in check if the given attackboard move is executed
		///</summary>
		///<param name="move">The potential piece move</param>
		///<returns>Whether the king will be in check if the given attackboard move is executed</returns>
		public static bool WillBeInCheck(AttackBoardMove move) {
			int xDiff = 0, zDiff = 0;

			//calculate the change in x and z positions
			if (Math.Abs(move.EndSqr.FileIndex - move.StartSqr.FileIndex) > 1) {  //if the board is moving in the x direction
				//calculate the change in x position
				xDiff = Math.Sign(move.EndSqr.FileIndex - move.StartSqr.FileIndex) * 4;
			} else {  //the board is moving in the z direction
				//calculate the change in z position
				zDiff = move.EndSqr.HeightMatch(move.BoardMoved.PinnedSquare) ? 4 : 2;
				zDiff *= Math.Sign(move.EndSqr.Rank - move.StartSqr.Rank);
			}

			//move all the Squares on the attackboard to their new positions
			foreach (Square sqr in move.BoardMoved.Squares)
				sqr.Coords = new Vector3Int(sqr.FileIndex + xDiff, move.EndSqr.BrdHeight + 1, sqr.Rank + zDiff);

			//determine whether the move puts the king in check
			bool willBeInCheck = ChessBoard.Instance.GetKingCheckEvaluation(move.Player.IsWhite);

			/*if the king was not put into check by the attack board move and the move causes an opponent promotion,
			*determine whether the promotion could put the king into check
			*/
			if (!willBeInCheck && move.CausesSecondaryPromotion() && !move.StartPinSqr.GamePiece.BelongsTo(move.Player)) {
				//get the pawn that would be promoted and the square it is on
				Square sqr = move.BoardMoved.PinnedSquare;
				var pawn = sqr.GamePiece as Pawn;

				//simulate the pawn promoting to a queen and determine whether that puts the king in check
				Queen tempQueen = pawn.gameObject.AddComponent<Queen>();
				tempQueen.SetWhite(pawn.IsWhite);
				tempQueen.Type = PieceType.QUEEN;
				sqr.GamePiece = tempQueen;
				willBeInCheck = ChessBoard.Instance.GetKingCheckEvaluation(move.Player.IsWhite);
				Destroy(tempQueen);

				//if the pawn promoting to a queen does not put the king into check
				if (!willBeInCheck) {
					//simulate the pawn promoting to a knight and determine whether that puts the king in check
					Knight tempKnight = pawn.gameObject.AddComponent<Knight>();
					tempKnight.SetWhite(pawn.IsWhite);
					tempKnight.Type = PieceType.KNIGHT;
					sqr.GamePiece = tempKnight;
					willBeInCheck = ChessBoard.Instance.GetKingCheckEvaluation(move.Player.IsWhite);
					Destroy(tempKnight);
				}

				//reset the pawn
				sqr.GamePiece = pawn;
			}

			//move all the Squares on the attackboard back to their old positions
			foreach (Square sqr in move.BoardMoved.Squares)
				sqr.Coords = new Vector3Int(sqr.FileIndex - xDiff, move.BoardMoved.Y, sqr.Rank - zDiff);

			//return whether the move put the king in check
			return willBeInCheck;
		}

		///<summary>
		///Returns a list of all the king's available moves
		///</summary>
		///<param name="asWhite">Whether the king is being moved by white</param>
		///<returns>A list of all the king's available moves</returns>
		public override List<Square> GetAvailableMoves(bool asWhite) {
			var moves = new List<Square>();
			if (IsWhite != asWhite) return moves;
			Square square = GetSquare();
			foreach (Square sqr in ChessBoard.Instance.EnumerableSquares()) {
				ChessPiece PieceOnSqr = sqr.GamePiece;
				if (PieceOnSqr is Rook && HasCastlingRights && IsSameColor(PieceOnSqr)) {
					if (!(PieceOnSqr as Rook).HasCastlingRights || Game.Instance.IsFirstMove() || IsInCheck) continue;
					Rook rook = PieceOnSqr as Rook;
					Square blockingSqr = ChessBoard.Instance.GetSquareAt(sqr.Coords + Vector3Int.right);
					if (rook.IsKingSide || (blockingSqr != null && !blockingSqr.HasPiece())) {
						var move = new PieceMove(GetOwner(), square, sqr);
						move.AddMoveEvent(rook.IsKingSide ? MoveEvent.CASTLING_KING_SIDE : MoveEvent.CASTLING_QUEEN_SIDE);
						if (!WillBeInCheck(move)) moves.Add(sqr);
					}
					continue;
				}
				int xDiff = Math.Abs(square.FileIndex - sqr.FileIndex);
				int zDiff = Math.Abs(square.Rank - sqr.Rank);
				if (xDiff > 1 || zDiff > 1 || xDiff + zDiff == 0) continue;
				if (sqr.HasPiece() && IsSameColor(PieceOnSqr)) continue;
				if (!WillBeInCheck(new PieceMove(GetOwner(), square, sqr))) moves.Add(sqr);
			}
			return moves;
		}

		///<summary>
		///Returns whether the king can castle king side
		///</summary>
		///<returns>Whether the king can castle king side</returns>
		public bool HasKSCastleRights() {
			if (!HasCastlingRights) return false;
			foreach (Square sqr in ChessBoard.Instance.EnumerableSquares()) {
				if (sqr.GamePiece is not Rook || !IsSameColor(sqr.GamePiece)) continue;
				Rook rook = sqr.GamePiece as Rook;
				if (rook.IsKingSide) return rook.HasCastlingRights;
			}
			return false;
		}

		///<summary>
		///Returns whether the king can castle queen side
		///</summary>
		///<returns>Whether the king can castle queen side</returns>
		public bool HasQSCastleRights() {
			if (!HasCastlingRights) return false;
			foreach (Square sqr in ChessBoard.Instance.EnumerableSquares()) {
				if (sqr.GamePiece is not Rook || !IsSameColor(sqr.GamePiece)) continue;
				Rook rook = sqr.GamePiece as Rook;
				if (!rook.IsKingSide && rook.HasCastlingRights) return true;
			}
			return false;
		}

		///<summary>
		///Returns the notation character of the king
		///</summary>
		///<param name="wantFigurine">Wether the figurine character is desired</param>
		///<returns>The notation character of the king</returns>
		public override string GetCharacter(bool wantFigurine) {
			return wantFigurine ? _FIGURINE_CHARACTER : _STANDARD_CHARACTER;
		}

		///<summary>
		///Update the piece rights that are lost when the piece moves
		///</summary>
		///<param name="move">The move of the piece</param>
		public override void SetMoved(Move move) {
			if (!HasCastlingRights) return;

			HasCastlingRights = false;
			move.AddMoveEvent(MoveEvent.LOST_CASTLING_RIGHTS);
		}

		///<summary>
		///Update whether the king is in check
		///</summary>
		public void UpdateCheckState() {
			IsInCheck = DetermineCheck();
		}

		///<summary>
		///Returns whether the king is in check
		///</summary>
		///<returns>Whether the king is in check</returns>
		public bool DetermineCheck() {
			//return whether the king is in check
			return ChessBoard.Instance.ArePiecesAttacking(GetSquare(), !IsWhite);
		}

		///<summary>
		///Returns a string of data about the king
		///</summary>
		///<returns>A string of data about the king</returns>
		public override string ToString() {
			var str = new StringBuilder(base.ToString());
			str.Append("\nIs In Check?").Append(IsInCheck);
			return str.ToString();
		}
	}
}
