using System.Collections.Generic;
using UnityEngine;

using TriDimensionalChess.Game.Boards;
using TriDimensionalChess.Game.Moves;

namespace TriDimensionalChess.Game.ChessPieces {
	public sealed class Rook : ChessPiece {
		//the notation and figurine characters of the rook
		private const string _STANDARD_CHARACTER = "R", _FIGURINE_CHARACTER = "♖";

		//has castling rights
		public bool HasCastlingRights = true;

		//the directions the rook can move in
		private static readonly Vector2Int[] _DIRECTIONS = {
			Vector2Int.up,
			Vector2Int.down,
			Vector2Int.left,
			Vector2Int.right
		};

		//whether the rook starts on the king side
		[field: SerializeField] public bool IsKingSide {get; private set;}

		///<summary>
		///Set the rook is on the king side
		///</summary>
		///<param name="isKingSide">Whether the rook is on the king side</param>
		public void SetKingSide(bool isKingSide) {
			IsKingSide = isKingSide;
		}

		///<summary>
		///Returns a list of all the rook's available moves
		///</summary>
		///<param name="asWhite">Whether the rook is being moved by white</param>
		///<returns>A list of all the rook's available moves</returns>
		public override List<Square> GetAvailableMoves(bool asWhite) {
			var moves = new List<Square>();
			if (IsWhite != asWhite) return moves;
			Square square = GetSquare();
			foreach (var direction in _DIRECTIONS) {
				bool blocked = false;
				for (var dist = 1; dist <= 9; dist++) {
					int x = direction.x * dist + square.FileIndex;
					int z = direction.y * dist + square.Rank;
					if (!BoardExtensions.WithinBounds(x, z)) break;
					foreach (Square sqr in ChessBoard.Instance.EnumerableSquares()) {
						if (sqr.FileIndex != x || sqr.Rank != z) continue;
						ChessPiece PieceOnSqr = sqr.GamePiece;
						if (sqr.HasPiece()) {
							blocked = true;
							if (IsSameColor(PieceOnSqr)) {
								if (!HasCastlingRights || Game.Instance.IsFirstMove()) continue;
								King king = PieceOnSqr as King;
								if (PieceOnSqr is not King || !king.HasCastlingRights || king.IsInCheck) continue;
								if (!IsKingSide && ChessBoard.Instance.GetSquareAt(square.Coords + Vector3Int.right).HasPiece()) continue;
								var move = new PieceMove(GetOwner(), square, sqr);
								move.AddMoveEvent(IsKingSide ? MoveEvent.CASTLING_KING_SIDE : MoveEvent.CASTLING_QUEEN_SIDE);
								if (!King.WillBeInCheck(move)) moves.Add(sqr);
								continue;
							}
						}
						if (!King.WillBeInCheck(new PieceMove(GetOwner(), square, sqr))) moves.Add(sqr);
					}
					if (blocked) break;
				}
			}
			return moves;
		}

		///<summary>
		///Returns the notation character of the rook
		///</summary>
		///<param name="wantFigurine">Whether the figurine character is desired</param>
		///<returns>The notation character of the rook</returns>
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
	}
}
