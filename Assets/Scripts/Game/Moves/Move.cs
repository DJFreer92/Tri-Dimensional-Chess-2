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
	//the square the pawn being en passant is on
	protected Square EnPassantCaptureSqr;

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
	///Requests for the user to choose a chess piece to promote to and waits until a choice is made
	///</summary>
	public IEnumerator GetPromotionChoice() {
		PromotionController.Instance.ShowPromotionOptions(this);

		Debug.Log("Waiting for promotion selection...");
		yield return new WaitUntil(() => !PromotionController.Instance.SelectionInProgress);

		if (Promotion == PieceType.NONE) {
			Debug.Log("Promotion selection aborted.");
			Game.Instance.MoveCommandHandler.UndoAndRemoveCommand();
		}

		Debug.Log("Promotion selection recieved.");
		Execute();
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