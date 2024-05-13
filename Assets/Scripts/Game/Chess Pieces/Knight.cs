using System;
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
			foreach (Square sqr in ChessBoard.Instance.EnumerableSquares()) {
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

	///<summary>
	///Returns whether the path of the knight is clear of pieces
	///</summary>
	///<param name="destination">The square the knight is moving to</param>
	///<param name="straightFirst">Whether the knight should move staight first</param>
	///<returns>Whether the path of the knight is clear of pieces</returns>
	public bool IsPathClear(Square destination, bool straightFirst) {
		Square curSqr = GetSquare();
		Vector3Int direction;
		int xDist = destination.Coords.x - curSqr.Coords.x;
		int zDist = destination.Coords.z - curSqr.Coords.z;

		direction = straightFirst ? Vector3Int.forward : Vector3Int.right;
		int dist = straightFirst ? zDist : xDist;
		direction *= Math.Sign(dist);

		while (true) {
			if (ChessBoard.Instance.GetSquareAt(curSqr.Coords + direction).HasPiece()) return false;
			if (dist == (straightFirst ? direction.z : direction.x)) break;
			direction *= 2;
		}

		direction = straightFirst ? Vector3Int.right : Vector3Int.forward;
		direction *= -Math.Sign(straightFirst ? xDist : zDist);
		Square sqr = ChessBoard.Instance.GetSquareAt(destination.Coords + direction);

		return sqr == destination || !sqr.HasPiece();
	}

	///<summary>
	///Update the piece rights that are lost when the piece moves
	///</summary>
	///<param name="move">The move of the piece</param>
	public override void SetMoved(Move move) {}
}
