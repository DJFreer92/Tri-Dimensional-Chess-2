using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public sealed class Pawn : ChessPiece {
	//the notation and figurine characters of the pawn
	private const string _STANDARD_CHARACTER = "", _FIGURINE_CHARACTER = "";
	//whether the pawn can make a double square move
	public bool HasDSMoveRights = true;  //Can make Double Square move
	//holds whether the pawn made a double square move on its last turn
	public bool JustMadeDSMove;  //Just Made Double Square Move

	///<summary>
	///Adds local methods to listeners
	///</summary>
	protected override void RegisterToEvents() {
		base.RegisterToEvents();

		Game.Instance.OnCurrentPlayerChangeWParam += UpdateJustMadeDSMove;
	}

	///<summary>
	///Removes local methods from listeners
	///</summary>
	protected override void UnregisterToEvents() {
		base.UnregisterToEvents();

		if (Game.IsCreated()) Game.Instance.OnCurrentPlayerChangeWParam -= UpdateJustMadeDSMove;
	}

	///<summary>
	///Returns the pawn, that just made a double square move, behind the given square
	///</summary>
	///<param name="sqr">The square to find the pawn behind</param>
	///<param name="isBehindWhite">Whether the pawn behind searched for is behind a white piece</param>
	///<returns>The pawn, that just made a double square move, behind the given square</returns>
	public static Pawn GetJMDSMPawnBehind(Square sqr, bool isBehindWhite) {
		//get the pieces immediately behind the pawn
		var pieces = ChessBoard.Instance.GetPiecesBehind(sqr, isBehindWhite);

		foreach (ChessPiece piece in pieces) {
			//if the piece is a pawn and it just made a double square move, return the pawn
			if (piece is Pawn && (piece as Pawn).JustMadeDSMove) return piece as Pawn;
		}

		//return no pawn found
		return null;
	}

	///<summary>
	///Returns a list of all the pawn's available moves
	///</summary>
	///<param name="asWhite">Whether the pawn is being moved by white</param>
	///<returns>A list of all the pawn's available moves</returns>
	public override List<Square> GetAvailableMoves(bool asWhite) {
		var moves = new List<Square>();
		if (IsWhite != asWhite) return moves;
		Square square = GetSquare();
		int direction = IsWhite ? 1 : -1;
		for (var xd = -1; xd <= 1; xd++) {
			for (var zd = 0; zd <= 2; zd++) {
				if (zd == 0 || (xd != 0 && zd == 2)) continue;
				if (zd == 2 && !HasDSMoveRights) break;
				int x = xd + square.Coords.x;
				int z = zd * direction + square.Coords.z;
				if (x < 0 || x > 5 || z < 0 || z > 9) break;
				bool blocked = false;
				foreach (Square sqr in ChessBoard.Instance.GetEnumerableSquares()) {
					if (sqr.Coords.x != x || sqr.Coords.z != z) continue;
					bool isEnPassant = false;
					if (xd == 0 && sqr.HasPiece()) {  //straight 1 or 2
						blocked = true;
						continue;
					} else if (xd != 0) {  //diagonal capture or en pasant
						if (!sqr.HasPiece()) {
							if (GetJMDSMPawnBehind(sqr, IsWhite) == null) continue;
							isEnPassant = true;
						} else if (sqr.GamePiece.IsWhite == IsWhite) continue;
					}
					var move = new PieceMove(Game.Instance.GetPlayer(IsWhite), square, sqr);
					if (isEnPassant) move.MoveEvents.Add(MoveEvent.EN_PASSANT);
					if (!King.WillBeInCheck(move)) moves.Add(sqr);
				}
				if (blocked) break;
			}
		}
		return moves;
	}

	///<summary>
	///Update whether the pawn just made a double square move based on who's turn it is
	///</summary>
	///<param name="player">The current player</param>
	private void UpdateJustMadeDSMove(Player player) {
		if (player.IsWhite == IsWhite) JustMadeDSMove = false;
	}

	///<summary>
	///Returns the notation character of the king
	///</summary>
	///<param name="wantFigurine">Whether the figurine character is desired</param>
	///<returns>The notation character of the king</returns>
	public override string GetCharacter(bool wantFigurine) {
		return wantFigurine ? _FIGURINE_CHARACTER : _STANDARD_CHARACTER;
	}

	///<summary>
	///Returns whether the pawn can be legally promoted
	///</summary>
	///<param name="sqr">The square the pawn is on</param>
	///<returns>Whether the pawn can be legally promoted</returns>
	public bool CanBePromoted(Square sqr) {
		//if the pawn hasn't reached the 8th rank for white or 1st rank for black, return cannot be promoted
		if (IsWhite ? (sqr.Coords.z < 8) : (sqr.Coords.z > 1)) return false;

		//if the pawn is at the 9th rank for white or 0th rank for black, return can be promoted
		if (sqr.Coords.z == (IsWhite ? 9 : 0)) return true;

		//if the pawn is on the z or e file, return cannot be promoted
		if (sqr.Coords.x % 5 == 0) return false;

		//if the pawn is on the b or c file, return can be promoted
		if (sqr.Coords.x == 2 || sqr.Coords.x == 3) return true;

		//return whether there isn't an attack board square directly infront of the pawn
		return ChessBoard.Instance.GetSquareAt(new Vector3Int(sqr.Coords.x, IsWhite ? 5 : 1, IsWhite ? 9 : 0)) == null;
	}

	///<summary>
	///Promotes the pawn to a new piece
	///</summary>
	///<param name="promotion">The type of piece to promote to (Queen, Rook, Bishop, or Knight)</param>
	///<returns>The new promoted piece</returns>
	public ChessPiece Promote(PieceType promotion) {
		ChessPiece newPiece = PieceCreator.Instance.ConvertPiece(this, promotion);

		//remove the new piece from the captured pieces
		CapturedPiecesController.Instance.RemovePieceOfType(promotion, !IsWhite);

		//set the pawn as captured
		SetCaptured();

		//return the new piece
		return newPiece;
	}

	///<summary>
	///Sets the position of the pawn's gameobject
	///</summary>
	///<param name="startSqr">The square the pawn is currently on</param>
	///<param name="endSqr">The square to move the pawn to</param>
	public void MovePiece(Square startSqr, Square endSqr) {
		if (Math.Abs(startSqr.Coords.z - endSqr.Coords.z) == 2) JustMadeDSMove = true;
		MoveTo(endSqr);
	}

	///<summary>
	///Returns a string of data about the pawn
	///</summary>
	///<returns>A string of data about the pawn</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		str.Append("\nJust Made Double Move? ").Append(JustMadeDSMove);
		return str.ToString();
	}
}
