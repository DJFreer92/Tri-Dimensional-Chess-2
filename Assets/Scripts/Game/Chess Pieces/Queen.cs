using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Queen : ChessPiece {
	//the notation and figurine characters of the queen
	private const string _STANDARD_CHARACTER = "Q", _FIGURINE_CHARACTER = "♕";
	//white queen prefab gameobject
	public static GameObject WhitePrefab {get; private set;}
	//black queen prefab gameobject
	public static GameObject BlackPrefab {get; private set;}

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
				bool blocked = false;
				for (var dist = 1; dist <= 9; dist++) {
					int x = xd * dist + square.Coords.x;
					int z = zd * dist + square.Coords.z;
					if (x < 0 || x > 5 || z < 0 || z > 9) break;
					foreach (Square sqr in ChessBoard.Instance.GetEnumerableSquares()) {
						if (sqr.Coords.x != x || sqr.Coords.z != z) continue;
						if (sqr.HasPiece()) {
							blocked = true;
							if (sqr.GamePiece.IsWhite == IsWhite) continue;
						}
						if (!King.WillBeInCheck(new PieceMove(Game.Instance.GetPlayer(IsWhite), square, sqr))) moves.Add(sqr);
					}
					if (blocked) break;
				}
			}
		}
		return moves;
	}

	///<summary>
	///Set the white and black queen prefabs
	///</summary>
	///<param name="whitePrefab">The white queen prefab</param>
	///<param name="blackPrefab">The black queen prefab</param>
	public static void SetPrefabs(GameObject whitePrefab, GameObject blackPrefab) {
		WhitePrefab = whitePrefab;
		BlackPrefab = blackPrefab;
	}

	///<summary>
	///Returns the notation character of the queen
	///</summary>
	///<param name="wantFigurine">Whether the figurine character is desired</param>
	///<returns></returns>
	public override string GetCharacter(bool wantFigurine) {
		return wantFigurine ? Queen._FIGURINE_CHARACTER : Queen._STANDARD_CHARACTER;
	}
}