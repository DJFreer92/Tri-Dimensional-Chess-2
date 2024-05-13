using UnityEngine;
using System;
using System.IO;
using SFB;

/* Uses standard FEN notation with noteable exceptions to account for the special board setup of Tri-Dimensional Chess.
 * - Attack board position are donoted a 'W' or 'w' for white-owned, 'B' or 'b' for black-owned, and 'N' or 'n' for neutral boards
 *   with captial letters being on the king's side and lowercase letters being on the queen's side followed by a number indicating
 *   which number pin it is on, the attack boards will be ordered from top to bottom then left to right, if there are no attack
 *   boards the field uses the character '-'.
 * - Pawns that can make a double square move will instead be denoted by a 'D' for white and 'd' for black, 'P' and 'p' will refer
 *   only to pawns which have already moved.
 * - The '|' character will separate the pieces of different boards, main boards will be listed before attack boards from top to
 *   bottom and attack boards will be in the same order as previously denoted.
 * - En passant will be denoted by the long Tri-Dimensional algebraic notation of the square of the pawn which made the double
 *   square move if en passant is legal.
 */
public struct FEN {
	private static readonly string[] _BOARD_SORT_ORDER = {"B", "N", "W", "QL6", "KL6", "QL5", "KL5", "QL4", "KL4", "QL3", "KL3", "QL2", "KL2", "QL1", "KL1"};

	public string Fen {
		readonly get => IsEmpty() ? null : string.Join(' ', _fen);
		set => _fen = value.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
	}
	private string[] _fen;

	public FEN(string fen = "") {
		_fen = fen.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

		if (_fen.Length != 7) {
			Debug.LogError("Invalid FEN");
			_fen = null;
		}
	}

	public FEN(Game game, ChessBoard board) {
		_fen = new string[7];
		ConstructFEN(board, game.CurPlayer.IsWhite, game.MoveRuleCount, game.MoveCount);
	}

	public FEN(ChessBoard board, bool currentIsWhite, int moveRuleCount, int moveNum) {
		_fen = new string[7];
		ConstructFEN(board, currentIsWhite, moveRuleCount, moveNum);
	}

	public static bool operator ==(FEN a, FEN b) => a._fen == b._fen;

	public static bool operator !=(FEN a, FEN b) => a._fen != b._fen;

	///<summary>
	///Constructs the FEN from the given board position
	///</summary>
	///<param name="board">The board</param>
	///<param name="moveRuleCount">The number of moves on the move rule counter</param>
	///<param name="moveNum">The move number</param>
	private readonly void ConstructFEN(ChessBoard board, bool currentIsWhite, int moveRuleCount, int moveNum) {

		//sort boards
		Board[] boards = SortBoards(board.Boards.ToArray());

		//attackboards
		string abfen;
		foreach (Board brd in boards) {
			if (brd is not AttackBoard) continue;
			abfen = char.ToString((char) brd.Owner);
			if (brd.Notation[0] == 'Q') abfen = abfen.ToLower();
			abfen += brd.Notation[^1];
			_fen[0] += abfen;
		}
		if (string.IsNullOrEmpty(_fen[0])) _fen[0] = "-";

		//piece positions
		int lastRank, emptySqrs;
		string notation;
		for (int i = 0; i < boards.Length; i++) {
			if (i > 0) _fen[1] += '|';
			Square[] sqrs = boards[i].GetSortedSquares();
			lastRank = sqrs[0].Coords.z;
			emptySqrs = 0;
			foreach (Square sqr in sqrs) {
				if (sqr.Coords.z != lastRank) {
					if (emptySqrs > 0) {
						_fen[1] += emptySqrs;
						emptySqrs = 0;
					}
					_fen[1] += '/';
					lastRank = sqr.Coords.z;
				}
				if (!sqr.HasPiece()) {
					emptySqrs++;
					continue;
				}
				if (emptySqrs > 0) {
					_fen[1] += emptySqrs;
					emptySqrs = 0;
				}
				notation = sqr.GamePiece.GetCharacter(false);
				if (string.IsNullOrEmpty(notation)) notation = (sqr.GamePiece as Pawn).HasDSMoveRights ? "D" : "P";
				if (!sqr.GamePiece.IsWhite) notation = notation.ToLower();
				_fen[1] += notation;
			}
			if (emptySqrs > 0) _fen[1] += emptySqrs;
		}

		//active color
		_fen[2] = currentIsWhite ? "w" : "b";

		//castling
		King king = board.GetKing(true);
		if (king.HasKSCastleRights()) _fen[3] += 'K';
		if (king.HasQSCastleRights()) _fen[3] += 'Q';
		king = board.GetKing(false);
		if (king.HasKSCastleRights()) _fen[3] += 'k';
		if (king.HasQSCastleRights()) _fen[3] += 'q';
		if (string.IsNullOrEmpty(_fen[3])) _fen[3] = "-";

		//en passant
		foreach (Square sqr in board.EnumerableSquares()) {
			if (sqr.GamePiece is not Pawn) continue;
			if (!(sqr.GamePiece as Pawn).JustMadeDSMove) continue;
			_fen[4] = sqr.Notation;
			break;
		}
		if (string.IsNullOrEmpty(_fen[4])) _fen[4] = "-";

		//move rule count and move number
		_fen[5] = moveRuleCount.ToString();
		_fen[6] = moveNum.ToString();
	}

