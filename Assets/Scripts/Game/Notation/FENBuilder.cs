using UnityEngine;
using System;
using System.IO;
using System.Text;

/* Uses standard FEN notation with noteable exceptions to account for the special board setup of Tri-Dimensional Chess.
 * - Attackboard position are donoted a 'W' or 'w' for white owned, 'B' or 'b' for black owned, and 'N' or 'n' for neutral boards
 *   with captial letters being on the king's side and lowercase letters being on the queen's side followed by a number indicating
 *   which number pin it is on, the attackboards will be ordered from top to bottom then left to right, if there are no attackboards
 *   the field uses the character '-'.
 * - Pawns that can make a double square move will instead be denoted by a 'D' for white and 'd' for black, 'P' and 'p' will refer
 *   only to pawns which have already made their double square move.
 * - The '|' character will separate the pieces of different boards, main boards will be listed before attackboards from top to
 *   bottom and attackboards will be in the same order as previously denoted.
 * - En passant will be denoted by the rank, file, and board of the pawn which made the double square move if en passant is legal.
 */
public static class FENBuilder {
	private static readonly string[] _BOARD_SORT_ORDER = {"B", "N", "W", "QL6", "KL6", "QL5", "KL5", "QL4", "KL4", "QL3", "KL3", "QL2", "KL2", "QL1", "KL1"};

	///<summary>
	///Returns the FEN of the given board position
	///</summary>
	///<param name="board">The board</param>
	///<param name="moveRuleCount">The number of moves on the move rule counter</param>
	///<param name="moveNum">The move number</param>
	///<returns>The FEN of the given board position</returns>
	public static string GetFEN(ChessBoard board, bool currentIsWhite, int moveRuleCount, int moveNum) {
		var fen = new StringBuilder();

		//sort boards
		Board[] boards = FENBuilder.SortBoards(board.Boards.ToArray());

		//attackboards
		var atkbrds = new StringBuilder();
		foreach (Board brd in boards) {
			if (brd is not AttackBoard) continue;
			string abfen = Char.ToString((Char) brd.Owner);
			if (brd.Annotation[0] == 'Q') abfen = abfen.ToLower();
			abfen += brd.Annotation[^1];
			atkbrds.Append(abfen);
		}
		fen.Append(atkbrds.Length == 0 ? '-' : atkbrds).Append(' ');

		//piece positions
		for (int i = 0; i < boards.Length; i++) {
			if (i > 0) fen.Append('|');
			Square[] sqrs = boards[i].GetSortedSquares();
			int lastRank = sqrs[0].Coords.z;
			int emptySqrs = 0;
			foreach (Square sqr in sqrs) {
				if (sqr.Coords.z != lastRank) {
					if (emptySqrs > 0) {
						fen.Append(emptySqrs);
						emptySqrs = 0;
					}
					fen.Append('/');
					lastRank = sqr.Coords.z;
				}
				if (!sqr.HasPiece()) {
					emptySqrs++;
					continue;
				}
				if (emptySqrs > 0) {
					fen.Append(emptySqrs);
					emptySqrs = 0;
				}
				string annotation = sqr.GamePiece.GetCharacter(false);
				if (annotation == "") annotation = (sqr.GamePiece as Pawn).HasDSMoveRights ? "D" : "P";
				if (!sqr.GamePiece.IsWhite) annotation = annotation.ToLower();
				fen.Append(annotation);
			}
			if (emptySqrs > 0) fen.Append(emptySqrs);
		}

		//active color
		fen.Append(' ').Append(currentIsWhite ? 'w' : 'b').Append(' ');

		//castling
		var castling = new StringBuilder();
		King king = board.GetKing(true);
		if (king.HasKSCastleRights()) castling.Append('K');
		if (king.HasQSCastleRights()) castling.Append('Q');
		king = board.GetKing(false);
		if (king.HasKSCastleRights()) castling.Append('k');
		if (king.HasQSCastleRights()) castling.Append('q');
		fen.Append(castling.Length == 0 ? '-' : castling).Append(' ');

		//en passant
		string enPassant = null;
		foreach (Square sqr in board.GetEnumerableSquares()) {
			if (sqr.GamePiece is not Pawn) continue;
			if (!(sqr.GamePiece as Pawn).JustMadeDSMove) continue;
			enPassant = sqr.GetAnnotation();
			break;
		}
		fen.Append(enPassant == null ? '-' : enPassant).Append(' ');

		//move rule count and move number
		fen.Append(moveRuleCount).Append(' ').Append(moveNum);

		return fen.ToString();
	}

	///<summary>
	///Sorts the boards
	///</summary>
	///<param name="boards">The boards to be sorted</param>
	///<returns>A sorted array of boards</returns>
	private static Board[] SortBoards(Board[] boards) {
		for (int i = 0; i < boards.Length - 1; i++) {
			int minIndex = Int16.MaxValue;
			int minOrderValue = Int16.MaxValue;
			for (int j = i; j < boards.Length; j++) {
				int orderValue = Array.IndexOf(_BOARD_SORT_ORDER, boards[j].Annotation);
				if (orderValue >= minOrderValue) continue;
				minIndex = j;
				minOrderValue = orderValue;
			}
			if (i == minIndex) continue;
			var temp = boards[i];
			boards[i] = boards[minIndex];
			boards[minIndex] = temp;
		}
		return boards;
	}

	///<summary>
	///Exports the FEN of the given board position
	///</summary>
	///<param name="filePath">The file path to export the FEN to</param>
	///<param name="board">The board</param>
	///<param name="moveRuleCount">The number of moves on the move rule counter</param>
	///<param name="moveNum">The move number</param>
	public static void ExportFEN(string filePath, ChessBoard board, bool currentIsWhite, int moveRuleCount, int moveNum) {
		if (String.IsNullOrEmpty(filePath)) throw new ArgumentException(nameof(filePath), "Invalid file path.");
		using (var outFile = new StreamWriter(Path.Combine(filePath, "test.fen"))) {
			outFile.WriteLine(FENBuilder.GetFEN(board, currentIsWhite, moveRuleCount, moveNum));
		}
	}
}