using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using DG.Tweening;

[DisallowMultipleComponent]
public sealed class AttackBoard : Board, IMovable {
	private const float _TWEEN_SPEED = 0.4f;
	private const float _UP_ARC_HEIGHT = 1.5f;
	private const float _DOWN_ARC_HEIGHT = 4f;
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
	public void SetPinnedSquare(Square pin) => PinnedSquare = pin;

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
		Pawn pawn = GetSinglePiece() as Pawn;

		//if the user hasn't been asked what piece to promote a pawn to
		if (pawn != null && move.Promotion == PieceType.NONE) {
			//get the square the pawn will be on when the board moves
			Square newSquare = pawn.GetSquare().Clone() as Square;
			newSquare.Coords = new Vector3Int(newSquare.Coords.x + xDiff, move.EndSqr.Coords.y + 1, newSquare.Coords.z + zDiff);
			//move all the squares on the attack board to their new positions
			foreach (Square sqr in Squares)
				sqr.Coords = new Vector3Int(sqr.Coords.x + xDiff, move.EndSqr.Coords.y + 1, sqr.Coords.z + zDiff);

			bool canBePromoted = pawn.CanBePromoted(newSquare);

			//move all the squares on the attack board back to their current positions
			foreach (Square sqr in Squares)
				sqr.Coords = new Vector3Int(sqr.Coords.x - xDiff, move.StartPinSqr.Coords.y + 1, sqr.Coords.z - zDiff);

			//if the move is a pawn promotion, ask the user what piece to promote to, then wait
			if (canBePromoted) {
				Game.Instance.StartCoroutine(move.GetPromotionChoice());
				throw new Exception("Must wait for promotion choice");
			}
		}

		//change the y value of the attack board
		Y = move.EndSqr.Coords.y + 1;

		//move all the Squares on the attack board to their new positions
		foreach (Square sqr in Squares)
			sqr.Coords = new Vector3Int(sqr.Coords.x + xDiff, Y, sqr.Coords.z + zDiff);

		//calculate the displacement of the attack board from the end square in the x and z directions
		float xDisplace = 0.5f, zDisplace = 0.5f;
		if (move.EndSqr.Coords.x == 1) xDisplace *= -1;
		if (move.EndSqr.Coords.z % 2 == 1) zDisplace *= -1;

		//change the position of the attackboard
		MoveTo(move.EndSqr.transform.position + new Vector3(xDisplace, 1.5f, zDisplace));

		//if there is a piece on the attack board, set it as moved
		GetSinglePiece()?.SetMoved(move);

		//set the current pinned square as unoccupied
		PinnedSquare.IsOccupiedByAB = false;

		//set the end square as the new pinned square and set as occupied
		PinnedSquare = move.EndSqr;
		PinnedSquare.IsOccupiedByAB = true;

		//set the notation of the attackboard
		SetBoardNotation();

		//if there is not a pawn promotion
		if (move.MoveEvents.Contains(MoveEvent.SECONDARY_PROMOTION) || move.Promotion == PieceType.NONE) return;

		//execute the promotion
		if (move.PromotionUndoRedoHolder == null) {
			move.PromotionUndoRedoHolder = pawn;
			pawn.Promote(move.Promotion);
		} else {
			ChessPiece promotedPiece = move.PromotionUndoRedoHolder;
			move.PromotionUndoRedoHolder = pawn;
			pawn.SetCaptured();
			promotedPiece.SetUncaptured();
			pawn.GetSquare().GamePiece = promotedPiece;
		}

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
		//get the piece on the board, if there is one
		ChessPiece piece = GetSinglePiece();

		//if there was a promotion
		if (move.MoveEvents.Contains(MoveEvent.PROMOTION)) {
			//unpromote the piece
			move.PromotionUndoRedoHolder.SetUncaptured();
			piece.SetCaptured();
			piece.GetSquare().GamePiece = move.PromotionUndoRedoHolder;
			(move.PromotionUndoRedoHolder, piece) = (piece, move.PromotionUndoRedoHolder);
		}

		//calculate the change in x and z positions
		int xDiff = 0, zDiff = 0;
		if (Math.Abs(move.EndSqr.Coords.x - move.StartPinSqr.Coords.x) > 1) {  //if the board is moving in the x direction
			//calculate the change in x position
			xDiff = Math.Sign(move.StartPinSqr.Coords.x - move.EndSqr.Coords.x) * 4;
		} else {  //the board is moving in the z direction
			//calculate the change in z position
			zDiff = (move.StartPinSqr.Coords.y == PinnedSquare.Coords.y) ? 4 : 2;
			zDiff *= Math.Sign(move.StartPinSqr.Coords.z - move.EndSqr.Coords.z);
		}

