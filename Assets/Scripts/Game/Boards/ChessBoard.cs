using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[DisallowMultipleComponent]
public sealed class ChessBoard : MonoSingleton<ChessBoard>, IEnumerable {
	//coordinates of castling pieces
	#region Castling Coordinates
	public static Vector3Int WhiteKingCoords = new Vector3Int(4, 1, 0);
	public static Vector3Int BlackKingCoords = new Vector3Int(4, 5, 9);
	public static Vector3Int WhiteKingSideRookCoords = new Vector3Int(5, 1, 0);
	public static Vector3Int WhiteQueenSideRookCoords = Vector3Int.up;
	public static Vector3Int BlackKingSideRookCoords = new Vector3Int(5, 5, 9);
	public static Vector3Int BlackQueenSideRookCoords = new Vector3Int(0, 5, 9);
	#endregion
	//x and z directions
	private static int[][] _XZDirections = new int[][] {
		new int[] {-1, -1},
		new int[] {-1, 0},
		new int[] {-1, 1},
		new int[] {0, -1},
		new int[] {0, 1},
		new int[] {1, -1},
		new int[] {1, 0},
		new int[] {1, 1}
	};
	//the square positions of each possible attackboard position
	private static Dictionary<string, Vector3Int> _ABPins = new() {
		{"QL1", new Vector3Int(1, 0, 1)},
		{"QL2", new Vector3Int(1, 0, 4)},
		{"QL3", new Vector3Int(1, 2, 3)},
		{"QL4", new Vector3Int(1, 2, 6)},
		{"QL5", new Vector3Int(1, 4, 5)},
		{"QL6", new Vector3Int(1, 4, 8)},
		{"KL1", new Vector3Int(4, 0, 1)},
		{"KL2", new Vector3Int(4, 0, 4)},
		{"KL3", new Vector3Int(4, 2, 3)},
		{"KL4", new Vector3Int(4, 2, 6)},
		{"KL5", new Vector3Int(4, 4, 5)},
		{"KL6", new Vector3Int(4, 4, 8)}
	};
	//piece's gameobject parents
	[field: SerializeField] public Transform WhitePiecesParent {get; private set;}
	[field: SerializeField] public Transform BlackPiecesParent {get; private set;}
	//attackboard prefab gameobject
	[SerializeField] private GameObject _attackboardPrefab;
	//holds the boards of the chessboard
	[field: SerializeField] public List<Board> Boards {get; private set;}

