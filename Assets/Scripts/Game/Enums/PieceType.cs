public enum PieceType : byte {
	NONE = 0,
	KING = 6,
	QUEEN = 5,
	ROOK = 4,
	BISHOP = 3,
	KNIGHT = 2,
	PAWN = 1
}

public static class PieceTypeExtensions {
	public static PieceTypeColor GetPieceTypeColor(this PieceType pt, bool isWhite) =>
		(PieceTypeColor) (7 - (int) pt + (isWhite ? 0 : 6));
}