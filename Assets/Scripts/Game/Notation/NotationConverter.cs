using System;
using System.Collections.Generic;
using UnityEngine;

using TriDimensionalChess.Game.Boards;
using TriDimensionalChess.Game.ChessPieces;

using static TriDimensionalChess.Game.ChessPieces.PieceType;
using static TriDimensionalChess.Game.ChessPieces.PieceTypeColor;

namespace TriDimensionalChess.Game.Notation {
	public static class NotationConverter {
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
		///Converts a notation into a vector of a square
		///</summary>
		///<param name="notation">The square notation</param>
		///<returns>Vector of a square</returns>
		public static Vector3Int NotationToVector(this string notation) {
			if (notation == null) throw new ArgumentNullException(nameof(notation), "Notation cannot be null");

			return new Vector3Int(
				Array.IndexOf(ChessBoard.FILES, notation[0]),
				BoardToIndex(notation[2..]),
				(int) char.GetNumericValue(notation[1])
			);
		}

		///<summary>
		///Converts an index into a file
		///</summary>
		///<params name="index">The index of the file</params>
		///<returns>The file of the given index</returns>
		public static char IndexToFile(this int index) {
			if (index < ChessBoard.MIN_INT_FILE || index > ChessBoard.MAX_INT_FILE)
				throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
			return ChessBoard.FILES[index];
		}

		///<summary>
		///Converts a file into an index
		///</summary>
		///<params name="file">The file</params>
		///<returns>The index of the given file</returns>
		public static int FileToIndex(this char file) {
			int index = Array.IndexOf(ChessBoard.FILES, file);
			if (index == -1) throw new ArgumentException(nameof(file), "Invalid file");
			return index;
		}

		///<summary>
		///Converts a board notation into an index
		///</summary>
		///<params name="board">The notation of the board</params>
		///<returns>The index of the board</returns>
		public static int BoardToIndex(this string board) {
			if (board == null) throw new ArgumentNullException(nameof(board), "Board notation cannot be null");

			int x = Array.IndexOf(ChessBoard.MAIN_BOARDS, board);
			if (x != -1) return x * 2;

			bool inverted = board[^1] == 'I';

			x = (int) char.GetNumericValue(board[inverted ? ^2 : ^1]);
			x = (x % 2 == 0) ? (x - 1) : x;
			return inverted ? x - 2 : x;
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
}
