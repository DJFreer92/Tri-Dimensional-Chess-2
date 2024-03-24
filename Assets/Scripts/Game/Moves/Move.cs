using System;
using System.Collections;
using UnityEngine;
using System.Text;

public abstract class Move : ICommand {
	//the player making the move
	public readonly Player Player;
	//the departure and arrival squares
	public readonly Square StartSqr, EndSqr;
	//what events occured during the move
	public MoveEvent MoveEvents;
	//whether the move is to be formatted in figurine notation
	public bool UseFigurineNotation;
	//what type of piece the user has selected to promote a pawn to
	public PieceType Promotion = PieceType.NONE;
	//what type of piece the secondary promotion is to
	public PieceType SecondaryPromotion = PieceType.NONE;
	//the square the pawn being en passant is on
	protected Square _enPassantCaptureSqr;

	public Move(Player player, Square start, Square end) {
		Player = player;
		StartSqr = start;
		EndSqr = end;
		UseFigurineNotation = SettingsManager.Instance.FigurineNotation;
	}

	///<summary>
	///Executes the move
	///</summary>
	public abstract void Execute();

	///<summary>
	///Undoes the move
	///</summary>
	public abstract void Undo();

	///<summary>
	///Redoes the move
	///</summary>
	public abstract void Redo();

	///<summary>
	///Build the move from the given annotation
	///</summary>
	public static Move BuildMove(string annotatedMove, bool isWhite) {
		Move move;
		int seperationIndex;

		if (annotatedMove.Contains("O-O")) {
			Square rookSqr;

			if (annotatedMove.Contains("O-O-O")) {
				rookSqr = isWhite ?
					ChessBoard.Instance.GetSquareAt(ChessBoard.WhiteQueenSideRookCoords) :
					ChessBoard.Instance.GetSquareAt(ChessBoard.BlackQueenSideRookCoords);
			} else {
				rookSqr = isWhite ?
					ChessBoard.Instance.GetSquareAt(ChessBoard.WhiteKingSideRookCoords) :
					ChessBoard.Instance.GetSquareAt(ChessBoard.BlackKingSideRookCoords);
			}

			move = new PieceMove(
				Game.Instance.GetPlayer(isWhite),
				isWhite ?
					ChessBoard.Instance.GetSquareAt(ChessBoard.WhiteKingCoords) :
					ChessBoard.Instance.GetSquareAt(ChessBoard.BlackKingCoords),
				rookSqr
			);
		} else if (annotatedMove.Contains('-')) {
			seperationIndex = annotatedMove.IndexOf('-');

			move = new AttackBoardMove(
				Game.Instance.GetPlayer(isWhite),
				ChessBoard.Instance.GetSquareAt(annotatedMove[..seperationIndex].BoardToVector() + Vector3Int.up),
				ChessBoard.Instance.GetSquareAt(annotatedMove[(seperationIndex + 1)..].BoardToVector())
			);
		} else {
			seperationIndex = annotatedMove.IndexOf('x');
			if (seperationIndex == -1)
				for (int i = 2; i < annotatedMove.Length; i++)
					if (char.IsLower(annotatedMove[i])) seperationIndex = i;

			move = new PieceMove(
				Game.Instance.GetPlayer(isWhite),
				ChessBoard.Instance.GetSquareAt(
					annotatedMove[(char.IsUpper(annotatedMove[0]) ? 1 : 0)..seperationIndex].AnnotationToVector()
				),
				ChessBoard.Instance.GetSquareAt(
					annotatedMove[(seperationIndex + (annotatedMove.Contains('x') ? 1 : 0))..].AnnotationToVector()
				)
			);
		}

		if (annotatedMove.Contains('x')) move.MoveEvents.Add(MoveEvent.CAPTURE);
		if (annotatedMove.Contains("e.p.")) move.MoveEvents.Add(MoveEvent.EN_PASSANT);
		else if (annotatedMove.Contains('=') || (annotatedMove.Contains('-') && !annotatedMove.Contains("O-O") && char.IsUpper(annotatedMove[^1]))) {
			move.MoveEvents.Add(MoveEvent.PROMOTION);
			move.Promotion = annotatedMove[^1].CharToPiece();
		}
		if (annotatedMove.Contains('#')) move.MoveEvents.Add(MoveEvent.CHECK, MoveEvent.CHECKMATE);
		else if (annotatedMove.Contains('+')) {
			move.MoveEvents.Add(MoveEvent.CHECK);
			ChessBoard.Instance.GetKing(!isWhite).IsInCheck = true;
		}

		return move;
	}

	///<summary>
	///Requests for the user to choose a chess piece to promote to and waits until a choice is made
	///</summary>
	///<param name="isSecondaryPromotion">Whether the promotion is for the opponent's pawn
	public IEnumerator GetPromotionChoice(bool isSecondaryPromotion = false) {
		//stop move from occuring while the promotion is in progress
		Game.Instance.AllowMoves = false;
		Game.Instance.AllowButtons = false;

		//display the promotion options
		PromotionController.Instance.ShowPromotionOptions(
			this,
			isSecondaryPromotion ? (this as AttackBoardMove).StartPinSqr.GamePiece.IsWhite : Player.IsWhite,
			isSecondaryPromotion
		);

		//wait for the player to make a selection
		Debug.Log("Waiting for promotion selection...");
		yield return new WaitUntil(() => !PromotionController.Instance.SelectionInProgress);
		Debug.Log("Promotion selection recieved.");

		//execute the promotion
		Execute();

		//allow moves to be made
		Game.Instance.AllowMoves = true;
		Game.Instance.AllowButtons = true;
	}

	///<summary>
	///Returns the annotation of the move in long 3D chess algebraic notation<br/>
	///NOTE: Base implementation should only be called by an overriding implementation
	///</summary>
	///<returns>The annotation of the move</returns>
	public virtual string GetAnnotation() {
		var move = new StringBuilder();
		if (MoveEvents.Contains(MoveEvent.DRAW_OFFERED)) move.Append("=");  //draw offered
		if (MoveEvents.Contains(MoveEvent.CHECKMATE)) move.Append("#");  //checkmate
		else if (MoveEvents.Contains(MoveEvent.CHECK)) move.Append("+");  //check
		return move.ToString();
	}

	///<summary>
	///Returns data about the move
	///</summary>
	///<returns>Data about the move</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		str.Append("\nAnnotation: ").Append(GetAnnotation());
		str.Append("\nStartSqr Square: ").Append(StartSqr.ToString());
		str.Append("\nEndSqr Square: ").Append(EndSqr.ToString());
		str.Append("\nIs Attackboard Move? ").Append(this is AttackBoardMove);
		str.Append("\nEvents: ").Append(MoveEvents.ToString());
		return str.ToString();
	}
}