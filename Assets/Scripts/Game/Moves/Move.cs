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
	//holder for pawn or promoted piece for when a promotion is undone or redone
	public ChessPiece PromotionUndoRedoHolder;
	//the square the pawn being en passant is on
	protected Square _enPassantCaptureSqr;
	//the notation of the square being departed from for the purposes of short algebraic move notation
	protected string _departureNotation;

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
	///Returns the notation of the move in long 3D chess algebraic notation
	///</summary>
	///<returns>The notation of the move in long 3D chess algebraic notation</returns>
	public abstract string GetLongNotation();

	///<summary>
	///Returns the notation of the move in short 3D chess algebraic notation
	///</summary>
	///<returns>The notation of the move in short 3D chess algebraic notation</returns>
	public abstract string GetShortNotation();

	///<summary>
	///Determine the departure notation of the move
	///</summary>
	protected abstract void DetermineDepartureNotation();

	///<summary>
	///Build the move from the given notation
	///</summary>
	///<param name="notatedMove">Notation of the move to be built</param>
	///<param name="isWhite">Whether the player is white</param>
	///<returns>The built move</returns>
	public static Move BuildMove(string notatedMove, bool isWhite) {
		Debug.Log($"Building {notatedMove}");
		Move move;

		int seperationIndex, endIndex = notatedMove.Length;
		if (notatedMove.Contains('=')) endIndex = notatedMove.IndexOf('=');
		else if (notatedMove.Contains('+') || notatedMove.Contains('#')) endIndex--;

		if (notatedMove.Contains("O-O")) {
			Square rookSqr;

			if (notatedMove.Contains("O-O-O"))
				rookSqr = ChessBoard.Instance.GetSquareAt(
					isWhite ? ChessBoard.WhiteQueenSideRookCoords : ChessBoard.BlackQueenSideRookCoords
				);
			else
				rookSqr = ChessBoard.Instance.GetSquareAt(
					isWhite ? ChessBoard.WhiteKingSideRookCoords : ChessBoard.BlackKingSideRookCoords
				);

			move = new PieceMove(
				Game.Instance.GetPlayer(isWhite),
				ChessBoard.Instance.GetSquareAt(isWhite ? ChessBoard.WhiteKingCoords : ChessBoard.BlackKingCoords),
				rookSqr
			);
		} else if (notatedMove.Contains('-')) {
			seperationIndex = notatedMove.IndexOf('-');
			if (char.IsUpper(notatedMove[endIndex - 1])) endIndex--;

			move = new AttackBoardMove(
				Game.Instance.GetPlayer(isWhite),
				ChessBoard.Instance.GetSquareAt(notatedMove[..seperationIndex].BoardToVector() + Vector3Int.up),
				ChessBoard.Instance.GetSquareAt(notatedMove[(seperationIndex + 1)..endIndex].BoardToVector())
			);
		} else {
			seperationIndex = notatedMove.IndexOf('x');
			if (seperationIndex == -1)
				for (int i = 2; i < notatedMove.Length; i++)
					if (char.IsLower(notatedMove[i])) seperationIndex = i;
			if (notatedMove.Contains("e.p.")) endIndex = notatedMove.IndexOf("e.p.");

			move = new PieceMove(
				Game.Instance.GetPlayer(isWhite),
				ChessBoard.Instance.GetSquareAt(
					notatedMove[(char.IsUpper(notatedMove[0]) ? 1 : 0)..seperationIndex].NotationToVector()
				),
				ChessBoard.Instance.GetSquareAt(
					notatedMove[(seperationIndex + (notatedMove.Contains('x') ? 1 : 0))..endIndex].NotationToVector()
				)
			);
		}

		if (notatedMove.Contains('x')) move.MoveEvents.Add(MoveEvent.CAPTURE);
		if (notatedMove.Contains("e.p.")) move.MoveEvents.Add(MoveEvent.EN_PASSANT);
		else if (notatedMove.Contains('=') || (notatedMove.Contains('-') && !notatedMove.Contains("O-O") && char.IsUpper(notatedMove[^1]))) {
			move.MoveEvents.Add(MoveEvent.PROMOTION);
			move.Promotion = notatedMove[^1].CharToPiece();
		}
		if (notatedMove.Contains('#')) move.MoveEvents.Add(MoveEvent.CHECK, MoveEvent.CHECKMATE);
		else if (notatedMove.Contains('+')) {
			move.MoveEvents.Add(MoveEvent.CHECK);
			ChessBoard.Instance.GetKing(!isWhite).IsInCheck = true;
		}

		return move;
	}

	///<summary>
	///Build the move from the given notation
	///</summary>
	///<param name="notatedMove">Notation of the move to be built</param>
	///<param name="isWhite">Whether the player is white</param>
	///<returns>The built move</returns>
	public static Move BuildMoveDynamic(string notatedMove, bool isWhite) {
		Debug.Log($"Dynamically Building {notatedMove}");
		Move move;
		Square startSqr = null, endSqr = null;
		bool isAttackBoardMove = false, hasCheckChar = false;
		int startIndex = 0, endIndex = notatedMove.Length, seperationIndex;

		if (notatedMove.Contains('+') || notatedMove.Contains('#')) hasCheckChar = true;

		if (notatedMove.Contains('/')) startIndex = 2;

		if (notatedMove.IndexOf('=') != notatedMove.IndexOf("(=)")) endIndex = notatedMove.IndexOf('=');
		else if (hasCheckChar) endIndex--;
		else if (notatedMove.Contains("(=)")) endIndex = notatedMove.IndexOf("(=)");

		if (notatedMove.Contains("O-O")) {  //castling move
			Square rookSqr;

			if (notatedMove.Contains("O-O-O"))
				rookSqr = ChessBoard.Instance.GetSquareAt(
					isWhite ? ChessBoard.WhiteQueenSideRookCoords : ChessBoard.BlackQueenSideRookCoords
				);
			else
				rookSqr = ChessBoard.Instance.GetSquareAt(
					isWhite ? ChessBoard.WhiteKingSideRookCoords : ChessBoard.BlackKingSideRookCoords
				);

			startSqr = ChessBoard.Instance.GetSquareAt(isWhite ? ChessBoard.WhiteKingCoords : ChessBoard.BlackKingCoords);
			endSqr = rookSqr;
		} else if (notatedMove.Contains('-')) {  //attack board move with discriminator
			isAttackBoardMove = true;
			seperationIndex = notatedMove.IndexOf('-');
			if (char.IsUpper(notatedMove[endIndex - 1])) endIndex--;

			startSqr = ChessBoard.Instance.GetSquareAt(notatedMove[startIndex..seperationIndex].BoardToVector() + Vector3Int.up);
			endSqr = ChessBoard.Instance.GetSquareAt(notatedMove[(seperationIndex + 1)..endIndex].BoardToVector());
		} else if (notatedMove[startIndex + 1] == 'L') {  //attack board move without discriminator
			isAttackBoardMove = true;
			if (char.IsUpper(notatedMove[endIndex - 1])) endIndex--;

			endSqr = ChessBoard.Instance.GetSquareAt(notatedMove[startIndex..endIndex].BoardToVector());

			foreach (Board brd in ChessBoard.Instance) {
				if (brd is not AttackBoard) continue;
				if (!(brd as AttackBoard).GetAvailableMoves(isWhite).Contains(endSqr)) continue;
				startSqr = brd.Squares[0];
				break;
			}
		} else {  //piece move
			PieceType pieceType = char.IsUpper(notatedMove[startIndex]) ? notatedMove[startIndex].CharToPiece() : PieceType.PAWN;

			if (pieceType != PieceType.PAWN) startIndex++;

			Debug.Log(pieceType);

			Debug.Log($"Start Index: {startIndex}");
			Debug.Log($"End Index: {endIndex}");

			string departureInfo = "", arrivalInfo = "";
			for (int i = startIndex; i < endIndex; i++) {
				if (notatedMove[i] == 'x') {
					departureInfo = arrivalInfo;
					arrivalInfo = "";
					i++;
				} else if (char.IsLower(notatedMove[i]) && string.IsNullOrEmpty(departureInfo)) {
					departureInfo = arrivalInfo;
					arrivalInfo = "";
				}

				arrivalInfo += notatedMove[i];
			}

			Debug.Log(departureInfo);
			Debug.Log(arrivalInfo);

			endSqr = ChessBoard.Instance.GetSquareAt(arrivalInfo.NotationToVector());

			if (departureInfo.Length >= 3 && char.IsDigit(departureInfo[1]))
				startSqr = ChessBoard.Instance.GetSquareAt(arrivalInfo.NotationToVector());

			foreach (ChessPiece piece in ChessBoard.Instance.GetPieces()) {
				Debug.Log($"Found piece of type: {piece.Type}");
				if (piece.Type != pieceType) continue;
				if (!string.IsNullOrEmpty(departureInfo) && !piece.GetSquare().Notation.Contains(departureInfo[0])) continue;
				if (departureInfo.Length > 1 && !piece.GetSquare().Notation.Contains(departureInfo[1..])) continue;

				foreach (Square sqr in piece.GetAvailableMoves(isWhite)) {
					if (sqr != endSqr) continue;

					startSqr = piece.GetSquare();
					break;
				}
				if (startSqr != null) break;
			}
		}

		Debug.Log($"Start Square: {(startSqr == null ? "NULL" : startSqr.Notation)}");
		Debug.Log($"End Square: {(endSqr == null ? "NULL" : endSqr.Notation)}");

		move = isAttackBoardMove ?
			   new AttackBoardMove(Game.Instance.GetPlayer(isWhite), startSqr, endSqr) :
			   new PieceMove(Game.Instance.GetPlayer(isWhite), startSqr, endSqr);

		if (notatedMove.Contains('x')) move.MoveEvents.Add(MoveEvent.CAPTURE);

		if (notatedMove.Contains("e.p.")) move.MoveEvents.Add(MoveEvent.EN_PASSANT);
		else if (notatedMove.IndexOf('=') != notatedMove.IndexOf("(=)")) {
			move.MoveEvents.Add(MoveEvent.PROMOTION);
			move.Promotion = notatedMove[endIndex + 1].CharToPiece();
		} else if (notatedMove.Contains('/')) {
			move.MoveEvents.Add(MoveEvent.SECONDARY_PROMOTION);
			move.SecondaryPromotion = notatedMove[0].CharToPiece();
			(move as AttackBoardMove).AutoReExecute = true;
		} else if (isAttackBoardMove && char.IsUpper(notatedMove[endIndex])) {
			move.MoveEvents.Add(MoveEvent.PROMOTION);
			move.Promotion = notatedMove[endIndex].CharToPiece();
		}

		if (notatedMove.Contains('#')) move.MoveEvents.Add(MoveEvent.CHECKMATE);
		else if (notatedMove.Contains('+')) {
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
	///Returns the ending notation of the move
	///</summary>
	///<returns>The ending notation of the move</returns>
	public string GetEndingNotation() {
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
		str.Append("\nNotation: ").Append(GetLongNotation());
		str.Append("\nStartSqr Square: ").Append(StartSqr.ToString());
		str.Append("\nEndSqr Square: ").Append(EndSqr.ToString());
		str.Append("\nIs Attackboard Move? ").Append(this is AttackBoardMove);
		str.Append("\nEvents: ").Append(MoveEvents.ToString());
		return str.ToString();
	}
}
