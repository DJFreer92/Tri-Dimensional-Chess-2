using System.Collections;
using UnityEngine;
using System.Text;

using TriDimensionalChess.Game.Boards;
using TriDimensionalChess.Game.ChessPieces;
using TriDimensionalChess.Game.Notation;

namespace TriDimensionalChess.Game.Moves {
	public abstract class Move : ICommand {
		//the player making the move
		public readonly Player Player;
		//the departure and arrival squares
		public readonly Square StartSqr, EndSqr;
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

		//what events occured during the move
		private MoveEvent _moveEvents;

		public Move(Player player, Square start, Square end, MoveEvent moveEvents = MoveEvent.NONE) {
			Player = player;
			StartSqr = start;
			EndSqr = end;
			_moveEvents = moveEvents;
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
		///Returns the notation of the move in short 3D chess algebraic notation
		///</summary>
		///<returns>The notation of the move in short 3D chess algebraic notation</returns>
		public abstract string GetNotation();

		///<summary>
		///Determine the departure notation of the move
		///</summary>
		protected abstract void DetermineDepartureNotation();

		///<summary>
		///Returns the events that occur during the move
		///</summary>
		///<returns>The events that occur during the move</returns>
		public MoveEvent GetMoveEvents() => _moveEvents;

		///<summary>
		///Adds the given move event(s) to the move events
		///</summary>
		///<param name="events">The move event(s) to add</param>
		public void AddMoveEvent(params MoveEvent[] events) => _moveEvents.Add(events);

		///<summary>
		///Removes the given move event(s) from the move events
		///</summary>
		///<param name="events">The move event(s) to be removed</param>
		public void RemoveMoveEvent(params MoveEvent[] events) => _moveEvents.Remove(events);

		///<summary>
		///Returns whether the move has the given move event(s)
		///</summary>
		///<param name="events">The move event(s) to check for</param>
		public bool HasMoveEvent(params MoveEvent[] events) => _moveEvents.Contains(events);

		///<summary>
		///Returns whether the move has a move event of the given category of move events
		///</summary>
		///<param name="evnt">Category of move events</param>
		public bool HasMoveEventOf(MoveEvent evnt) => _moveEvents.PartialContains(evnt);

		///<summary>
		///Build the move from the given notation
		///</summary>
		///<param name="notatedMove">Notation of the move to be built</param>
		///<param name="isWhite">Whether the player is white</param>
		///<returns>The built move</returns>
		public static Move BuildMove(string notatedMove, bool isWhite) {
			Move move;
			Square startSqr = null, endSqr = null;
			bool isAttackBoardMove = false, hasCheckChar = false;
			int startIndex = 0, endIndex = notatedMove.Length, seperationIndex;

			if (notatedMove.Contains('+') || notatedMove.Contains('#')) hasCheckChar = true;

			if (notatedMove.Contains('/')) startIndex = 2;

			if (notatedMove.IndexOf('=') != notatedMove.IndexOf("(=)")) endIndex = notatedMove.IndexOf('=');
			else if (hasCheckChar) endIndex--;
			else if (notatedMove.Contains("(=)")) endIndex = notatedMove.IndexOf("(=)");

			if (notatedMove.Contains('⟳')) {  //attack board rotation
				isAttackBoardMove = true;
				startSqr = endSqr = ChessBoard.Instance.GetSquareAt(notatedMove[1..4].BoardToVector() + Vector3Int.up);
				if (char.IsUpper(notatedMove[endIndex - 1])) endIndex--;
			} else if (notatedMove.Contains("O-O")) {  //castling move
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

				foreach (AttackBoard ab in ChessBoard.Instance.AttackBoards) {
					if (!ab.GetAvailableMoves(isWhite).Contains(endSqr)) continue;
					startSqr = ab.Squares[0];
					break;
				}
			} else {  //piece move
				PieceType pieceType = char.IsUpper(notatedMove[startIndex]) ? notatedMove[startIndex].CharToPiece() : PieceType.PAWN;

				if (pieceType != PieceType.PAWN) startIndex++;

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

				endSqr = ChessBoard.Instance.GetSquareAt(arrivalInfo.NotationToVector());

				if (departureInfo.Length >= 3 && char.IsDigit(departureInfo[1]))
					startSqr = ChessBoard.Instance.GetSquareAt(arrivalInfo.NotationToVector());

				foreach (ChessPiece piece in ChessBoard.Instance.GetPieces()) {
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

			if (endIndex == notatedMove.Length) endIndex--;

			move = isAttackBoardMove ?
				new AttackBoardMove(Game.Instance.GetPlayer(isWhite), startSqr, endSqr) :
				new PieceMove(Game.Instance.GetPlayer(isWhite), startSqr, endSqr);

			if (notatedMove.Contains('x')) move.AddMoveEvent(MoveEvent.CAPTURE);
			else if (notatedMove.Contains('⟳')) move.AddMoveEvent(MoveEvent.ATTACK_BOARD_ROTATE);

			if (notatedMove.Contains("e.p.")) move.AddMoveEvent(MoveEvent.EN_PASSANT);
			else if (notatedMove.IndexOf('=') != notatedMove.IndexOf("(=)")) {
				move.AddMoveEvent(MoveEvent.PROMOTION);
				move.Promotion = notatedMove[endIndex + 1].CharToPiece();
			} else if (notatedMove.Contains('/')) {
				move.AddMoveEvent(MoveEvent.SECONDARY_PROMOTION);
				move.SecondaryPromotion = notatedMove[0].CharToPiece();
				(move as AttackBoardMove).AutoReExecute = true;
			} else if (isAttackBoardMove && char.IsUpper(notatedMove[endIndex])) {
				move.AddMoveEvent(MoveEvent.PROMOTION);
				move.Promotion = notatedMove[endIndex].CharToPiece();
			}

			if (notatedMove.Contains('#')) move.AddMoveEvent(MoveEvent.CHECKMATE);
			else if (notatedMove.Contains('+')) {
				move.AddMoveEvent(MoveEvent.CHECK);
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
			if (HasMoveEvent(MoveEvent.DRAW_OFFERED)) move.Append("=");  //draw offered
			if (HasMoveEvent(MoveEvent.CHECKMATE)) move.Append("#");  //checkmate
			else if (HasMoveEvent(MoveEvent.CHECK)) move.Append("+");  //check
			return move.ToString();
		}

		///<summary>
		///Returns data about the move
		///</summary>
		///<returns>Data about the move</returns>
		public override string ToString() {
			var str = new StringBuilder(base.ToString());
			str.Append("\nNotation: ").Append(GetNotation());
			str.Append("\nStartSqr Square: ").Append(StartSqr.ToString());
			str.Append("\nEndSqr Square: ").Append(EndSqr.ToString());
			str.Append("\nIs Attackboard Move? ").Append(this is AttackBoardMove);
			str.Append("\nEvents: ").Append(_moveEvents.ToString());
			return str.ToString();
		}
	}
}
