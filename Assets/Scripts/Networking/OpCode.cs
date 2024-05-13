namespace TriDimensionalChess.Networking {
	public enum OpCode : byte {
		KEEP_ALIVE = 0,
		WELCOME = 1,
		FEN = 2,
		PGN = 3,
		START_GAME = 4,
		GAME_STATE = 5,
		TIMERS = 6,
		MAKE_MOVE = 7,
		REMATCH = 8
	}
}
