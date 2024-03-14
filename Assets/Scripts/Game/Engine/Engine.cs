using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

using static CardinalDirections;
using static PieceTypeColor;
using static ESquare;

public enum CardinalDirections : byte {
	NORTH = 0,
	EAST = 1,
	SOUTH = 2,
	WEST = 3,
	NORTH_EAST = 4,
	SOUTH_EAST = 5,
	SOUTH_WEST = 6,
	NORTH_WEST = 7
}

readonly struct Magic {
	public Magic(ulong[] attackTable, ulong mask, ulong magic, int shift) {
		this.attackTable = attackTable;
		this.mask = mask;
		this.magic = magic;
		this.shift = shift;
	}

	public readonly ulong[] attackTable;
	public readonly ulong mask, magic;
	public readonly int shift;
}

public sealed class Engine : MonoSingleton<Engine> {
	private const bool WHITE = true;
	private const bool BLACK = false;
	private const ulong UNUSED_BITS_MASK = 0xFFFFFFFFFFFFFFFUL;
	private const ulong WHITE_BOARD = 0x1E79E780UL;
	private const ulong NEUTRAL_BOARD = 0x1E79E780000UL;
	private const ulong BLACK_BOARD = 0x1E79E780000000UL;
	private const ulong NO_NON_EXIST = 0xCFFFFFFFFFFFFF3UL;
	private const ulong NOT_Z_FILE = 0x7DF7DF7DF7DF7DFUL;
	private const ulong NOT_A_FILE = 0xBEFBEFBEFBEFBEFUL;
	private const ulong NOT_D_FILE = 0xF7DF7DF7DF7DF7DUL;
	private const ulong NOT_E_FILE = 0xFBEFBEFBEFBEFBEUL;
	public static bool IsInitialized {get; private set;}
	//lookup tables
	private static Dictionary<ulong, int> _bitLookup;
	private static Dictionary<CardinalDirections, ulong[]> _rays;
	private static ulong[] _kingMoves;
	private static ulong[] _knightMoves;
	private static Dictionary<bool, ulong[]> _pawnSinglePush;
	private static Dictionary<bool, ulong[]> _pawnDoublePush;
	private static Dictionary<bool, ulong[]> _pawnAttacks;
	//current position variables
	private ulong[] _boards;
	private ulong _legalSqrs;
	private Dictionary<PieceTypeColor, ulong>[] _positions;

	protected override void Awake() {
		base.Awake();

		Init();
	}