	///<summary>
	///Constructs a chessboard position from a FEN position
	///</summary>
	///<param name="fen">FEN position</param>
	public void ConstructPosition(string fen) {
		//clear the current position
		Clear();

		//get relevant sections of FEN
		string[] sections = fen.Split(' ');
		string attackboards = sections[0];
		string pieces = sections[1];
		string currentPlayer = sections[2];
		string castling = sections[3];
		string enPassant = sections[4];

		//attackboards
		if (attackboards != "-") {
			for (int i = 0; i < (attackboards.Length / 2); i++) {
				string atckbrd = attackboards.Substring(i * 2, 2);
				bool queenSide = Char.IsLower(atckbrd[0]);
				int rank = (int) Char.GetNumericValue(atckbrd[^1]);
				Square pinSqr = GetSquareAt(_ABPins[(queenSide ? "QL" : "KL") + atckbrd[^1]]);
				pinSqr.IsOccupiedByAB = true;
				GameObject abGameObject = GameObject.Instantiate(
					_attackboardPrefab,
					new Vector3(
						pinSqr.gameObject.transform.position.x + (queenSide ? -0.5f : 0.5f),
						pinSqr.gameObject.transform.position.y + 1.5f,
						pinSqr.gameObject.transform.position.z + ((rank % 2 == 1) ? -0.5f : 0.5f)
					),
					Quaternion.identity,
					ChessBoard.Instance.gameObject.transform
				);
				AttackBoard ab = abGameObject.GetComponent<AttackBoard>();
				Boards.Insert(i, ab);
				ab.Owner = (Ownership) Char.ToUpper(atckbrd[0]);
				ab.Y = pinSqr.Coords.y + 1;
				ab.SetPinnedSquare(pinSqr);
				foreach (Square sqr in ab) {
					sqr.Coords += new Vector3Int(
						queenSide ? 0 : 4,
						ab.Y,
						rank + (rank % 2 == 0 ? 2 : -1)
					);
				}
			}
		}

		//pieces
		//pieces on main boards
		string[] boardPieces = pieces.Split('|');
		for (int i = 0; i < 3; i++) {
			Board brd = Boards[Boards.Count - (3 - i)];
			string[] ranksOfPieces = boardPieces[i].Split('/');

			for (int z = 0; z < 4; z++) {
				string rankPieces = ranksOfPieces[z];
				int piecesIndex = 0;
				for (int x = 1; x <= 4; x++, piecesIndex++) {
					if (Char.IsDigit(rankPieces[piecesIndex])) {
						x += (int) Char.GetNumericValue(rankPieces[piecesIndex]) - 1;
						continue;
					}
					Square sqr = brd.GetSquareAt(new Vector3Int(x, (4 - i * 2), (8 - i * 2 - z)));
					sqr.GamePiece = CreatePiece(sqr, rankPieces[piecesIndex]);
				}
			}
		}
		//pieces on attackboards
		for (int i = 3; i < Boards.Count; i++) {
			AttackBoard ab = Boards[i - 3] as AttackBoard;
			string[] ranksOfPieces = boardPieces[i].Split('/');

			int startX = ab.PinnedSquare.Coords.x;
			if (Char.IsLower(attackboards[(i - 3) * 2])) startX--;
			int y = ab.PinnedSquare.Coords.y + 1;
			int startZ = ab.PinnedSquare.Coords.z;
			if (attackboards[(i - 3) * 2 + 1] % 2 == 0) startZ++;

			for (int z = 0; z < 2; z++) {
				string rankPieces = ranksOfPieces[z];
				int piecesIndex = 0;
				for (int x = 0; x < 2; x++, piecesIndex++) {
					if (Char.IsDigit(rankPieces[piecesIndex])) {
						x += (int) Char.GetNumericValue(rankPieces[piecesIndex]) - 1;
						continue;
					}
					Square sqr = ab.Squares[z * 2 + x];
					sqr.GamePiece = CreatePiece(sqr, rankPieces[piecesIndex]);
				}
			}
			ab.SetBoardAnnotation();
		}

		//castling
		if (castling != "-") {
			foreach (Square sqr in GetEnumerableSquares()) {
				if (sqr.GamePiece is Rook) (sqr.GamePiece as Rook).HasCastlingRights = false;
			}
			foreach (char castlingRight in castling) {
				foreach(Square sqr in GetEnumerableSquares()) {
					if (sqr.GamePiece is not Rook || sqr.GamePiece.IsWhite != Char.IsUpper(castlingRight)) continue;
					Rook rook = sqr.GamePiece as Rook;
					if ((castlingRight == 'K' && sqr.Coords == WhiteKingSideRookCoords) ||
						(castlingRight == 'k' && sqr.Coords == BlackKingSideRookCoords)
					) rook.SetKingSide(true);
					if (rook.IsKingSide != (Char.ToUpper(castlingRight) == 'K')) continue;
					rook.SetCastlingRights(true);
				}
			}
			if (castling.ToLower() == castling) {
				GetKing(true).HasCastlingRights = false;
			} else if (castling.ToUpper() == castling) {
				GetKing(false).HasCastlingRights = false;
			}
		}

		//en passant
		if (enPassant != "-") {
			(GetSquareAt(
				new Vector3Int(enPassant[0].FileToIndex(),
				enPassant.Substring(2).BoardToIndex(),
				(int) Char.GetNumericValue(enPassant[1])
			)).GamePiece as Pawn).JustMadeDSMove = true;
		}

		//update check states
		UpdateKingCheckState(currentPlayer == "w");

		//inner functions
		ChessPiece CreatePiece(Square sqr, char pieceChar) {
			ChessPiece piece = Instantiate(
				ChessPiece.GetPrefab(pieceChar.CharToPieceColor()),
				sqr.gameObject.transform.position,
				Quaternion.identity,
				Char.IsUpper(pieceChar) ? WhitePiecesParent : BlackPiecesParent
			).GetComponent<ChessPiece>();
			if (Char.ToUpper(pieceChar) == 'P') (piece as Pawn).RevokeDSMoveRights();
			if (Char.IsLower(pieceChar)) piece.gameObject.transform.Rotate(Vector3.up * 180);
			piece.gameObject.GetComponent<MeshCollider>().enabled = true;
			piece.SetWhite(Char.IsUpper(pieceChar));
			if (Char.ToUpper(pieceChar) == 'B') (piece as Bishop).SquareColorIsWhite = sqr.IsWhite;
			return piece;
		}
	}

