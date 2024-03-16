using System;
using UnityEngine;
using System.Text;

public sealed class PieceMove : Move {
	//the piece being moved
	public ChessPiece PieceMoved {get; private set;}
	//the piece being captured
	public ChessPiece PieceCaptured {get; private set;}

	public PieceMove(Player player, Square start, Square end) : base(player, start, end) {
		PieceMoved = start.GamePiece;
	}

	///<summary>
	///Executes the move
	///</summary>
	public override void Execute() {
		//if the move is a pawn promotion
		if (Promotion == PieceType.NONE && PieceMoved is Pawn && (PieceMoved as Pawn).CanBePromoted(EndSqr)) {
			//ask the user what piece to promote to, then wait
			Game.Instance.StartCoroutine(GetPromotionChoice());
			throw new Exception("Must wait for promotion choice");
		}

		//move the piece
		if (((PieceMoved is King && EndSqr.GamePiece is Rook) || (PieceMoved is Rook && EndSqr.GamePiece is King)) && PieceMoved.IsWhite == EndSqr.GamePiece.IsWhite) {  //castling move
			//perform castling
			Castle();
			return;
		}

		//if a piece is being captured
		if (EndSqr.HasPiece()) {
			//find the piece being captured
			PieceCaptured = EndSqr.GamePiece;
			MoveEvents.Add(MoveEvent.CAPTURE);
		}

		//if the piece is moving to a neutral attackboard, claim it
		Board endBoard = EndSqr.GetBoard();
		if (endBoard is AttackBoard && endBoard.Owner == Ownership.NEUTRAL) {
			endBoard.Owner = Player.IsWhite ? Ownership.WHITE : Ownership.BLACK;
			MoveEvents.Add(MoveEvent.ATTACKBOARD_CLAIM);
		}

		if (PieceMoved is Pawn) {  //if the piece being moved is a pawn
			//if the move is a double square move
			if (Math.Abs(StartSqr.Coords.z - EndSqr.Coords.z) == 2) MoveEvents.Add(MoveEvent.PAWN_DOUBLE_SQUARE);
			else if (!EndSqr.HasPiece() && StartSqr.Coords.x != EndSqr.Coords.x) {  //if the move is an en passant
				//find the piece being captured
				PieceCaptured = Pawn.GetJMDSMPawnBehind(EndSqr, Player.IsWhite);

				//find the square of the piece being catured
				_enPassantCaptureSqr = PieceCaptured.GetSquare();

				//unassign piece captured in en passant from its square
				_enPassantCaptureSqr.GamePiece = null;

				MoveEvents.Add(MoveEvent.EN_PASSANT);
			}
			//if the pawn has double square move rights remove them
			if ((PieceMoved as Pawn).HasDSMoveRights)  {
				(PieceMoved as Pawn).HasDSMoveRights = false;
				MoveEvents.Add(MoveEvent.LOST_DOUBLE_SQUARE_MOVE_RIGHTS);
			}
			(PieceMoved as Pawn).MovePiece(StartSqr, EndSqr);
		} else {  //non-pawn move
			PieceMoved.MoveTo(EndSqr);
			if (PieceMoved is King && (PieceMoved as King).HasCastlingRights) {
				(PieceMoved as King).HasCastlingRights = false;
				MoveEvents.Add(MoveEvent.LOST_CASTLING_RIGHTS);
			} else if (PieceMoved is Rook && (PieceMoved as Rook).HasCastlingRights) {
				(PieceMoved as Rook).HasCastlingRights = false;
				MoveEvents.Add(MoveEvent.LOST_CASTLING_RIGHTS);
			}
		}

		//set the piece being captured as captured
		PieceCaptured?.SetCaptured();

		//move the piece from the starting square to the ending square
		EndSqr.GamePiece = PieceMoved;
		StartSqr.GamePiece = null;

		//if the piece is not a pawn or it cannot be promoted, finish the move execution
		if (Promotion == PieceType.NONE) return;

		//execute the promotion
		PieceMoved = (PieceMoved as Pawn).Promote(Promotion);

		//mark the move as having made a promotion
		MoveEvents.Add(MoveEvent.PROMOTION);

		//finish the turn
		Game.Instance.FinishTurn(this);
	}

