using System.Collections.Generic;
using UnityEngine;

public sealed class Bishop : ChessPiece {
	//the notation and figurine characters of the bishop
	private const string _STANDARD_CHARACTER = "B", _FIGURINE_CHARACTER = "♗";

	///<summary>
	///Returns a list of all the bishop's available moves
	///</summary>
	///<param name="asWhite">Whether the rook is being moved by white</param>
	///<returns>A list of all the bishop's available moves</returns>
	public override List<Square> GetAvailableMoves(bool asWhite) {
		var moves = new List<Square>();
		if (IsWhite != asWhite) return moves;
		Square square = GetSquare();
		for (var xd = -1; xd <= 1; xd += 2) {
			for (var zd = -1; zd <= 1; zd += 2) {
				bool blocked = false;
				for (var dist = 1; dist <= 5; dist++) {
					int x = xd * dist + square.Coords.x;
					int z = zd * dist + square.Coords.z;
					if (x < 0 || x > 5 || z < 0 || z > 9) break;
					foreach (Square sqr in ChessBoard.Instance.GetEnumerableSquares()) {
						if (sqr.Coords.x != x || sqr.Coords.z != z) continue;
						if (sqr.HasPiece()) {
							blocked = true;
							if (sqr.GamePiece.IsWhite == IsWhite) continue;
						}
						if (!King.WillBeInCheck(new PieceMove(GetOwner(), square, sqr))) moves.Add(sqr);
					}
					if (blocked) break;
				}
			}
		}
		return moves;
	}

	///<summary>
	///Returns the notation character of the bishop
	///</summary>
	///<param name="wantFigurine">Whether the figurine character is desired</param>
	///<returns>The notation character of the bishop</returns>
	public override string GetCharacter(bool wantFigurine) {
		return wantFigurine ? _FIGURINE_CHARACTER : _STANDARD_CHARACTER;
	}
}
