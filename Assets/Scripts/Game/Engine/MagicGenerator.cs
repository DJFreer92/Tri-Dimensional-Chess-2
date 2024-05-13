using System;
using System.Text;
using System.IO;

namespace TriDimensionalChess.Game.Engine {
	public class Generator {
		public static void Main(string[] args) {
			MagicGenerator.PrintMagics();
		}
	}

	public static class MagicGenerator {
		private static readonly int[] BitTable = {
			63, 30, 03, 32, 25, 41, 22, 33,
			15, 50, 42, 13, 11, 53, 19, 34,
			61, 29, 02, 51, 21, 43, 45, 10,
			18, 47, 01, 54, 09, 57, 00, 35,
			62, 31, 40, 04, 49, 05, 52, 26,
			60, 06, 23, 44, 46, 27, 56, 16,
			07, 39, 48, 24, 59, 14, 12, 55,
			38, 28, 58, 20, 37, 17, 36, 08
		};
		private static readonly int[] RBits = {
			12, 11, 11, 11, 11, 12,
			11, 10, 10, 10, 10, 11,
			11, 10, 10, 10, 10, 11,
			11, 10, 10, 10, 10, 11,
			11, 10, 10, 10, 10, 11,
			11, 10, 10, 10, 10, 11,
			11, 10, 10, 10, 10, 11,
			11, 10, 10, 10, 10, 11,
			11, 10, 10, 10, 10, 11,
			12, 11, 11, 11, 11, 12
		};
		private static readonly int[] BBits = {
			4, 3, 3, 3, 3, 4,
			4, 3, 3, 3, 3, 4,
			5, 4, 5, 5, 4, 5,
			6, 5, 5, 5, 5, 6,
			7, 6, 6, 6, 6, 7,
			7, 6, 6, 6, 6, 7,
			6, 5, 6, 6, 5, 6,
			5, 4, 5, 5, 4, 5,
			4, 3, 3, 3, 3, 4,
			4, 3, 3, 3, 3, 4
		};

		public static void PrintMagics() {
			int square;

			var str = new StringBuilder();
			str.AppendLine("public static ulong[] RMagic = {");
			for(square = 0; square < 60; square++)
				str.Append('\t').Append(string.Format("0x{0:X}", FindMagic(square, RBits[square], false))).AppendLine(",");
			str.AppendLine("};\n\n");

			str.AppendLine("public static ulong[] BMagic = {");
			for(square = 0; square < 60; square++)
				str.Append('\t').Append(string.Format("0x{0:X}", FindMagic(square, BBits[square], true))).AppendLine(",");
			str.AppendLine("};");

			using (var outfile = new StreamWriter("Magics.txt")) outfile.Write(str);
		}

		private static ulong FindMagic(int sq, int m, bool isBishop) {
			ulong mask, magic;
			ulong[] b = new ulong[4096], a = new ulong[4096], used = new ulong[4096];
			int i, j, k, n, fail;

			mask = isBishop ? BMask(sq) : RMask(sq);
			n = CountOnes(mask);

			for (i = 0; i < (1 << n); i++) {
				b[i] = IndexToUlong(i, n, mask);
				a[i] = isBishop ? BAtt(sq, b[i]) : RAtt(sq, b[i]);
			}
			for (k = 0; k < 100000000; k++) {
				magic = RandomUlongFewBits();
				if (CountOnes((mask * magic) & 0xFF00000000000000UL) < 6) continue;
				for (i = 0; i < 4096; i++) used[i] = 0UL;
				for (i = 0, fail = 0; fail == 0 && i < (1 << n); i++) {
					j = Transform(b[i], magic, m);
					if (used[j] == 0UL) used[j] = a[i];
					else if (used[j] != a[i]) fail = 1;
				}
				if (fail == 0) return magic;
			}
			Console.WriteLine("***Failed***\n");
			return 0UL;
		}