	public static void Init() {
		if (IsInitialized) return;

		//create bit lookup
		_bitLookup = new();
		for (int i = 0; i < 64; i++) _bitLookup.Add(1UL << i, i);

		//create the rays
		_rays = new Dictionary<CardinalDirections, ulong[]>() {
			{NORTH, new ulong[60]},
			{EAST, new ulong[60]},
			{SOUTH, new ulong[60]},
			{WEST, new ulong[60]},
			{NORTH_EAST, new ulong[60]},
			{SOUTH_EAST, new ulong[60]},
			{SOUTH_WEST, new ulong[60]},
			{NORTH_WEST, new ulong[60]}
		};

		//generate lookup tables
		for (int i = 0; i < 60; i++) {
			//generate rays
			for (int j = i + 6; j < 60; j += 6) _rays[NORTH][i] |= 1UL << j;  //North
			_rays[NORTH][i] &= NO_NON_EXIST;
			for (int j = i + 1; j % 6 != 0; j++) _rays[EAST][i] |= 1UL << j;  //East
			_rays[EAST][i] &= NO_NON_EXIST;
			for (int j = i - 6; j >= 0; j -= 6) _rays[SOUTH][i] |= 1UL << j;  //South
			_rays[SOUTH][i] &= NO_NON_EXIST;
			for (int j = i - 1; (j + 1) % 6 != 0; j--) _rays[WEST][i] |= 1UL << j;  //West
			_rays[WEST][i] &= NO_NON_EXIST;
			for (int j = i + 7; j % 6 != 0 && j < 60; j += 7) _rays[NORTH_EAST][i] |= 1UL << j;  //North-East
			_rays[NORTH_EAST][i] &= NO_NON_EXIST;
			for (int j = i - 5; j % 6 != 0 && j >= 0; j -= 5) _rays[SOUTH_EAST][i] |= 1UL << j;  //South-East
			_rays[SOUTH_EAST][i] &= NO_NON_EXIST;
			for (int j = i - 7; (j + 1) % 6 != 0 && j >= 0; j -= 7) _rays[SOUTH_WEST][i] |= 1UL << j;  //South-West
			_rays[SOUTH_WEST][i] &= NO_NON_EXIST;
			for (int j = i + 5; (j + 1) % 6 != 0 && j < 60; j += 5) _rays[NORTH_WEST][i] |= 1UL << j;  //North-West
			_rays[NORTH_WEST][i] &= NO_NON_EXIST;

			//generate king moves
			_kingMoves = new ulong[60];
			_kingMoves[i] = 1UL << i;
			_kingMoves[i] |= EOne(_kingMoves[i]) | WOne(_kingMoves[i]);
			_kingMoves[i] |= NOne(_kingMoves[i]) | SOne(_kingMoves[i]);
			_kingMoves[i] &= NO_NON_EXIST;

			//generate knight moves
			_knightMoves = new ulong[60];
			int iModSix = i % 6;
			if (iModSix != 0) {
				_knightMoves[i] = (
					(1UL << (i + 13) & NOT_Z_FILE) |
					(1UL >> (i + 11) & NOT_Z_FILE)
				);
				if (iModSix != 1) _knightMoves[i] |= (
					(1UL << (i + 8) & NOT_Z_FILE & NOT_A_FILE) |
					(1UL >> (i + 4) & NOT_Z_FILE & NOT_A_FILE)
				);
			}
			if (iModSix != 5) {
				_knightMoves[i] |= (
					(1UL >> (i + 13) & NOT_E_FILE) |
					(1UL << (i + 11) & NOT_E_FILE)
				);
				if (iModSix != 4) _knightMoves[i] |= (
					(1UL >> (i + 8) & NOT_D_FILE & NOT_E_FILE) |
					(1UL << (i + 4) & NOT_D_FILE & NOT_E_FILE)
				);
			}
			_knightMoves[i] &= NO_NON_EXIST;

			//generate pawn pushes
			_pawnSinglePush = new() {
				{WHITE, new ulong[60]},
				{BLACK, new ulong[60]}
			};
			_pawnDoublePush = new() {
				{WHITE, new ulong[60]},
				{BLACK, new ulong[60]}
			};
			_pawnAttacks = new() {
				{WHITE, new ulong[60]},
				{BLACK, new ulong[60]}
			};
			if (i >= 6 && i < 54) {
				//white pawns
				_pawnSinglePush[WHITE][i] = 1UL << (i + 6);  //single square
				//double square
				if ((i == 6 || i == 7) || (i == 10 || i == 11) || (i >= 13 && i <= 16))
					_pawnDoublePush[WHITE][i] = 1UL << (i + 12);
				_pawnDoublePush[WHITE][i] &= NO_NON_EXIST;

				//black pawns
				_pawnSinglePush[BLACK][i] = 1UL << (i - 6);  //single square
				//double square
				if ((i == 53 || i == 52) || (i == 49 || i == 48) || (i <= 46 && i >= 43))
					_pawnDoublePush[BLACK][i] = 1UL << (i - 12);
				_pawnDoublePush[BLACK][i] &= NO_NON_EXIST;
			}

			//generate pawn attacks
			//white pawns
			if (i % 5 != 0) _pawnAttacks[WHITE][i] = 1UL << (i + 7);  //North-East
			if (iModSix != 0) _pawnAttacks[WHITE][i] = 1UL << (i + 5);  //North-West
			_pawnAttacks[WHITE][i] &= NO_NON_EXIST;
			//black pawns
			if (i % 5 != 0) _pawnAttacks[BLACK][i] = 1UL >> (i + 5);  //South-East
			if (iModSix != 0) _pawnAttacks[BLACK][i] = 1UL >> (i + 7);  //South-West
			_pawnAttacks[BLACK][i] &= NO_NON_EXIST;
		}

		IsInitialized = true;
	}

