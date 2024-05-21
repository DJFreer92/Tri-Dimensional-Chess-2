using static TriDimensionalChess.Game.Boards.Ownership;

namespace TriDimensionalChess.Game.Boards {
	public enum Ownership {
		NEUTRAL = 'N',
		WHITE = 'W',
		BLACK = 'B'
	}

	public static class OwnershipExtentions {
		///<summary>
		///Returns whether the owner is white or black
		///</summary>
		///<param name="owner">the owner</param>
		///<returns>Whether the owner is white or black</returns>
		public static bool IsWhiteOrBlack(this Ownership owner) => owner != NEUTRAL;

		///<summary>
		///Returns whether the owner matches the boolean value of whether the player is white or black
		///</summary>
		///<param name="owner">The owner</param>
		///<param name="player">White or black</param>
		///<returns>Whether the owner matches the boolean value</returns>
		public static bool MatchesPlayerBool(this Ownership owner, bool player) => (player ? WHITE : BLACK) == owner;
	}
}
