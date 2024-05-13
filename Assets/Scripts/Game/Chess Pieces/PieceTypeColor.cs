namespace TriDimensionalChess.Game.ChessPieces {
	public enum PieceTypeColor : byte {
		NONE = 0,
		WHITE_KING = 1,
		WHITE_QUEEN = 2,
		WHITE_ROOK = 3,
		WHITE_BISHOP = 4,
		WHITE_KNIGHT = 5,
		WHITE_PAWN = 6,
		BLACK_KING = 7,
		BLACK_QUEEN = 8,
		BLACK_ROOK = 9,
		BLACK_BISHOP = 10,
		BLACK_KNIGHT = 11,
		BLACK_PAWN = 12
	}

	public static class PieceTypeColorExtensions {
		public static PieceType GetPieceType(this PieceTypeColor ptc) => (PieceType) ((byte) ptc % 7 + 1);

		public static bool GetColor(this PieceTypeColor ptc) => ptc < PieceTypeColor.BLACK_KING;
	}
}
