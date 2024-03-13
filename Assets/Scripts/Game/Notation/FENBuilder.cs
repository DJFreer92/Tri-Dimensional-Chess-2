using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;

/* Uses standard FEN notation with noteable exceptions to account for the special board setup of Tri-Dimensional Chess.
 * - Attackboard position are donoted a 'W' or 'w' for white owned, 'B' or 'b' for black owned, and 'N' or 'n' for neutral boards
 *   with captial letters being on the king's side and lowercase letters being on the queen's side followed by a number indicating
 *   which number pin it is on, the attackboards will be ordered from top to bottom then left to right, if there are no attackboards
 *   the field uses the character '-'.
 * - Pawns that can make a double square move will instead be denoted by a 'D' for white and 'd' for black, 'P' and 'p' will refer
 *   only to pawns which have already moved.
 * - The '|' character will separate the pieces of different boards, main boards will be listed before attackboards from top to
 *   bottom and attackboards will be in the same order as previously denoted.
 * - En passant will be denoted by the long Tri-Dimensional algebraic notation of the square of the pawn which made the double
 *   square move if en passant is legal.
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
		Board[] boards = SortBoards(board.Boards.ToArray());

		//attackboards
		var atkbrds = new StringBuilder();
		foreach (Board brd in boards) {
			if (brd is not AttackBoard) continue;
			string abfen = char.ToString((char) brd.Owner);
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
	///Pseudo verifies a FEN
	///</summary>
	///<param name="fen">The FEN to be verified</param>
	///<returns>Whether the FEN is pseudo valid</returns>
	public static bool VerifyFEN(string fen) {
		if (string.IsNullOrEmpty(fen)) return false;

		string[] sections = fen.Split(' ');

		if (sections.Length != 7) return false;
		if (sections[0].Length % 2 == 1 && sections[0] != "-") return false;
		if (sections[1].Count(x => x == 'K') != 1) return false;
		if (sections[1].Count(x => x == 'k') != 1) return false;
		string[] boards = sections[1].Split('|');
		if (boards.Length != sections[0].Length / 2 + 3) return false;
		for (int i = 0; i < boards.Length; i++) {
			string[] ranks = boards[i].Split('/');
			if (ranks.Length != (i < 3 ? 4 : 2)) return false;
			foreach (string rank in ranks) {
				int sum = 0;
				foreach (char c in rank) {
					if (char.IsLetter(c)) {
						sum++;
						continue;
					}
					if (!char.IsDigit(c)) return false;
					sum += (int) char.GetNumericValue(c);
				}
				if (sum != (i < 3 ? 4 : 2)) return false;
			}
		}
		if (sections[2] != "w" && sections[2] != "b") return false;
		if (sections[3].Length > 4) return false;
		if (sections[3] != "-") {
			char lastChar = ' ';
			foreach (char c in sections[3]) {
				if (lastChar == c) return false;
				char upper = char.ToUpper(c);
				if (upper != 'K' && upper != 'Q') return false;
				lastChar = c;
			}
		}
		if (sections[4] != "-" && sections[4].Length != 3 && sections[4].Length != 5) return false;
		try {
			double num = char.GetNumericValue(sections[5][0]);
			if (num != (int) num || num < 0 || num >= 50) return false;
			num = char.GetNumericValue(sections[6][0]);
			if (num != (int) num || num < 0) return false;
		} catch {
			return false;
		}

		return true;
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