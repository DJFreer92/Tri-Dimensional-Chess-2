using System.Collections.Generic;
using UnityEngine;

public sealed class Queen : ChessPiece {
	//the notation and figurine characters of the queen
	private const string _STANDARD_CHARACTER = "Q", _FIGURINE_CHARACTER = "♕";

	///<summary>
	///Returns a list of all the queen's available moves
	///</summary>
	///<param name="asWhite">Whether the queen is being moved by white</param>
	///<returns>A list of all the queen's available moves</returns>
	public override List<Square> GetAvailableMoves(bool asWhite) {
		var moves = new List<Square>();
		if (IsWhite != asWhite) return moves;
		Square square = GetSquare();
		for (var xd = -1; xd <= 1; xd++) {
			for (var zd = -1; zd <= 1; zd++) {
				if (xd == 0 && zd == 0) continue;
				bool blocked = false;
				for (var dist = 1; dist <= 9; dist++) {
					int x = xd * dist + square.Coords.x;
					int z = zd * dist + square.Coords.z;
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