	///<summary>
	///Undoes the move
	///</summary>
	public override void Undo() {
		//if move was king side castle
		if (MoveEvents.PartialContains(MoveEvent.CASTLING)) {
			//get the starting squares for the king and rook
			Square kingLandSqr, rookStartSqr = null;
			if (MoveEvents.Contains(MoveEvent.CASTLING_KING_SIDE)) {
				kingLandSqr = ChessBoard.Instance.GetSquareAt(
					Player.IsWhite ?
					ChessBoard.WhiteKingSideRookCoords :
					ChessBoard.BlackKingSideRookCoords
				);
			} else {
				kingLandSqr = ChessBoard.Instance.GetSquareAt(
					Player.IsWhite ?
					ChessBoard.WhiteQueenSideKingLandingCoords :
					ChessBoard.BlackQueenSideKingLandingCoords
				);
				rookStartSqr = ChessBoard.Instance.GetSquareAt(
					Player.IsWhite ?
					ChessBoard.WhiteQueenSideRookCoords :
					ChessBoard.BlackQueenSideRookCoords
				);
			}
			Square rookLandSqr = ChessBoard.Instance.GetSquareAt(
				Player.IsWhite ?
				ChessBoard.WhiteKingCoords :
				ChessBoard.BlackKingCoords
			);

			//get the king and rook
			King king = kingLandSqr.GamePiece as King;
			Rook rook = rookLandSqr.GamePiece as Rook;

			//move the king and rook to their starting squares
			king.MoveTo(rookLandSqr);
			rook.MoveTo(rookStartSqr ?? kingLandSqr);

			//assign the king and the rook to their starting squares
			(rookStartSqr ?? kingLandSqr).GamePiece = rook;
			rookLandSqr.GamePiece = king;
			if (rookStartSqr != null) kingLandSqr.GamePiece = null;

			//give the king and rook their castling rights back
			king.HasCastlingRights = true;
			rook.HasCastlingRights = true;
			return;
		}

		//move the piece back to its original position
		PieceMoved.MoveTo(StartSqr);
		StartSqr.GamePiece = PieceMoved;
		EndSqr.GamePiece = null;

		//if move was en passant
		if (MoveEvents.Contains(MoveEvent.EN_PASSANT)) {
			PieceCaptured.SetUncaptured();
			PieceCaptured.MoveTo(_enPassantCaptureSqr);
			_enPassantCaptureSqr.GamePiece = PieceCaptured;

			//update whether the king is in check
			ChessBoard.Instance.UpdateKingCheckState(Player.IsWhite);
			return;
		}

		if (PieceMoved is Pawn) {  //if it was a pawn move
			//if the move was a double square pawn move
			if (MoveEvents.Contains(MoveEvent.PAWN_DOUBLE_SQUARE)) (PieceMoved as Pawn).JustMadeDSMove = false;
			//if the pawn lost double square move rights, give them back
			if (MoveEvents.Contains(MoveEvent.LOST_DOUBLE_SQUARE_MOVE_RIGHTS)) (PieceMoved as Pawn).HasDSMoveRights = true;
		} else if (PieceMoved is King) {  //if it was a king move
			//if the king lost castling rights, give them back
			if (MoveEvents.Contains(MoveEvent.LOST_CASTLING_RIGHTS)) (PieceMoved as King).HasCastlingRights = true;
		} else if (PieceMoved is Rook) {  //if it was a rook move
			//if the rook lost castling rights, give them back
			if (MoveEvents.Contains(MoveEvent.LOST_CASTLING_RIGHTS)) (PieceMoved as Rook).HasCastlingRights = true;
		}

		//if move was a capture
		if (MoveEvents.Contains(MoveEvent.CAPTURE)) {
			PieceCaptured.SetUncaptured();
			EndSqr.GamePiece = PieceCaptured;
		}

		//if the move claimed an attackboard, unclaim it
		if (MoveEvents.Contains(MoveEvent.ATTACKBOARD_CLAIM)) EndSqr.GetBoard().Owner = Ownership.NEUTRAL;

		//if move was a promotion
		if (MoveEvents.Contains(MoveEvent.PROMOTION)) {
			Pawn pawn = PieceMoved.Unpromote();
			Game.Destroy(PieceMoved.gameObject);
			PieceMoved = pawn;
		}

		//update whether the king is in check
		ChessBoard.Instance.UpdateKingCheckState(Player.IsWhite);
	}

