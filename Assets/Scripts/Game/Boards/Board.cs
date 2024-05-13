using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Text;

[DisallowMultipleComponent]
public class Board : MonoBehaviour, IEnumerable {
	private static readonly char[] _FILE_ORDER = {'z', 'a', 'b', 'c', 'd', 'e'};
	public int Y;
	//squares on the board
	public List<Square> Squares {get; private set;}
	//holds if the owner of the attackboard is white, black, or if it is neutral
	public Ownership Owner;
	//notation of the board
	public string Notation {get; protected set;}

	private void Awake() {
		Squares = GetComponentsInChildren<Square>().ToList();
		SetBoardNotation();
	}

	///<summary>
	///Clears the board
	///</summary>
	public virtual void Clear() {
		foreach (Square sqr in Squares) sqr.Clear();
	}

	///<summary>
	///Return the square at the given coordinates
	///</summary>
	///<param name="coords">The coordinates of the square</param>
	///<returns>The square at the give coordinates</returns>
	public Square GetSquareAt(Vector3Int coords) {
		foreach (Square sqr in Squares)
			if (sqr.Coords.Equals(coords)) return sqr;
		return null;
	}

	///<summary>
	///Returns the square with the given piece
	///</summary>
	///<param name="piece">The piece on the desired square</param>
	///<returns>The square with the given piece</returns>
	public Square GetSquareWithPiece(ChessPiece piece) {
		foreach (Square sqr in Squares)
			if (sqr.GamePiece == piece) return sqr;
		//no square found with the specified piece
		return null;
	}

	///<summary>
	///Returns the number of pieces on the board
	///</summary>
	///<returns>The number of pieces on the board</returns>
	public int GetPieceCount() {
		int count = 0;
		foreach (Square sqr in Squares)
			if (sqr.HasPiece()) count++;
		return count;
	}

	///<summary>
	///Returns a list of all the pieces on the board
	///</summary>
	///<returns>A list of all the pieces on the board</returns>
	public List<ChessPiece> GetPieces() {
		var pieces = new List<ChessPiece>();
		foreach (Square sqr in Squares)
			if (sqr.HasPiece()) pieces.Add(sqr.GamePiece);
		return pieces;
	}

	///<summary>
	///Returns a sorted array of the squares on the board
	///</summary>
	///<returns>A sorted array of the squares on the board</returns>
	public Square[] GetSortedSquares() {
		Square[] sqrs = Squares.ToArray();
		for (int i = 0; i < sqrs.Length; i++) {
			int maxIndex = Int16.MinValue;
			int maxValue = Int16.MinValue;
			for (int j = i; j < sqrs.Length; j++) {
				int value = sqrs[j].Coords.z * 5 + Array.IndexOf(_FILE_ORDER, sqrs[j].Coords.x);
				if (value <= maxValue) continue;
				maxIndex = j;
				maxValue = value;
			}
			if (i == maxIndex) continue;
			var temp = sqrs[i];
			sqrs[i] = sqrs[maxIndex];
			sqrs[maxIndex] = temp;
		}
		return sqrs;
	}

	///<summary>
	///Sets the notation of the board
	///</summary>
	public virtual void SetBoardNotation() => Notation = char.ToString((char) Owner);

	///<summary>
	///Returns the list of squares as an enumerator
	///</summary>
	///<returns>The list of squares as an enumerator</returns>
	public IEnumerator GetEnumerator() => Squares.GetEnumerator();

	///<summary>
	///Returns a string of data about the board
	///</summary>
	///<returns>A string of data about the board</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		str.Append(Notation);
		foreach (Square sqr in Squares)
			str.Append("\n").Append(sqr.ToString());
		return str.ToString();
	}
}
