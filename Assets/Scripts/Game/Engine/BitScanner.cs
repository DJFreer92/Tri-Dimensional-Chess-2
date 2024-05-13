using UnityEngine;

namespace TriDimensionalChess.Game.Engine {
	public static class BitScanner {
		private const ulong _MAGIC = 0x37E84A99DAE458F;
		private static readonly int[] _MAGIC_TABLE = {
			00, 01, 17, 02, 18, 50, 03, 57,
			47, 19, 22, 51, 29, 04, 33, 58,
			15, 48, 20, 27, 25, 23, 52, 41,
			54, 30, 38, 05, 43, 34, 59, 08,
			63, 16, 49, 56, 46, 21, 28, 32,
			14, 26, 24, 40, 53, 37, 42, 07,
			62, 55, 45, 31, 13, 39, 36, 06,
			61, 44, 12, 35, 60, 11, 10, 09,
		};

		public static int BitScanForward(ulong b) => _MAGIC_TABLE[((ulong) ((long) b & -(long) b) * _MAGIC) >> 58];

		public static int BitScanReverse(ulong b) {
			b |= b >> 1;
			b |= b >> 2;
			b |= b >> 4;
			b |= b >> 8;
			b |= b >> 16;
			b |= b >> 32;
			b &= ~(b >> 1);
			return _MAGIC_TABLE[b * _MAGIC >> 58];
		}
	}
}