	///<summary>
	///Clear the chess board
	///</summary>
	public void Clear() {
		for (int i = 0; i < Boards.Count; i++) {
			Boards[i].Clear();
			if (Boards[i] is AttackBoard) Boards.RemoveAt(i--);
		}
	}

	///<summary>
	///Returns the square at the specified coordinates
	///</summary>
	///<param name="coords">The coordinates of the square</param>
	///<returns>The square at the specified coordinates</returns>
	public Square GetSquareAt(Vector3Int coords) {
		foreach (Board brd in Boards) {
			Square sqr = brd.GetSquareAt(coords);
			if (sqr != null) return sqr;
		}
		//no square found at the specified coordinates
		return null;
	}

	///<summary>
	///Returns the square with the given piece
	///</summary>
	///<param name="piece">Chess piece on the desired square</param>
	///<returns>The square with the given piece</returns>
	public Square GetSquareWithPiece(ChessPiece piece) {
		foreach (Board brd in Boards) {
			Square sqr = brd.GetSquareWithPiece(piece);
			if (sqr != null) return sqr;
		}
		//no square found with the specified piece
		return null;
	}

	///<summary>
	///Return the board containing the given square
	///</summary>
	///<param name="sqr">The square the board should contain</param>
	///<returns>The board containing the given square</returns>
	public Board GetBoardWithSquare(Square sqr) {
		foreach (Board brd in Boards) {
			if (brd.Squares.Contains(sqr)) return brd;
		}
		//no board found with the specified square
		return null;
	}

	///<summary>
	///Returns an enumerable for all the squares on the board
	///</summary>
	///<returns>An enumerable for all the squares on the board</returns>
	public IEnumerable<Square> GetEnumerableSquares() {
		foreach (Board brd in Boards) {
			foreach (Square sqr in brd) {
				yield return sqr;
			}
		}
	}

	///<summary>
	///Returns the King of the given pieces
	///</summary>
	///<param name="isWhite">Whether the King should be of the white pieces</param>
	///<returns>The King of the given pieces</returns>
	public King GetKing(bool isWhite) {
		foreach (Square sqr in GetEnumerableSquares()) {
			if (sqr.GamePiece is King && sqr.GamePiece.IsWhite == isWhite) return sqr.GamePiece as King;
		}
		return null;
	}

	///<summary>
	///Returns a list of all the pieces on the chess board
	///</summary>
	///<returns>A list of all the pieces on the chess board</returns>
	public List<ChessPiece> GetPieces() {
		var pieces = new List<ChessPiece>();
		foreach (Board brd in Boards) {
			pieces.AddRange(brd.GetPieces());
		}
		return pieces;
	}

