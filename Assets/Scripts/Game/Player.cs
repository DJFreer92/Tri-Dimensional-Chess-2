using UnityEngine;
using System.Text;

namespace TriDimensionalChess.Game {
	public sealed class Player {
		//holds whether the player is playing the white or black pieces
		public readonly bool IsWhite;
		//the color of the player's pieces
		public readonly string ColorPieces;

		public Player(bool isWhitePieces) {
			IsWhite = isWhitePieces;
			ColorPieces = isWhitePieces ? "White" : "Black";
		}

		///<summary>
		///Return a string of data about the player
		///</summary>
		///<returns>Data about the player</returns>
		public override string ToString() {
			var str = new StringBuilder(base.ToString());
			str.Append(" (Color: ").Append(ColorPieces).Append(")");
			return str.ToString();
		}
	}
}
