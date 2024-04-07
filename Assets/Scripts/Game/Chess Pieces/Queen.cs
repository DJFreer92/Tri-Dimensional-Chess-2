using System.Collections.Generic;
using UnityEngine;

public sealed class Queen : ChessPiece {
	//the notation and figurine characters of the queen
	private const string _STANDARD_CHARACTER = "Q", _FIGURINE_CHARACTER = "♕";
	//the directions the queen can move in
	private static readonly Vector2Int[] _DIRECTIONS = {
		Vector2Int.up,
		Vector2Int.up + Vector2Int.right,
		Vector2Int.right,
		Vector2Int.down + Vector2Int.right,
		Vector2Int.down,
		Vector2Int.down + Vector2Int.left,
		Vector2Int.left,
		Vector2Int.up + Vector2Int.left
	};

	///<summary>
	///Returns a list of all the queen's available moves
	///</summary>
	///<param name="asWhite">Whether the queen is being moved by white</param>
	///<returns>A list of all the queen's available moves</returns>
	public override List<Square> GetAvailableMoves(bool asWhite) {
		var moves = new List<Square>();
		if (IsWhite != asWhite) return moves;
		Square square = GetSquare();
		foreach (var direction in _DIRECTIONS) {
			bool blocked = false;
			for (var dist = 1; dist <= 9; dist++) {
				int x = direction.x * dist + square.Coords.x;
				int z = direction.y * dist + square.Coords.z;
				if (x < 0 || x > 5 || z < 0 || z > 9) break;
				foreach (Square sqr in ChessBoard.Instance.GetEnumerableSquares()) {
					if (sqr.Coords.x != x || sqr.Coords.z != z) continue;
					if (sqr.HasPiece()) {
						blocked = true;
						if (IsSameColor(sqr.GamePiece)) continue;
					}
					if (!King.WillBeInCheck(new PieceMove(GetOwner(), square, sqr))) moves.Add(sqr);
				}
				if (blocked) break;
			}
		}
		return moves;
	}

	///<summary>
	///Returns the notation character of the queen
	///</summary>
	///<param name="wantFigurine">Whether the figurine character is desired</param>
	///<returns></returns>
	public override string GetCharacter(bool wantFigurine) {
		return wantFigurine ? _FIGURINE_CHARACTER : _STANDARD_CHARACTER;
	}
}