	///<summary>
	///Sorts the boards
	///</summary>
	///<param name="boards">The boards to be sorted</param>
	///<returns>A sorted array of boards</returns>
	private static Board[] SortBoards(Board[] boards) {
		for (int i = 0; i < boards.Length - 1; i++) {
			int minIndex = int.MaxValue;
			int minOrderValue = int.MaxValue;
			for (int j = i; j < boards.Length; j++) {
				int orderValue = Array.IndexOf(_BOARD_SORT_ORDER, boards[j].Notation);
				if (orderValue >= minOrderValue) continue;
				minIndex = j;
				minOrderValue = orderValue;
			}
			if (i == minIndex) continue;
            (boards[minIndex], boards[i]) = (boards[i], boards[minIndex]);
        }
        return boards;
	}

	///<summary>
	///Returns the FEN notation of the attack board positions
	///</summary>
	///<returns>The FEN notation of the attack board positions</returns>
	public readonly string GetABPositions() => _fen[0] == "-" ? null : _fen[0];

	///<summary>
	///Returns the FEN notation of the piece positions
	///</summary>
	///<returns>The FEN notation of the piece positions</returns>
	public readonly string GetPiecePositions() => _fen[1];

	///<summary>
	///Returns whether the current player is white
	///</summary>
	///<returns>Whether the current player is white</returns>
	public readonly bool GetCurPlayer() => _fen[2] == "w";

	///<summary>
	///Returns the FEN notation of the castling rights
	///</summary>
	///<returns>The FEN notation of the castling rights</returns>
	public readonly string GetCastlingRights() => _fen[3] == "-" ? null : _fen[3];

	///<summary>
	///Returns the FEN notation of the en passant
	///</summary>
	///<returns>The FEN notation of the en passant</returns>
	public readonly string GetEnPassant() => _fen[4] == "-" ? null : _fen[4];

	///<summary>
	///Returns the move rule count
	///</summary>
	///<returns>The move rule count</returns>
	public readonly int GetMoveRuleCount() => int.Parse(_fen[5]);

	///<summary>
	///Returns the move count
	///</summary>
	///<returns>The move count</returns>
	public readonly int GetMoveCount() => int.Parse(_fen[6]);

	///<summary>
	///Returns whether the FEN is empty
	///</summary>
	///<returns>Whether the FEN is empty</returns>
	public readonly bool IsEmpty() => _fen.Length == 0;

	///<summary>
	///Prints the FEN to the debug console
	///</summary>
	public readonly void Print() => Debug.Log(Fen);

	///<summary>
	///Exports the FEN of the given board position
	///</summary>
	///<param name="board">The board</param>
	///<param name="moveRuleCount">The number of moves on the move rule counter</param>
	///<param name="moveNum">The move number</param>
	public readonly void Export() {
		string path = StandaloneFileBrowser.SaveFilePanel("Select Save Location", "", $"Tri-D_{DateTime.Now:yyyy-MM-dd_hhmmss}", "fen");

		if (string.IsNullOrEmpty(path)) {
			MessageManager.Instance.CreateMessage("Export Aborted");
			return;
		}

        using var outFile = new StreamWriter(path);
        outFile.WriteLine(Fen);

		MessageManager.Instance.CreateMessage("Exported FEN");
    }

	public override readonly bool Equals(object o) => base.Equals(o);

	public override readonly int GetHashCode() => base.GetHashCode();

    public override readonly string ToString() => Fen;
}
