using UnityEngine;
using System.Text;

public sealed class AttackBoardMove : Move {
	//the attackboard being moved
	public AttackBoard BoardMoved {get; private set;}
	//the square the board moved was pinned to at the start of the move
	public Square StartPinSqr {get; private set;}
	//the annotation of the attackboard being moved before it has moved
	private readonly string _boardMovedAnnotationAtStart;

	public AttackBoardMove(Player player, Square start, Square end) : base(player, start, end) {
		BoardMoved = ChessBoard.Instance.GetBoardWithSquare(start) as AttackBoard;
		StartPinSqr = BoardMoved.PinnedSquare;
		_boardMovedAnnotationAtStart = start.Coords.VectorToBoard();
	}

	///<summary>
	///Executes the move
	///</summary>
	public override void Execute() {
		//if there was a secondary promotion
		if (MoveEvents.Contains(MoveEvent.SECONDARY_PROMOTION)) {
			//promote the pawn
			(StartPinSqr.GamePiece as Pawn).Promote(SecondaryPromotion);
			return;
		}

		//make board move
		BoardMoved.Move(this);

		if (!CausesSecondaryPromotion()) return;

		//promote the pawn
		MoveEvents.Add(MoveEvent.SECONDARY_PROMOTION);
		Game.Instance.StartCoroutine(GetPromotionChoice(true));
	}

	///<summary>
	///Undoes the move
	///</summary>
	public override void Undo() {
		//if there was a secondary promotion
		if (MoveEvents.Contains(MoveEvent.SECONDARY_PROMOTION)) {
			//unpromote the piece
			ChessPiece piece = StartPinSqr.GamePiece;
			StartPinSqr.GamePiece.Unpromote();
			Game.Destroy(piece.gameObject);
		}
		BoardMoved.Unmove(this);
	}

    ///<summary>
    ///Redoes the move
    ///</summary>
    public override void Redo() {
		//if there was an opponent pawn promotion, promote the opponent's pawn
		if (MoveEvents.Contains(MoveEvent.SECONDARY_PROMOTION))
			(StartPinSqr.GamePiece as Pawn).Promote(SecondaryPromotion);

		//make board move
		BoardMoved.Move(this);
    }

    ///<summary>
    ///Returns the annotation in long 3D chess algebraic notation
    ///</summary>
    ///<returns>The annotation</returns>
    public override string GetAnnotation() {
		var move = new StringBuilder();
		//the starting board level - the ending board level
		move.Append(_boardMovedAnnotationAtStart).Append("-").Append(BoardMoved.Squares[0].Coords.VectorToBoard());

		//had a pawn promotion
		if (MoveEvents.Contains(MoveEvent.PROMOTION)) move.Append(BoardMoved.GetPieces()[0].GetCharacter(UseFigurineNotation));

		return move.Append(base.GetAnnotation()).ToString();
	}

	///<summary>
	///Returns whether the attack board move will cause a secondary promotion
	///</summary>
	///<returns>Whether the attack board move will cause a secondary promotion</returns>
	public bool CausesSecondaryPromotion() {
		return StartPinSqr.HasPiece() &&  //the square the attackboard is pinned to has a piece
			   StartPinSqr.GamePiece.Type == PieceType.PAWN &&  //the piece is a pawn
			   StartPinSqr.Coords.z == (StartPinSqr.GamePiece.IsWhite ? 8 : 1);  //the piece is at the opposite end of the board form its starting position
	}
}