	public void UpdatePositions(ChessBoard board) {
		UpdateBoard(board);
		_positions = new Dictionary<PieceTypeColor, ulong>[_boards.Length];
		for (int i = 0; i < board.Boards.Count; i++) {
			for (PieceTypeColor ptc = WHITE_KING; ptc <= BLACK_PAWN; ptc++) {
				PieceType type = ptc.GetPieceType();
				bool isWhite = ptc < PieceTypeColor.BLACK_KING;
				foreach (Square sqr in board.Boards[i]) {
					if (sqr.GamePiece.IsWhite != isWhite || sqr.GamePiece.Type != type) continue;
					_positions[i][ptc] |= 1UL << (sqr.Coords.z * 6 + sqr.Coords.x);
				}
			}
		}
	}

	public void CalculateMoves(bool forWhite) {
		ulong[] blockers = GetBlockers();

		var maskedSamePieces = new ulong[_boards.Length];
		var opposingPieces = new ulong[_boards.Length];
		for (int i = 0; i < _positions.Length; i++) {
			foreach (KeyValuePair<PieceTypeColor, ulong> kv in _positions[i]) {
				if (kv.Key.GetColor() == forWhite) maskedSamePieces[i] |= kv.Value;
				else opposingPieces[i] |= kv.Value;
			}
			maskedSamePieces[i] = ~maskedSamePieces[i];
		}
	}

	private void UpdateBoard(ChessBoard board) {
		_boards = new ulong[] {WHITE_BOARD, NEUTRAL_BOARD, BLACK_BOARD};
		int boardIndex = 3;
		foreach (Board brd in board) {
			if (brd is not AttackBoard) continue;
			foreach (Square sqr in brd)
				_boards[boardIndex] |= 1UL << (sqr.Coords.z * 6 + sqr.Coords.x);
		}
		SetLegalSqrs();
	}

	private void SetLegalSqrs() {
		_legalSqrs = 0UL;
		foreach (ulong brd in _boards) _legalSqrs |= brd;
	}

	private ulong[] GetBlockers() {
		var blockersBBs = new ulong[_boards.Length];
		for (int i = 0; i < _positions.Length; i++)
			foreach (KeyValuePair<PieceTypeColor, ulong> typeColor in _positions[i])
				blockersBBs[i] |= typeColor.Value;
		return blockersBBs;
	}

	private ulong[] GetKingMovesClassical(int kingIndex, ulong[] maskedSamePieces) {
		var attacksBBs = new ulong[_boards.Length];
		for (int i = 0; i < attacksBBs.Length; i++)
			attacksBBs[i] = _kingMoves[kingIndex] & maskedSamePieces[i] & _boards[i];
		return attacksBBs;
	}

	private ulong[] GetQueenMovesClassical(ulong queenIndex, ulong[] blockers, ulong[] maskedSamePieces) {
		ulong[] moves = GetBishopMovesClassical(queenIndex, blockers, maskedSamePieces);
		ulong[] rookMoves = GetRookMovesClassical(queenIndex, blockers, maskedSamePieces);
		for (int i = 0; i < moves.Length; i++) moves[i] |= rookMoves[i];
		return moves;
	}

	private ulong[] GetRookMovesClassical(ulong rookIndex, ulong[] blockers, ulong[] maskedSamePieces) {
		var attacksBBs = new ulong[blockers.Length];

		for (int i = 0; i < attacksBBs.Length; i++) {
			for (CardinalDirections direction = NORTH; direction <= WEST; direction++) {
				attacksBBs[i] |= _rays[direction][rookIndex];
				ulong maskedBlockers = _rays[direction][rookIndex] & blockers[i];
				if (maskedBlockers > 0) {
					int blockerIndex = (
						(direction == SOUTH || direction == WEST) ?
						BitScanner.BitScanReverse(maskedBlockers) :
						BitScanner.BitScanForward(maskedBlockers)
					);
					attacksBBs[i] &= ~_rays[direction][blockerIndex];
				}
			}
			attacksBBs[i] &= maskedSamePieces[i] & _boards[i];
		}

		return attacksBBs;
	}

