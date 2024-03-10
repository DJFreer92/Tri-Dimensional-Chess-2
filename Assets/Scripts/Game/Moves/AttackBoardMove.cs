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
		//if there was an opponent pawn promotion
		if (MoveEvents.Contains(MoveEvent.OPPONENT_PROMOTION)) {
			//promote the opponent's pawn
			(StartPinSqr.GamePiece as Pawn).Promote(OpponentPromotion);
			return;
		}

		//make board move
		BoardMoved.Move(this);

		//if there is not an opponent pawn on the square the attack board was pinned to, return
		if (StartPinSqr.GamePiece.Type != PieceType.PAWN || StartPinSqr.GamePiece.IsWhite == Player.IsWhite) return;
		//if the pawn is not at the end of the board, return
		if (Player.IsWhite ? StartPinSqr.Coords.z != 1 : StartPinSqr.Coords.z != 8) return;

		//promote the opponent's pawn
		MoveEvents.Add(MoveEvent.OPPONENT_PROMOTION);
		Game.Instance.StartCoroutine(GetPromotionChoice(true));
	}

	///<summary>
	///Undoes the move
	///</summary>
	public override void Undo() {
		BoardMoved.Unmove(this);
	}

	///<summary>
	///Returns the annotation in long 3D chess algebraic notation
	///</summary>
	///<returns>The annotation</returns>
	public override string GetAnnotation() {
		var move = new StringBuilder();
		//the staring board level - the ending board level
		move.Append(_boardMovedAnnotationAtStart).Append("-").Append(EndSqr.Coords.VectorToBoard());

		//had a pawn promotion
		if (MoveEvents.Contains(MoveEvent.PROMOTION)) move.Append(BoardMoved.GetPieces()[0].GetCharacter(UseFigurineNotation));

		return move.Append(base.GetAnnotation()).ToString();
	}
}