	///<summary>
	///Returns whether any pieces are attacking the given square
	///</summary>
	///<param name="square">The square to find pieces attacking</param>
	///<param name="attackPiecesAreWhite">Whether the attacking pieces should be white</param>
	///<returns>Whether the are any pieces attacking the given square</returns>
	public bool ArePiecesAttacking(Square square, bool attackingPiecesAreWhite) {
		//check for attacking pieces
		foreach (int[] direction in _XZDirections) {
			//get the x and z coordinates of the square
			int x = square.Coords.x;
			int z = square.Coords.z;

			bool end = false;
			while (!end) {
				//modify the x and z coordinate to search for attackers
				x += direction[0];
				z += direction[1];

				//if the x or z coordinate is out of the boards bounds break out of the loop
				if (x < 0 || x >= 5 || z < 0 || z >= 9) break;

				//loop for Squares at the desired coordinates
				foreach (Square sqr in GetEnumerableSquares()) {
					//if there is not a piece on the square or the coordinates do not match the coordinates of the square, continue checking squares
					if (!sqr.HasPiece() || sqr.Coords.x != x || sqr.Coords.z != z) continue;

					//get the piece on the square
					ChessPiece piece = sqr.GamePiece;

					//mark for the while loop to end after this iteration
					end = true;

					//if the piece is not the color of the attacking pieces, continue checking Squares
					if (attackingPiecesAreWhite != piece.IsWhite) continue;

					switch (piece.Type) {
						case PieceType.KING:
							int xDiff = Math.Abs(square.Coords.x - x);
							int zDiff = Math.Abs(square.Coords.z - z);
							//if the piece is one square away, return attacker found
							if (xDiff <= 1 && zDiff <= 1 && xDiff + zDiff != 0) return true;
						continue;
						case PieceType.QUEEN: return true;
						case PieceType.ROOK:
							//if the direction isn't diagonal, return attacker found
							if (direction[0] == 0 || direction[1] == 0) return true;
						continue;
						case PieceType.BISHOP:
							//if the direction is diagonal, return attacker found
							if (direction[0] != 0 && direction[1] != 0) return true;
						continue;
						case PieceType.KNIGHT: continue;
						case PieceType.PAWN:
							//if the pawn is in range to attack the piece and the pawn is facing the correct direction, return attacker found
							if (direction[0] != 0 &&
								direction[1] != 0 &&
								(Math.Abs(square.Coords.x - x) == 1) &&
								direction[1] == (piece.IsWhite ? -1 : 1)
							) return true;
						continue;
					}
				}
			}
		}

		//loop through all the possible knight offsets
		foreach (int[] offset in Knight.Offsets) {
			//get the x and z coordinates of the square and modify by the knight offset
			int x = square.Coords.x + offset[0];
			int z = square.Coords.z + offset[1];

			//loop for Squares at the desired coordinates
			foreach (Square sqr in GetEnumerableSquares()) {
				//if the square does not have a piece, continue checking Squares
				if (!sqr.HasPiece()) continue;

				//if the piece is not the color of the attacking pieces, continue checking Squares
				if (attackingPiecesAreWhite != sqr.GamePiece.IsWhite) continue;

				//if there is a knight at the desired coordinates, return attacker found
				if (sqr.Coords.x == x && sqr.Coords.z == z && sqr.GamePiece is Knight) return true;
			}
		}

		//no attackers found
		return false;
	}

	///<summary>
	///Update the status of whether the king of the given color is in check
	///</summary>
	///<param name="isWhite">Whether the king to be updated is white</param>
	public void UpdateKingCheckState(bool isWhite) {
		GetKing(isWhite).UpdateCheckState();
	}

	///<summary>
	///Returns whether the given player has a move
	///</summary>
	///<param name="isWhite">Whether the player is white</param>
	///<returns>Whether the given player has a move</returns>
	public bool DoesPlayerHaveMove(bool isWhite) {
		foreach (Board brd in Boards) {
			if (brd is AttackBoard && (brd as AttackBoard).GetAvailableMoves(isWhite).Count > 0) return true;
			foreach (Square sqr in brd) {
				if (!sqr.HasPiece() || sqr.GamePiece.IsWhite != isWhite) continue;
				if (sqr.GamePiece.GetAvailableMoves(isWhite).Count > 0) return true;
			}
		}
		return false;
	}