	private ulong[] GetBishopMovesClassical(ulong bishopIndex, ulong[] blockers, ulong[] maskedSamePieces) {
		var attacksBBs = new ulong[blockers.Length];

		for (int i = 0; i < attacksBBs.Length; i++) {
			for (CardinalDirections direction = NORTH_EAST; direction <= NORTH_WEST; direction++) {
				attacksBBs[i] |= _rays[direction][bishopIndex];
				ulong maskedBlockers = _rays[direction][bishopIndex] & blockers[i];
				if (maskedBlockers > 0UL) {
					int blockerIndex = (
						(direction == SOUTH_EAST || direction == SOUTH_WEST) ?
						BitScanner.BitScanReverse(maskedBlockers) :
						BitScanner.BitScanForward(maskedBlockers)
					);
					attacksBBs[i] &= ~_rays[direction][blockerIndex];
				}
			}
			attacksBBs[i] &= maskedSamePieces[i] & _boards[i];
		}

		return attacksBBs;
	}

	private ulong[] GetKnightMovesClassical(int knightIndex, ulong[] maskedSamePieces) {
		var attacksBBs = new ulong[_boards.Length];
		for (int i = 0; i < attacksBBs.Length; i++)
			attacksBBs[i] = _knightMoves[knightIndex] & maskedSamePieces[i] & _boards[i];
		return attacksBBs;
	}

	private ulong[] GetPawnMovesClassical(int kingIndex, ulong[] maskedSamePieces, ulong[] opposingPieces, bool isWhite) {
		var attacksBBs = new ulong[_boards.Length];
		for (int i = 0; i < attacksBBs.Length; i++) {
			attacksBBs[i] = _pawnSinglePush[isWhite][kingIndex] & maskedSamePieces[i] & _boards[i];
			if (attacksBBs[i] > 0) attacksBBs[i] |= _pawnDoublePush[isWhite][kingIndex];
			attacksBBs[i] |= _pawnAttacks[isWhite][kingIndex] & opposingPieces[i];
			attacksBBs[i] &= maskedSamePieces[i] & _boards[i];
		}
		return attacksBBs;
	}

	private static ulong NOne(ulong BB) => BB << 6;
	private static ulong EOne(ulong BB) => (BB & NOT_E_FILE) << 1;
	private static ulong SOne(ulong BB) => BB >> 6;
	private static ulong WOne(ulong BB) => (BB & NOT_Z_FILE) >> 1;
	private static ulong NEOne(ulong BB) => (BB & NOT_E_FILE) << 7;
	private static ulong SEOne(ulong BB) => (BB & NOT_E_FILE) >> 7;
	private static ulong SWOne(ulong BB) => (BB & NOT_Z_FILE) >> 5;
	private static ulong NWOne(ulong BB) => (BB & NOT_Z_FILE) << 5;

	public static void PrintBitboard(ulong bb) {
		string bitStr = Convert.ToString((long) bb, 2).PadLeft(64, '0');
		var bbStr = new StringBuilder(bitStr.Substring(0, 4)).Append('\n');
		bitStr = bitStr.Remove(0, 4);
		for (int i = 0; i < bitStr.Length; i += 6) {
			for (int j = 5; j >= 0; j--) bbStr.Append(bitStr[i + j]);
			bbStr.Append('\n');
		}
		Debug.Log(bbStr);
	}




	//MAGIC
	private readonly Magic[] _bishopTable = new Magic[60];
	private readonly Magic[] _rookTable = new Magic[60];

	private ulong BishopAttacks(ulong occ, ESquare sqr) {
		occ &= _bishopTable[(int) sqr].mask;
		occ *= _bishopTable[(int) sqr].magic;
		occ >>= _bishopTable[(int) sqr].shift;
		return _bishopTable[(int) sqr].attackTable[occ];
		/*return _bishopTable[(int) sqr].attackTable[
			(occ &
			_bishopTable[(int) sqr].mask) *
			_bishopTable[(int) sqr].magic >>
			_bishopTable[(int) sqr].shift
		];*/
	}

	private ulong RookAttacks(ulong occ, ESquare sqr) {
		occ &= _rookTable[(int) sqr].mask;
		occ *= _rookTable[(int) sqr].magic;
		occ >>= _rookTable[(int) sqr].shift;
		return _rookTable[(int) sqr].attackTable[occ];
	}

	private void IterateSubsets(ulong set) {
		ulong sub = 0UL;
		do {

			sub = (sub - set) & set;
		} while (sub != 0UL);
	}



}