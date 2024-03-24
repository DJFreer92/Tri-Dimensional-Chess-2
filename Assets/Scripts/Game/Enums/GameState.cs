using System;

[Flags]
public enum GameState : ushort {
	PRE_GAME = 0,
	WHITE_TURN = 1,
	BLACK_TURN = 1 << 1,
	WHITE_WIN_NORMAL = 1 << 2,
	BLACK_WIN_NORMAL = 1 << 3,
	DRAW_MUTUAL_AGREEMENT = 1 << 4,
	DRAW_STALEMATE = 1 << 5,
	DRAW_THREEFOLD_REPETITION = 1 << 6,
	DRAW_FIFTY_MOVE_RULE = 1 << 7,
	DRAW_DEAD_POSITION = 1 << 8,
	DRAW_TIMEOUT_VS_INSUFFICIENT_MATERIAL = 1 << 9,
	WHITE_RESIGNATION = 1 << 10,
	BLACK_RESIGNATION = 1 << 11,
	WHITE_TIMEOUT_LOSE = 1 << 12,
	BLACK_TIMEOUT_LOSE = 1 << 13,
	ANALYSIS = 1 << 14,
	ACTIVE = WHITE_TURN | BLACK_TURN,
	INACTIVE = ushort.MaxValue ^ ACTIVE,
	POST_GAME = INACTIVE ^ PRE_GAME,
	WIN_NORMAL = WHITE_WIN_NORMAL | BLACK_WIN_NORMAL,
	DRAW = DRAW_MUTUAL_AGREEMENT | DRAW_STALEMATE | DRAW_DEAD_POSITION | DRAW_TIMEOUT_VS_INSUFFICIENT_MATERIAL,
	RESIGNATION = WHITE_RESIGNATION | BLACK_RESIGNATION,
	TIMEOUT_LOSE = WHITE_TIMEOUT_LOSE | BLACK_TIMEOUT_LOSE,
	TIMEOUT = TIMEOUT_LOSE | DRAW_TIMEOUT_VS_INSUFFICIENT_MATERIAL,
	WHITE_WIN = WHITE_WIN_NORMAL | BLACK_RESIGNATION | BLACK_TIMEOUT_LOSE,
	BLACK_WIN = BLACK_WIN_NORMAL | WHITE_RESIGNATION | WHITE_TIMEOUT_LOSE,
	WIN = WHITE_WIN | BLACK_WIN
}

public static class GameStateExtensions {
	///<summary>
	///Returns whether the given state is in the given category
	///</summary>
	///<params name="state">The state to look for in the category</params>
	///<params name="category">The category to check for the state in</params>
	///<returns>Whether the given state is in the given category</returns>
	public static bool Is(this GameState state, GameState category) {
		return (category & state) == state;
	}

	///<summary>
	///Returns the GameState of all the given states combined
	///</summary>
	///<params name="state">The state to add the given states to</params>
	///<params name="states">The states to be combined with the given state</params>
	///<returns>The GameState of all the given states combined</returns>
	public static GameState Combine(this GameState state, params GameState[] states) {
		if (states == null) throw new ArgumentNullException(nameof(states), "States cannot be null");
		GameState result = state;
		foreach (GameState other in states) result |= other;
		return result;
	}
}