	///<summary>
	///Returns whether the king of the specified pieces is in check
	///</summary>
	///<param name="isWhite">Whether the king is white</param>
	///<returns>Whether the king of the specified pieces is in check</returns>
	public bool IsKingInCheck(bool isWhite) {
		//return whether the king is in check
		return GetKing(isWhite).IsInCheck;
	}

	///<summary>
	///Returns an evaluation of whether the king is in check in the current position
	///</summary>
	///<param name="isWhite">Whether the king is white</param>
	///<returns>An evaluation of whether the king is in check in the current position</returns>
	public bool GetKingCheckEvaluation(bool isWhite) {
		return GetKing(isWhite).DetermineCheck();
	}

	///<summary>
	///Returns whether there is sufficient material left on the board to continue the game
	///</summary>
	///<returns>Whether the chess board is a dead position</returns>
	public bool IsDeadPosition() {
		//get all the pieces on the gameboard
		List<ChessPiece> pieces = GetPieces();

		switch (pieces.Count) {  //the number of pieces on the board
			case 2: return true;  //2 pieces on the board, there is insufficient material to continue the game
			case 3:  //3 pieces on the board
				foreach (ChessPiece piece in pieces) {
					if (piece is Knight) return true;  //there is insufficient material to continue the game
				}
				break;
			case 4:  //4 pieces on the board
				Bishop whiteBishop = null, blackBishop = null;
				foreach (ChessPiece piece in pieces) {
					if (piece is King) continue;
					if (piece is not Bishop) return false;  //there is sufficient material to continue the game
					if (piece.IsWhite) whiteBishop = piece as Bishop;
					else blackBishop = piece as Bishop;
				}
				//return whether there is sufficent material to continue the game based on whether the bishops are on opposite colors or not
				return whiteBishop.SquareColorIsWhite == blackBishop.SquareColorIsWhite;
		}

		//there is sufficent material to continue the game
		return false;
	}

	///<summary>
	///Whether the given player has sufficient material to win the game on time
	///</summary>
	///<param name="player">The player</param>
	///<returns>Whether the given player has sufficient material to win the game on time</returns>
	public bool HasMaterialToWinOnTime(Player player) {
		//get all the pieces on the game board
		List<ChessPiece> pieces = GetPieces();

		//remove all the pieces of the opposite color
		for (var i = 0; i < pieces.Count; i++) {
			if (pieces[i].IsWhite != player.IsWhite) pieces.RemoveAt(i--);
		}

		switch (pieces.Count) {
			case 1: return false;
			case 2:
				foreach (ChessPiece piece in pieces) {
					if (piece is Bishop || piece is Knight) return false;
				}
				return true;
			case 3:
				foreach (ChessPiece piece in pieces) {
					if (piece is not King && piece is not Knight) return true;
				}
				return false;
		}
		return true;
	}

	///<summary>
	///Returns the pieces behind the given square
	///</summary>
	///<param name="square">The square to check for pieces behind</param>
	///<param name="isBehindWhite">Whether the pieces being searched for is behind a white piece</param>
	///<returns>An enumerable piece behind the given square</returns>
	public IEnumerable<ChessPiece> GetPiecesBehind(Square square, bool isBehindWhite) {
		//get the z coordinate for the square behind the given square
		int z = square.Coords.z + (isBehindWhite ? -1 : 1);

		//loop all the Squares
		foreach (Square sqr in GetEnumerableSquares()) {
			//if the coordinates match and the square has a piece, yield and return the piece
			if (sqr.Coords.x == square.Coords.x && sqr.Coords.z == z && sqr.HasPiece()) yield return sqr.GamePiece;
		}
	}

	///<summary>
	///Returns the list of boards as an enumerator
	///</summary>
	///<returns>The list of boards as an enumerator</returns>
	public IEnumerator GetEnumerator() {
		return Boards.GetEnumerator();
	}

	///<summary>
	///returns a string of data about the chess board
	///</summary>
	///<returns>A string of data about the chess board</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		foreach (Board brd in Boards) {
			str.Append("\n").Append(brd.ToString()).Append("\n");
		}
		return str.ToString();
	}
}