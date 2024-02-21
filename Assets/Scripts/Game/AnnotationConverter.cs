using System;
using UnityEngine;

public static class AnnotationConverter {
	private const int _MAX_INT_FILE = 5, _MAX_RANK = 9, _MAX_INT_BOARD = 5;
	private static readonly char[] _FILES = {'z', 'a', 'b', 'c', 'd', 'e'};
	private static readonly string[] _MAIN_BOARDS = {"W", "N", "B"};

	///<summary>
	///Converts the vector of a square into an annotation
	///</summary>
	///<params name="coords">The vector of a square</params>
	///<returns>Annotation of the square</returns>
	public static string VectorToAnnotation(this Vector3Int coords) {
		if (coords.x < 0) throw new ArgumentOutOfRangeException("File cannot be negative");
		if (coords.y < 0) throw new ArgumentOutOfRangeException("Board cannot be negative");
		if (coords.z < 0) throw new ArgumentOutOfRangeException("Rank cannot be negative");
		if (coords.x > _MAX_INT_FILE) throw new ArgumentOutOfRangeException("File out of range");
		if (coords.y > _MAX_INT_BOARD) throw new ArgumentOutOfRangeException("Board out of range");
		if (coords.z > _MAX_RANK) throw new ArgumentOutOfRangeException("Rank out of range");
		return $"{coords.x.IndexToFile()}{coords.z}" + coords.VectorToBoard();
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
	///Converts a vector of a square into a board annotation
	///</summary>
	///<params name="coords">The vector of a square</params>
	///<returns>The annotation of the board</returns>
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
	///Converts a board annotation into an index
	///</summary>
	///<params name="board">The annotation of the board</params>
	///<returns>The index of the board</returns>
	public static int BoardToIndex(this string board) {
		int x = Array.IndexOf(_MAIN_BOARDS, board);
		if (x != -1) return x * 2;
		x = Convert.ToInt32(board[^1]);
		return (x % 2 == 0) ? (x - 1) : x;
	}
}