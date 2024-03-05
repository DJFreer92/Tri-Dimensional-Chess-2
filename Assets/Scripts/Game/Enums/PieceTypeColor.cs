public enum PieceTypeColor : byte {
	WHITE_KING = 0,
	WHITE_QUEEN = 1,
	WHITE_ROOK = 2,
	WHITE_BISHOP = 3,
	WHITE_KNIGHT = 4,
	WHITE_PAWN = 5,
	BLACK_KING = 6,
	BLACK_QUEEN = 7,
	BLACK_ROOK = 8,
	BLACK_BISHOP = 9,
	BLACK_KNIGHT = 10,
	BLACK_PAWN = 11
}

public static class PieceTypeColorExtensions {
	public static PieceType GetPieceType(this PieceTypeColor ptc) => (PieceType) ((byte) ptc % 6 + 1);

	public static bool GetColor(this PieceTypeColor ptc) => ptc < PieceTypeColor.BLACK_KING;
}