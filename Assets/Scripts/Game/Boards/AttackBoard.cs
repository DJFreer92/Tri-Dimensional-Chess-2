using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[DisallowMultipleComponent]
public sealed class AttackBoard : Board, IMovable {
	//holds if the owner of the attackboard is white
	[field: SerializeField] public Ownership Owner {get; private set;}
	//the square the attackboard is pinned to
	[field: SerializeField] public Square PinnedSquare {get; private set;}

	///<summary>
	///Sets where the owner of the attackboard is white
	///</summary>
	///<param name="isWhite">Whether the owner is white</param>
	public void SetOwner(Ownership owner) {
		Owner = owner;
	}

	///<summary>
	///Sets the square the attackboard is pinned to
	///</summary>
	///<param name="pin">The square the attackboard is pinned to</param>
	public void SetPinnedSquare(Square pin) {
		PinnedSquare = pin;
	}

	///<summary>
	///Returns a list of all the attackboard's available moves
	///</summary>
	///<param name="asWhite">Whether the attackboard is being moved by white</param>
	///<returns>A list of all the attackboard's available moves</returns>
	public List<Square> GetAvailableMoves(bool asWhite) {
		var moves = new List<Square>();
		List<ChessPiece> pieces = GetPieces();
		if (pieces.Count > 1) return moves;
		if (pieces.Count == 1 && pieces[0].IsWhite != asWhite) return moves;
		if (pieces.Count == 0 && Owner != Ownership.Neutral && (asWhite ? Ownership.White : Ownership.Black) != Owner) return moves;
		foreach (Board brd in ChessBoard.Instance) {
			if (brd is AttackBoard || Math.Abs(brd.Y - PinnedSquare.Coords.y) > 2) continue;
			foreach (Square sqr in brd) {
				if (!sqr.HasAttackBoardPin || sqr.IsOccupiedByAB) continue;
				int xDiff = sqr.Coords.x - PinnedSquare.Coords.x;
				int zDiff = sqr.Coords.z - PinnedSquare.Coords.z;
				if (pieces.Count == 1 && (Math.Sign(zDiff) + (asWhite ? 1 : -1) == 0)) continue;
				if (xDiff != 0 && zDiff != 0) continue;
				if (Math.Abs(zDiff) > 4) continue;
				if (!King.WillBeInCheck(new AttackBoardMove(Game.Instance.GetPlayer(asWhite), Squares[0], sqr))) moves.Add(sqr);
			}
		}
		return moves;
	}

	///<summary>
	///Moves the attackboard
	///</summary>
	///<param name="move">The attackboard move</param>
	///<returns>Whether the move was successful</returns>
	public void Move(AttackBoardMove move) {
		int xDiff = 0, zDiff = 0;

		//calculate the change in x and z positions
		if (Math.Abs(move.EndSqr.Coords.x - move.StartSqr.Coords.x) > 1) {  //if the board is moving in the x direction
			//calculate the change in x position
			xDiff = Math.Sign(move.EndSqr.Coords.x - move.StartSqr.Coords.x) * 4;
		} else {  //the board is moving in the z direction
			//calculate the change in z position
			zDiff = (move.EndSqr.Coords.y == PinnedSquare.Coords.y) ? 4 : 2;
			zDiff *= Math.Sign(move.EndSqr.Coords.z - move.StartSqr.Coords.z);
		}

		//get the pawn on the board, if there is one
		Pawn pawn = null;
		foreach (Square square in Squares) {
			pawn = square.GamePiece as Pawn;
			if (pawn != null) break;
		}

		//if the user hasn't been asked what piece to promote a pawn to
		if (pawn != null && move.Promotion == null) {
			//get the square the pawn will be on when the board moves
			Square newSquare = ChessBoard.Instance.GetSquareWithPiece(pawn).Clone() as Square;
			newSquare.Coords = new Vector3Int(newSquare.Coords.x + xDiff, move.EndSqr.Coords.y + 1, newSquare.Coords.z + zDiff);

			//if the move is a pawn promotion
			if (pawn.CanBePromoted(move, newSquare)) {
				//ask the user what piece to promote to, then wait
				Game.Instance.StartCoroutine(move.GetPromotionChoice());
			}
		}

		//move all the Squares on the attackboard to their new positions
		foreach (Square sqr in Squares) {
			sqr.Coords = new Vector3Int(sqr.Coords.x + xDiff, move.EndSqr.Coords.y + 1, sqr.Coords.z + zDiff);
			Debug.Log("Moved square to: " + sqr.Coords.ToString());
		}

		//change the y value of the attackboard
		if (move.EndSqr.Coords.y != Y) Y = move.EndSqr.Coords.y + 1;

		//calculate the displacement of the attackboard from the end square in the x and z directions
		float xDisplace = 0.5f, zDisplace = 0.5f;
		if (move.EndSqr.Coords.x == 1) xDisplace *= -1;
		if (move.EndSqr.Coords.z % 2 == 1) zDisplace *= -1;

		//change the position of the attackboard
		transform.position = move.EndSqr.transform.position + new Vector3(xDisplace, 1.5f, zDisplace);

		//move the pieces to the new location of their Squares
		UpdatePiecePositions();

		//set the current pinned square as unoccupied
		PinnedSquare.IsOccupiedByAB = false;

		//set the end square as the new pinned square and set as occupied
		PinnedSquare = move.EndSqr;
		PinnedSquare.IsOccupiedByAB = true;

		//if the piece is not a pawn or it cannot be promoted, return board move successful
		if (move.Promotion == null) return;

		//execute the promotion
		pawn.Promote(move.Promotion);

		//mark the move as having made a promotion
		move.MoveEvents.Add(MoveEvent.PROMOTION);

		//finish the promotion move
		Game.Instance.FinishTurn(move);
	}

	///<summary>
	///Update the positions of all the pieces on the attackboard
	///</summary>
	private void UpdatePiecePositions() {
		foreach (Square sqr in Squares) {
			if (!sqr.HasPiece()) continue;
			sqr.GamePiece.transform.position = sqr.transform.position;
			(sqr.GamePiece as ICastlingRights)?.RevokeCastlingRights();
			(sqr.GamePiece as Pawn)?.RevokeDSMoveRights();
		}
	}

	///<summary>
	///Returns a string of data about the object
	///</summary>
	///<returns>A string of data about the object</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		str.Append("\nOwner: ").Append(Owner);
		return str.ToString();
	}
}