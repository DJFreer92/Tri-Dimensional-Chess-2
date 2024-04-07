using System.Collections.Generic;
using UnityEngine;

public sealed class Knight : ChessPiece {
	//the notation and figurine characters of the knight
	private const string _STANDARD_CHARACTER = "N", _FIGURINE_CHARACTER = "♘";
	//offsets from the knight where it can move to
	public static readonly Vector2Int[] OFFSETS = {
		Vector2Int.up * 2 + Vector2Int.right,
		Vector2Int.up * 2 + Vector2Int.left,
		Vector2Int.up + Vector2Int.right * 2,
		Vector2Int.up + Vector2Int.left * 2,
		Vector2Int.down * 2 + Vector2Int.right,
		Vector2Int.down * 2 + Vector2Int.left,
		Vector2Int.down + Vector2Int.right * 2,
		Vector2Int.down + Vector2Int.left * 2
	};

	///<summary>
	///Returns a list of all the knight's available moves
	///</summary>
	///<param name="aswhite">Whether the knight is being moved by white</param>
	///<returns>A list of all the knight's available moves</returns>
	public override List<Square> GetAvailableMoves(bool asWhite) {
		var moves = new List<Square>();
		if (IsWhite != asWhite) return moves;
		Square square = GetSquare();
		foreach (var offset in OFFSETS) {
			int x = square.Coords.x + offset.x;
			int z = square.Coords.z + offset.y;
			if (!BoardExtensions.WithinBounds(x, z)) continue;
			foreach (Square sqr in ChessBoard.Instance.GetEnumerableSquares()) {
				if (sqr.Coords.x != x || sqr.Coords.z != z) continue;
				if (sqr.HasPiece() && IsSameColor(sqr.GamePiece)) continue;
				if (!King.WillBeInCheck(new PieceMove(GetOwner(), square, sqr))) moves.Add(sqr);
			}
		}
		return moves;
	}

	///<summary>
	///Returns the notation character of the knight
	///</summary>
	///<param name="wantFigurine">Whether the figurine character is desired</param>
	///<returns>The notation character of the knight</returns>
	public override string GetCharacter(bool wantFigurine) {
		return wantFigurine ? _FIGURINE_CHARACTER : _STANDARD_CHARACTER;
	}
}
