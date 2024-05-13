using System;
using System.Collections.Generic;
using UnityEngine;

using static PieceType;
using static PieceTypeColor;

public static class NotationConverter {
	private const int _MAX_INT_FILE = 5, _MAX_RANK = 9, _MAX_INT_BOARD = 5;
	private static readonly char[] _FILES = {'z', 'a', 'b', 'c', 'd', 'e'};
	private static readonly string[] _MAIN_BOARDS = {"W", "N", "B"};
	private static readonly Dictionary<char, PieceType> _PIECES_TYPE = new() {
		{'K', KING},
		{'Q', QUEEN},
		{'R', ROOK},
		{'B', BISHOP},
		{'N', KNIGHT}
	};
	private static readonly Dictionary<char, PieceTypeColor> _PIECES_TYPE_COLOR = new() {
		{'K', WHITE_KING},
		{'Q', WHITE_QUEEN},
		{'R', WHITE_ROOK},
		{'B', WHITE_BISHOP},
		{'N', WHITE_KNIGHT},
		{'P', WHITE_PAWN},
		{'D', WHITE_PAWN},
		{'k', BLACK_KING},
		{'q', BLACK_QUEEN},
		{'r', BLACK_ROOK},
		{'b', BLACK_BISHOP},
		{'n', BLACK_KNIGHT},
		{'p', BLACK_PAWN},
		{'d', BLACK_PAWN}
	};

	///<summary>
	///Converts the vector of a square into a notation
	///</summary>
	///<params name="coords">The vector of a square</params>
	///<returns>Notation of the square</returns>
	public static string VectorToNotation(this Vector3Int coords) {
		if (coords.x < 0) throw new ArgumentOutOfRangeException("File cannot be negative");
		if (coords.y < 0) throw new ArgumentOutOfRangeException("Board cannot be negative");
		if (coords.z < 0) throw new ArgumentOutOfRangeException("Rank cannot be negative");
		if (coords.x > _MAX_INT_FILE) throw new ArgumentOutOfRangeException("File out of range");
		if (coords.y > _MAX_INT_BOARD) throw new ArgumentOutOfRangeException("Board out of range");
		if (coords.z > _MAX_RANK) throw new ArgumentOutOfRangeException("Rank out of range");
		return $"{coords.x.IndexToFile()}{coords.z}{coords.VectorToBoard()}";
	}

	///<summary>
	///Converts a notation into a vector of a square
	///</summary>
	///<param name="notation">The square notation</param>
	///<returns>Vector of a square</returns>
	public static Vector3Int NotationToVector(this string notation) {
		if (notation == null) throw new ArgumentNullException(nameof(notation), "Notation cannot be null");

		return new Vector3Int(
			Array.IndexOf(_FILES, notation[0]),
			BoardToIndex(notation[notation.Length == 3 ? 2.. : 2..5]),
			(int) char.GetNumericValue(notation[1])
		);
	}

	///<summary>
	///Converts an index into a file
	///</summary>
	///<params name="index">The index of the file</params>
	///<returns>The file of the given index</returns>
	public static char IndexToFile(this int index) {
		if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
		if (index > _MAX_INT_FILE) throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
		return _FILES[index];
	}

	///<summary>
	///Converts a file into an index
	///</summary>
	///<params name="file">The file</params>
	///<returns>The index of the given file</returns>
	public static int FileToIndex(this char file) {
		int index = Array.IndexOf(_FILES, file);
		if (index == -1) throw new ArgumentException("Invalid file");
		return index;
	}

	///<summary>
	///Converts a vector of a square into a board notation
	///</summary>
	///<params name="coords">The vector of a square</params>
	///<returns>The notation of the board</returns>
	public static string VectorToBoard(this Vector3Int coords) {
		if (coords.y < 0) throw new ArgumentOutOfRangeException("Board cannot be negative");
		if (coords.y > _MAX_INT_BOARD) throw new ArgumentOutOfRangeException("Board out of range");

		if (coords.y % 2 == 0) return _MAIN_BOARDS[coords.y / 2];

		if (coords.x < 0) throw new ArgumentOutOfRangeException("File cannot be negative");
		if (coords.z < 0) throw new ArgumentOutOfRangeException("Rank cannot be negative");
		if (coords.x > _MAX_INT_FILE) throw new ArgumentOutOfRangeException("File out of range");
		if (coords.z > _MAX_RANK) throw new ArgumentOutOfRangeException("Rank out of range");

		if (coords.x == 2 || coords.x == 3) throw new ArgumentException("Invalid position");

		string str = (coords.x <= 1) ? "QL" : "KL";
		switch (coords.z) {
			case 0: case 1: str += "1";
			break;
			case 2: case 3: str += "3";
			break;
			case 4: case 5: str += (coords.y == 2) ? "2" : "5";
			break;
			case 6: case 7: str += "4";
			break;
			case 8: case 9: str += "6";
			break;
		}
		return str;
	}

	///<summary>
	///Converts a board notation into an index
	///</summary>
	///<params name="board">The notation of the board</params>
	///<returns>The index of the board</returns>
	public static int BoardToIndex(this string board) {
		if (board == null) throw new ArgumentNullException(nameof(board), "Board notation cannot be null");

		int x = Array.IndexOf(_MAIN_BOARDS, board);
		if (x != -1) return x * 2;

		x = (int) char.GetNumericValue(board[^1]);
		return (x % 2 == 0) ? (x - 1) : x;
	}

	///<summary>
	///Converts a board notation to a pinned square vector
	///</summary>
	///<param name="board">The notation of the board</param>
	///<returns>The vector of the square the board is pinned to</returns>
	public static Vector3Int BoardToVector(this string board) {
		if (board == null) throw new ArgumentNullException(nameof(board), "Board notation cannot be null");

		int depth = (int) char.GetNumericValue(board[^1]);
		if (depth % 2 == 0) depth += 2;

		return new Vector3Int(
			board[0] == 'Q' ? 1 : 4,
			BoardToIndex(board) - 1,
			depth
		);
	}

	///<summary>
	///Converts a piece to character to a piece type
	///</summary>
	///<returns>Piece type of the given character</returns>
	public static PieceType CharToPiece(this char charPiece) => _PIECES_TYPE[charPiece];

	///<summary>
	///Converts a piece character to a piece type and color
	///</summary>
	///<returns>Piece type and color of the given piece character</returns>
	public static PieceTypeColor CharToPieceColor(this char charPiece) => _PIECES_TYPE_COLOR[charPiece];
}
