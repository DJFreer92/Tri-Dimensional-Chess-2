using System.Collections.Generic;
using UnityEngine;

public sealed class Knight : ChessPiece {
	//the notation and figurine characters of the knight
	private const string _STANDARD_CHARACTER = "N", _FIGURINE_CHARACTER = "♘";
	//knight offsets
	public static readonly int[][] Offsets = {
		new int[] {-2, -1},
		new int[] {-2, 1},
		new int[] {-1, -2},
		new int[] {-1, 2},
		new int[] {1, -2},
		new int[] {1, 2},
		new int[] {2, -1},
		new int[] {2, 1}
	};
	//white knight prefab gameobject
	public static GameObject WhitePrefab {get; private set;}
	//black knight prefab gameobject
	public static GameObject BlackPrefab {get; private set;}

	///<summary>
	///Returns a list of all the knight's available moves
	///</summary>
	///<param name="aswhite">Whether the knight is being moved by white</param>
	///<returns>A list of all the knight's available moves</returns>
	public override List<Square> GetAvailableMoves(bool asWhite) {
		var moves = new List<Square>();
		if (IsWhite != asWhite) return moves;
		Square square = GetSquare();
		foreach (int[] offset in Offsets) {
			int x = square.Coords.x + offset[0];
			int z = square.Coords.z + offset[1];
			if (x < 0 || x > 5 || z < 0 || z > 9) continue;
			foreach (Square sqr in ChessBoard.Instance.GetEnumerableSquares()) {
				if (sqr.Coords.x != x || sqr.Coords.z != z) continue;
				if (sqr.HasPiece() && sqr.GamePiece.IsWhite == IsWhite) continue;
				if (!King.WillBeInCheck(new PieceMove(Game.Instance.GetPlayer(IsWhite), square, sqr))) moves.Add(sqr);
			}
		}
		return moves;
	}

	///<summary>
	///Set the white and black knight prefabs
	///</summary>
	///<param name="whitePrefab">The white knight prefab</param>
	///<param name="blackPrefab">The black knight prefab</param>
	public static void SetPrefabs(GameObject whitePrefab, GameObject blackPrefab) {
		WhitePrefab = whitePrefab;
		BlackPrefab = blackPrefab;
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