		//change the y value of the attackboard
		Y = move.StartPinSqr.Coords.y + 1;

		//move all the squares on the attackboard to their old positions
		foreach (Square sqr in Squares)
			sqr.Coords = new Vector3Int(sqr.Coords.x + xDiff, Y, sqr.Coords.z + zDiff);

		//calculate the displacement of the attackboard from the end square in the x and z directions
		float xDisplace = 0.5f, zDisplace = 0.5f;
		if (move.StartPinSqr.Coords.x == 1) xDisplace *= -1;
		if (move.StartPinSqr.Coords.z % 2 == 1) zDisplace *= -1;

		//change the position of the attackboard
		MoveTo(move.StartPinSqr.transform.position + new Vector3(xDisplace, 1.5f, zDisplace));

		//if there is a piece on the attackboard update its rights
		if (piece != null) {
			if (move.MoveEvents.Contains(MoveEvent.LOST_CASTLING_RIGHTS)) {
				if (piece is King) (piece as King).HasCastlingRights = true;
				else (piece as Rook).HasCastlingRights = true;
				return;
			}

			if (move.MoveEvents.Contains(MoveEvent.LOST_DOUBLE_SQUARE_MOVE_RIGHTS))
				(piece as Pawn).HasDSMoveRights = true;
		}

		//set the current pinned square as unoccupied
		PinnedSquare.IsOccupiedByAB = false;

		//set the end square as the new pinned square and set as occupied
		PinnedSquare = move.StartPinSqr;
		PinnedSquare.IsOccupiedByAB = true;

		//set the notation of the attackboard
		SetBoardNotation();

		//update whether the king is in check
		ChessBoard.Instance.UpdateKingCheckState(move.Player.IsWhite);
	}

	///<summary>
	///Moves the attack board to the given position
	///</summary>
	///<param name="pos">The position to move the attackboard to</param>
	private void MoveTo(Vector3 pos) {
		if (!Game.Instance.AllowMoves) {
			MoveInstant(pos);
			return;
		}

		if (pos.y != transform.position.y) {
			MoveInArc(pos);
			return;
		}

		MoveInLine(pos);
	}

	///<summary>
	///Moves the attack board instantly to the given position
	///</summary>
	///<param name="pos">The position to move the piece to</param>
	private void MoveInstant(Vector3 pos) => transform.position = pos;

	///<summary>
	///Moves the attack board in a straight line to the given position
	///</summary>
	///<param name="pos">The position to move the piece to</param>
	private void MoveInLine(Vector3 pos) => transform.DOMove(pos, _TWEEN_SPEED);

	///<summary>
	///Moves the attack board in an arc to the given position
	///</summary>
	///<param name="pos">The position to move the piece to</param>
	private void MoveInArc(Vector3 pos) {
		float height = pos.y > transform.position.y ? _UP_ARC_HEIGHT : _DOWN_ARC_HEIGHT;
		transform.DOJump(pos, height, 1, _TWEEN_SPEED);
	}

	///<summary>
	///Rotates the attack board and a piece on it 180 degrees
	///</summary>
	///<param name="move">The attack board move</param>
	public void Rotate(AttackBoardMove move) {
		//rotate the attackboard gameobject
		transform.DORotate((Quaternion.Euler(0f, 180f, 0f) * transform.rotation).eulerAngles, _TWEEN_SPEED);

		//get the piece on the board
		ChessPiece piece = GetSinglePiece();

		//if there is a piece on the attack board, counter-rotate the piece
		if (piece != null) piece.transform.DORotate(piece.transform.rotation.eulerAngles, _TWEEN_SPEED);

		//set the piece as moved
		piece.SetMoved(move);

		//update the coordinates of the squares on the attack board
		foreach (Square sqr in Squares)
			sqr.Coords += (sqr.Coords.x % 2 == 0 ? Vector3Int.right : Vector3Int.left) +
						  (sqr.Coords.z % 2 == 0 ? Vector3Int.forward : Vector3Int.back);
	}

	///<summary>
	///Returns the first piece found on the attack board
	///</summary>
	///<returns>The first piece found on the attack board</returns>
	public ChessPiece GetSinglePiece() {
		//if there is a piece on the attack board, return the piece
		foreach (Square sqr in Squares)
			if (sqr.HasPiece()) return sqr.GamePiece;

		return null;
	}

	///<summary>
	///Sets the notation of the board
	///</summary>
	public override void SetBoardNotation() => Notation = Squares[0].Coords.VectorToBoard();

	///<summary>
	///Selects the attackboard
	///</summary>
	public void Select() {
		Square square = null;
		foreach (Square sqr in this)
			if (!sqr.HasPiece()) square = sqr;
		if (square != null) Game.Instance.SelectSquare(square);
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
