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
		if (Promotion == PieceType.NONE && PieceMoved is Pawn && (PieceMoved as Pawn).CanBePromoted(this, EndSqr)) {
			//ask the user what piece to promote to, then wait
			Game.Instance.StartCoroutine(GetPromotionChoice());
		}

		//move the piece
		if (((PieceMoved is King && EndSqr.GamePiece is Rook) || (PieceMoved is Rook && EndSqr.GamePiece is King)) && PieceMoved.IsWhite == EndSqr.GamePiece.IsWhite) {  //castling move
			//perform castling
			Castle();
			return;
		}

		if (EndSqr.HasPiece()) {  //if a piece is being captured
			//find the piece being captured
			PieceCaptured = EndSqr.GamePiece;
			MoveEvents.Add(MoveEvent.CAPTURE);
		}

		if (PieceMoved is Pawn) {  //pawn move
			//if the move is a double square move
			if (Math.Abs(StartSqr.Coords.z - EndSqr.Coords.z) == 2) MoveEvents.Add(MoveEvent.PAWN_DOUBLE_SQUARE);
			//if the move is an en passant
			if (!EndSqr.HasPiece() && StartSqr.Coords.x != EndSqr.Coords.x) {
				//find the piece being captured
				PieceCaptured = Pawn.GetJMDSMPawnBehind(EndSqr, Player.IsWhite);

				//find the square of the piece being catured
				EnPassantCaptureSqr = PieceCaptured.GetSquare();

				//unassign piece captured in en passant from its square
				EnPassantCaptureSqr.GamePiece = null;

				MoveEvents.Add(MoveEvent.EN_PASSANT);
			}
			(PieceMoved as Pawn).MovePiece(StartSqr, EndSqr);
			(PieceMoved as Pawn).RevokeDSMoveRights();
		} else {  //regular piece move
			PieceMoved.MoveTo(EndSqr);
			if (PieceMoved is King) (PieceMoved as King).HasCastlingRights = false;
			if (PieceMoved is Rook) (PieceMoved as Rook).HasCastlingRights = false;
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
	}

	///<summary>
	///Undoes the move
	///</summary>
	///<param name="fullUndo">Whether to do a full undo or only a partial undo</param>
	public override void Undo() {
		//if move was king side castle
		if (MoveEvents.PartialContains(MoveEvent.CASTLING)) {
			//get the starting squares for the king and rook
			Square kingSqr;
			if (MoveEvents.Contains(MoveEvent.CASTLING_KING_SIDE)) {
				kingSqr = ChessBoard.Instance.GetSquareAt(Player.IsWhite ? ChessBoard.WhiteKingSideRookCoords : ChessBoard.BlackKingSideRookCoords);
			} else {
				kingSqr = ChessBoard.Instance.GetSquareAt(Player.IsWhite ? ChessBoard.WhiteQueenSideRookCoords : ChessBoard.BlackQueenSideRookCoords);
			}
			Square rookSqr = ChessBoard.Instance.GetSquareAt(Player.IsWhite ? ChessBoard.WhiteKingCoords : ChessBoard.BlackKingCoords);

			//get the king and the rook
			King king = kingSqr.GamePiece as King;
			Rook rook = rookSqr.GamePiece as Rook;

			//move the king and rook to their starting squares
			king.MoveTo(rookSqr);
			rook.MoveTo(kingSqr);

			//assign the king and the rook to their starting squares
			kingSqr.GamePiece = rook;
			rookSqr.GamePiece = king;

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
			PieceCaptured.MoveTo(EnPassantCaptureSqr);
			EnPassantCaptureSqr.GamePiece = PieceCaptured;

			//update whether the king is in check
			ChessBoard.Instance.UpdateKingCheckState(Player.IsWhite);
			return;
		}

		//if the move was a double square pawn move
		if (MoveEvents.Contains(MoveEvent.PAWN_DOUBLE_SQUARE)) (PieceMoved as Pawn).JustMadeDSMove = false;

		//if move was a capture
		if (MoveEvents.Contains(MoveEvent.CAPTURE)) {
			PieceCaptured.SetUncaptured();
			PieceCaptured.MoveTo(EndSqr);
			EndSqr.GamePiece = PieceCaptured;
		}

		//if move was a promotion
		if (MoveEvents.Contains(MoveEvent.PROMOTION)) PieceMoved = PieceMoved.ConvertTo(PieceType.PAWN);

		//update whether the king is in check
		ChessBoard.Instance.UpdateKingCheckState(Player.IsWhite);
	}

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

		//move the rook to the king's position
		rook.MoveTo(kingSqr);

		//if king side castling
		if (rook.IsKingSide) {
			//move the king to the rook's position
			king.MoveTo(rookSqr);

			//assign the pieces to their new squares
			kingSqr.GamePiece = rook;
			rookSqr.GamePiece = king;

			king.HasCastlingRights = false;
			rook.HasCastlingRights = false;

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

		king.HasCastlingRights = false;
		rook.HasCastlingRights = false;
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