		private static ulong BMask(int sq) {
			ulong result = 0UL;
			int rank = sq / 6, file = sq % 6, r, f;
			for (r = rank + 1, f = file + 1; r <= 4 && f <= 4; r++, f++) result |= 1UL << f + r * 6;
			for (r = rank + 1, f = file - 1; r <= 4 && f >= 1; r++, f--) result |= 1UL << f + r * 6;
			for (r = rank - 1, f = file + 1; r >= 1 && f <= 4; r--, f++) result |= 1UL << f + r * 6;
			for (r = rank - 1, f = file - 1; r >= 1 && f >= 1; r--, f--) result |= 1UL << f + r * 6;
			return result;
		}

		private static ulong RMask(int sq) {
			ulong result = 0UL;
			int rank = sq / 6, file = sq % 6, r, f;
			for (r = rank + 1; r <= 4; r++) result |= 1UL << file + r * 6;
			for (r = rank - 1; r >= 1; r--) result |= 1UL << file + r * 6;
			for (f = file + 1; f <= 4; f++) result |= 1UL << f + rank * 6;
			for (f = file - 1; f >= 1; f--) result |= 1UL << f + rank * 6;
			return result;
		}

		private static int CountOnes(ulong b) {
			int count;
			for (count = 0; b != 0; count++, b &= b - 1);
			return count;
		}

		private static ulong IndexToUlong(int index, int bits, ulong m) {
			int i, j;
			ulong result = 0UL;
			for (i = 0; i < bits; i++) {
				j = PopFirstBit(m);
				if ((index & (1 << i)) != 0) result |= 1UL << j;
			}
			return result;
		}

		private static int PopFirstBit(ulong bb) {
			ulong b = bb ^ (bb - 1);
			uint fold = (uint) ((b & 0xffffffff) ^ (b >> 32));
			bb &= (bb - 1);
			return BitTable[(fold * 0x783a9b23) >> 26];
		}

		private static ulong BAtt(int sq, ulong block) {
			ulong result = 0UL;
			int rank = sq / 6, file = sq % 6, r, f;
			for (r = rank + 1, f = file + 1; r <= 5 && f <= 5; r++, f++) {
				result |= 1UL << f + r * 6;
				if ((block & (1UL << (f + r * 6))) != 0) break;
			}
			for (r = rank + 1, f = file - 1; r <= 5 && f >= 0; r++, f--) {
				result |= 1UL << f + r * 6;
				if ((block & (1UL << (f + r * 6))) != 0) break;
			}
			for (r = rank - 1, f = file + 1; r >= 0 && f <= 5; r--, f++) {
				result |= 1UL << f + r * 6;
				if ((block & (1UL << (f + r * 6))) != 0) break;
			}
			for (r = rank - 1, f = file - 1; r >= 0 && f >= 0; r--, f--) {
				result |= 1UL << f + r * 6;
				if ((block & (1UL << (f + r * 6))) != 0) break;
			}
			return result;
		}

		private static ulong RAtt(int sq, ulong block) {
			ulong result = 0UL;
			int rank = sq / 6, file = sq % 6, r, f;
			for (r = rank + 1; r <= 5; r++) {
				result |= 1UL << file + r * 6;
				if ((block & (1UL << (file + r * 6))) != 0) break;
			}
			for (r = rank - 1; r >= 0; r--) {
				result |= 1UL << file + r * 6;
				if ((block & (1UL << (file + r * 6))) != 0) break;
			}
			for (f = file + 1; f <= 5; f++) {
				result |= 1UL << f + rank * 6;
				if ((block & (1UL << (f + rank * 6))) != 0) break;
			}
			for (f = file - 1; f >= 0; f--) {
				result |= 1UL << f + rank * 6;
				if ((block & (1UL << (f + rank * 6))) != 0) break;
			}
			return result;
		}

		private static ulong RandomUlongFewBits() => RandomUlong() & RandomUlong() & RandomUlong();

		private static ulong RandomUlong() {
			var random = new Random();
			ulong u1, u2, u3, u4;
			u1 = (ulong) random.Next() & 0xFFFFUL; u2 = (ulong) random.Next() & 0xFFFFUL;
			u3 = (ulong) random.Next() & 0xFFFFUL; u4 = (ulong) random.Next() & 0xFFFFUL;
			return u1 | (u2 << 16) | (u3 << 32) | (u4 << 48);
		}

		private static int Transform(ulong b, ulong magic, int bits) => (int) ((b * magic) >> (64 - bits));
	}
}
