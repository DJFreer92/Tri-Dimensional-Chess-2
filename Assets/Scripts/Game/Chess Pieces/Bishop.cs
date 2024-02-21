using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Bishop : ChessPiece {
	//the notation and figurine characters of the bishop
	private const string _STANDARD_CHARACTER = "B", _FIGURINE_CHARACTER = "♗";
	//white bishop prefab gameobject
	public static GameObject WhitePrefab {get; private set;}
	//black bishop prefab gameobject
	public static GameObject BlackPrefab {get; private set;}
	//whether the color of squares the bishop travels on is white
	[field: SerializeField] public bool SquareColorIsWhite {get; set;}

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
				for (var dist = 1; dist < 9; dist++) {
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
	///Set the white and black bishop prefabs
	///</summary>
	///<param name="whitePrefab">The white bishop prefab</param>
	///<param name="blackPrefab">The black bishop prefab</param>
	public static void SetPrefabs(GameObject whitePrefab, GameObject blackPrefab) {
		WhitePrefab = whitePrefab;
		BlackPrefab = blackPrefab;
	}

	///<summary>
	///Returns the notation character of the bishop
	///</summary>
	///<param name="wantFigurine">Whether the figurine character is desired</param>
	///<returns>The notation character of the bishop</returns>
	public override string GetCharacter(bool wantFigurine) {
		return wantFigurine ? Bishop._FIGURINE_CHARACTER : Bishop._STANDARD_CHARACTER;
	}
}