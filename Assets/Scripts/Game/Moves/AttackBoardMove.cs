using UnityEngine;
using System.Text;

public sealed class AttackBoardMove : Move {
	//the attackboard being moved
	public AttackBoard BoardMoved {get; private set;}
	//the annotation of the attackboard being moved before it has moved
	private readonly string _boardMovedAnnotationAtStart;

	public AttackBoardMove(Player player, Square start, Square end) : base(player, start, end) {
		BoardMoved = ChessBoard.Instance.GetBoardWithSquare(start) as AttackBoard;
		_boardMovedAnnotationAtStart = start.Coords.VectorToBoard();
	}

	///<summary>
	///Executes the move
	///</summary>
	public override void Execute() {
		BoardMoved.Move(this);
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