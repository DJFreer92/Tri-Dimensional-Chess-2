using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Rook : ChessPiece {
	//the notation and figurine characters of the rook
	private const string _STANDARD_CHARACTER = "R", _FIGURINE_CHARACTER = "♖";
	//white rook prefab gameobject
	public static GameObject WhitePrefab {get; private set;}
	//black rook prefab gameobject
	public static GameObject BlackPrefab {get; private set;}
	//has castling rights
	public bool HasCastlingRights = true;
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
	///Sets whether the rook has castling rights
	///</summary>
	///<param name="hasRights">Whether the rook has castling rights</param>
	public void SetCastlingRights(bool hasRights) {
		HasCastlingRights = hasRights;
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
		for (var xd = -1; xd <= 1; xd++) {
			for (var zd = -1; zd <= 1; zd++) {
				if (xd != 0 && zd != 0) continue;
				bool blocked = false;
				for (var dist = 1; dist < 9; dist++) {
					int x = xd * dist + square.Coords.x;
					int z = zd * dist + square.Coords.z;
					if (x < 0 || x > 5 || z < 0 || z > 9) break;
					foreach (Square sqr in ChessBoard.Instance.GetEnumerableSquares()) {
						if (sqr.Coords.x != x || sqr.Coords.z != z) continue;
						ChessPiece PieceOnSqr = sqr.GamePiece;
						if (sqr.HasPiece()) {
							blocked = true;
							if (PieceOnSqr.IsWhite == IsWhite) {
								if (!HasCastlingRights || Game.Instance.IsFirstMove()) continue;
								King king = PieceOnSqr as King;
								if (!(PieceOnSqr is King) || !king.HasCastlingRights || king.IsInCheck) continue;
								if (!IsKingSide && ChessBoard.Instance.GetSquareAt(square.Coords + Vector3Int.right).HasPiece()) continue;
								var move = new PieceMove(Game.Instance.GetPlayer(IsWhite), square, sqr);
								move.MoveEvents.Add(IsKingSide ? MoveEvent.CASTLING_KING_SIDE : MoveEvent.CASTLING_QUEEN_SIDE);
								if (!King.WillBeInCheck(move)) moves.Add(sqr);
								continue;
							}
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
	///Set the white and black rook prefabs
	///</summary>
	///<param name="whitePrefab">The white rook prefab</param>
	///<param name="blackPrefab">The black rook prefab</param>
	public static void SetPrefabs(GameObject whitePrefab, GameObject blackPrefab) {
		WhitePrefab = whitePrefab;
		BlackPrefab = blackPrefab;
	}

	///<summary>
	///Returns the notation character of the rook
	///</summary>
	///<param name="wantFigurine">Whether the figurine character is desired</param>
	///<returns>The notation character of the rook</returns>
	public override string GetCharacter(bool wantFigurine) {
		return wantFigurine ? Rook._FIGURINE_CHARACTER : Rook._STANDARD_CHARACTER;
	}
}