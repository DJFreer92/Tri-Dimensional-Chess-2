using static TriDimensionalChess.Game.ChessPieces.PieceType;

namespace TriDimensionalChess.Game.ChessPieces {
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

		public static PieceType GetTypeFromChar(this char pieceChar) {
			return char.ToUpper(pieceChar) switch {
				'K' => KING,
				'Q' => QUEEN,
				'R' => ROOK,
				'B' => BISHOP,
				'N' => KNIGHT,
				'P' => PAWN,
				'D' => PAWN,
				_ => NONE,
			};
		}
	}
}
