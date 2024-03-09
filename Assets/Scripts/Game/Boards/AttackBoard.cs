using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[DisallowMultipleComponent]
public sealed class AttackBoard : Board, IMovable {
	//the square the attackboard is pinned to
	[field: SerializeField] public Square PinnedSquare {get; private set;}

	///<summary>
	///Clears the attackboard
	///</summary>
	public override void Clear() {
		base.Clear();
		Destroy(gameObject);
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
		if (Owner == Ownership.NEUTRAL) return moves;
		if (pieces.Count > 1) return moves;
		if (pieces.Count == 1 && pieces[0].IsWhite != asWhite) return moves;
		if (pieces.Count == 0 && (asWhite ? Ownership.WHITE : Ownership.BLACK) != Owner) return moves;
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
	public void Move(AttackBoardMove move) {
		//calculate the change in x and z positions
		int xDiff = 0, zDiff = 0;
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
		if (pawn != null && move.Promotion == PieceType.NONE) {
			//get the square the pawn will be on when the board moves
			Square newSquare = pawn.GetSquare().Clone() as Square;
			newSquare.Coords = new Vector3Int(newSquare.Coords.x + xDiff, move.EndSqr.Coords.y + 1, newSquare.Coords.z + zDiff);

			//if the move is a pawn promotion, ask the user what piece to promote to, then wait
			if (pawn.CanBePromoted(move, newSquare)) {
				Game.Instance.StartCoroutine(move.GetPromotionChoice());
				throw new Exception("Must wait for pormotion choice");
			}
		}

		//change the y value of the attackboard
		Y = move.EndSqr.Coords.y + 1;

		//move all the Squares on the attackboard to their new positions
		foreach (Square sqr in Squares)
			sqr.Coords = new Vector3Int(sqr.Coords.x + xDiff, Y, sqr.Coords.z + zDiff);

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

		//set the annotation of the attackboard
		SetBoardAnnotation();

		//if the piece is not a pawn or it cannot be promoted, return
		if (move.Promotion == PieceType.NONE) return;

		//execute the promotion
		pawn.Promote(move.Promotion);

		//mark the move as having made a promotion
		move.MoveEvents.Add(MoveEvent.PROMOTION);

		//finish the promotion move
		Game.Instance.FinishTurn(move);
	}

	///<summary>
	///Unmoves the attackboard
	///</summary>
	///<param name="move">Attackboard move to undo</param>
	public void Unmove(AttackBoardMove move) {
		//if there was a promotion
		if (move.MoveEvents.Contains(MoveEvent.PROMOTION)) {
			//get the piece that was promoted
			ChessPiece promotedPiece = null;
			foreach (Square sqr in Squares) {
				if (!sqr.HasPiece()) continue;
				promotedPiece = sqr.GamePiece;
				break;
			}

			//convert the promoted piece back into a pawn
			promotedPiece.ConvertTo(PieceType.PAWN);
		}

		//calculate the change in x and z positions
		int xDiff = 0, zDiff = 0;
		if (Math.Abs(move.EndSqr.Coords.x - move.StartPinSqr.Coords.x) > 1) {  //if the board is moving in the x direction
			//calculate the change in x position
			xDiff = Math.Sign(move.StartPinSqr.Coords.x - move.EndSqr.Coords.x) * 4;
		} else {  //the board is moving in the z direction
			//calculate the change in z position
			zDiff = (move.StartPinSqr.Coords.y + 1 == PinnedSquare.Coords.y) ? 4 : 2;
			zDiff *= Math.Sign(move.StartPinSqr.Coords.z - move.EndSqr.Coords.z);
		}

		//change the y value of the attackboard
		Y = move.StartPinSqr.Coords.y + 1;

		//move all the Squares on the attackboard to their old positions
		foreach (Square sqr in Squares)
			sqr.Coords = new Vector3Int(sqr.Coords.x + xDiff, Y, sqr.Coords.z + zDiff);

		//calculate the displacement of the attackboard from the end square in the x and z directions
		float xDisplace = 0.5f, zDisplace = 0.5f;
		if (move.StartPinSqr.Coords.x == 1) xDisplace *= -1;
		if (move.StartPinSqr.Coords.z % 2 == 1) zDisplace *= -1;

		//change the position of the attackboard
		transform.position = move.StartPinSqr.transform.position + new Vector3(xDisplace, 1.5f, zDisplace);

		//move the pieces to the new location of their Squares
		UpdatePiecePositions();

		//set the current pinned square as unoccupied
		PinnedSquare.IsOccupiedByAB = false;

		//set the end square as the new pinned square and set as occupied
		PinnedSquare = move.StartPinSqr;
		PinnedSquare.IsOccupiedByAB = true;

		//set the annotation of the attackboard
		SetBoardAnnotation();

		//update whether the king is in check
		ChessBoard.Instance.UpdateKingCheckState(move.Player.IsWhite);
	}

	///<summary>
	///Update the positions of all the pieces on the attackboard
	///</summary>
	private void UpdatePiecePositions() {
		foreach (Square sqr in Squares) {
			if (!sqr.HasPiece()) continue;
			sqr.GamePiece.transform.position = sqr.transform.position;
			if (sqr.GamePiece is King) (sqr.GamePiece as King).HasCastlingRights = false;
			if (sqr.GamePiece is Rook) (sqr.GamePiece as Rook).HasCastlingRights = false;
			(sqr.GamePiece as Pawn)?.RevokeDSMoveRights();
		}
	}

	///<summary>
	///Sets the annotation of the board
	///</summary>
	public override void SetBoardAnnotation() {
		Annotation = Squares[0].Coords.VectorToBoard();
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