    ///<summary>
    ///Undoes the move
    ///</summary>
    public override void Redo() => Execute();

    ///<summary>
    ///Executes a castling move
    ///</summary>
    private void Castle() {
		//find whether the king is the piece that was selected to move
		bool kingIsPrimary = PieceMoved is King;

		//get the king and rook being moved
		King king = (kingIsPrimary ? PieceMoved : EndSqr.GamePiece) as King;
		Rook rook = (kingIsPrimary ? EndSqr.GamePiece : PieceMoved) as Rook;

		//get the squares of the king and rook being moved
		Square kingSqr = kingIsPrimary ? StartSqr : EndSqr;
		Square rookSqr = kingIsPrimary ? EndSqr : StartSqr;

		//remove the future castling rights of the king and rook
		king.HasCastlingRights = false;
		rook.HasCastlingRights = false;

		//move the rook to the king's position
		rook.MoveTo(kingSqr);

		//if king side castling
		if (rook.IsKingSide) {
			//move the king to the rook's position
			king.MoveTo(rookSqr);

			//assign the pieces to their new squares
			kingSqr.GamePiece = rook;
			rookSqr.GamePiece = king;

			MoveEvents.Add(MoveEvent.CASTLING_KING_SIDE);
			return;
		}

		//queen side castling
		//get the king's landing square
		Square kingLandingSqr = ChessBoard.Instance.GetSquareAt(rookSqr.Coords + Vector3Int.right);

		//move the king to its landing square
		king.MoveTo(kingLandingSqr);

		//assign the pieces to their new squares
		kingLandingSqr.GamePiece = king;
		kingSqr.GamePiece = rook;
		rookSqr.GamePiece = null;

		MoveEvents.Add(MoveEvent.CASTLING_QUEEN_SIDE);
	}

	///<summary>
	///Returns the annotation of the move in long 3D chess algebraic notation
	///</summary>
	///<returns>The annotation of the move</returns>
	public override string GetAnnotation() {
		//if the move was castling, O-O king side, O-O-O queen side
		if (MoveEvents.Contains(MoveEvent.CASTLING_KING_SIDE)) return "O-O" + base.GetAnnotation();
		if (MoveEvents.Contains(MoveEvent.CASTLING_QUEEN_SIDE)) return "O-O-O" + base.GetAnnotation();

		//the move was not castling
		var move = new StringBuilder(PieceMoved.GetCharacter(UseFigurineNotation));  //piece character
		move.Append(StartSqr.GetAnnotation());  //starting position
		if (MoveEvents.Contains(MoveEvent.CAPTURE)) move.Append("x"); //A piece was captured
		move.Append(EndSqr.GetAnnotation());  //ending position
		if (MoveEvents.Contains(MoveEvent.PROMOTION)) move.Append("=" + PieceMoved.GetCharacter(UseFigurineNotation));  //had a pawn promotion
		else if (MoveEvents.Contains(MoveEvent.EN_PASSANT)) move.Append(" e.p.");  //En Passant

		//set the move annotation
		return move.Append(base.GetAnnotation()).ToString();
	}

	///<summary>
	///Returns data about the piece move
	///</summary>
	///<returns>Data about the piece move</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		str.Append("\nPiece Moved: " + PieceMoved.ToString()).Append(PieceMoved.ToString());
		str.Append("\nPiece Captured: ").Append(PieceCaptured != null ? PieceCaptured.ToString() : "null");
		return str.ToString();
	}
}