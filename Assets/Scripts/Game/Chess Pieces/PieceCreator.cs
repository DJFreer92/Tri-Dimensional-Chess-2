using System.Collections.Generic;
using UnityEngine;

using TriDimensionalChess.Game.Boards;
using TriDimensionalChess.Tools;

namespace TriDimensionalChess.Game.ChessPieces {
	[DisallowMultipleComponent]
	public sealed class PieceCreator : MonoSingleton<PieceCreator> {
		[SerializeField] private GameObject[] _piecesPrefabs;
		[SerializeField] private Transform _piecesParent;

		private readonly Dictionary<PieceTypeColor, GameObject> _typeToPieceDict = new();

		protected override void Awake() {
			base.Awake();

			ChessPiece piece;
			foreach (var prefab in _piecesPrefabs) {
				piece = prefab.GetComponent<ChessPiece>();
				_typeToPieceDict.Add(piece.Type.GetPieceTypeColor(piece.IsWhite), prefab);
			}
		}

		///<summary>
		///Creates a piece of the given type and color and places it on the given square
		///</summary>
		///<param name="ptc">The type and color of the piece to be created</param>
		///<param name="sqr">The square to place the piece on</param>
		///<returns>The created piece</returns>
		public ChessPiece CreatePiece(PieceTypeColor ptc, Square sqr) {
			//get the prefab and color of the piece
			GameObject prefab = _typeToPieceDict[ptc];
			bool isWhite = ptc.GetColor();

			//if the prefab was not found, return null
			if (prefab == null) return null;

			//create the piece
			GameObject newPiece = Instantiate(
				prefab,
				sqr.gameObject.transform.position,
				Quaternion.identity,
				sqr.Brd.gameObject.transform
			);

			//orient the piece, enable the collider, and place it on its square
			if (!isWhite) newPiece.transform.Rotate(Vector3.up * 180);
			newPiece.GetComponent<MeshCollider>().enabled = true;
			sqr.GamePiece = newPiece.GetComponent<ChessPiece>();

			//return the piece
			return sqr.GamePiece;
		}

		///<summary>
		///Creates a piece of the given type and color but does not place it on the board
		///</summary>
		///<param name="ptc">The type and color of the piece to be created</param>
		///<returns>The created piece</returns>
		public ChessPiece CreatePiece(PieceTypeColor ptc) {
			//get the prefab of the piece
			GameObject prefab = _typeToPieceDict[ptc];

			//if the prefab was not found, return null
			if (prefab == null) return null;

			//create and return the piece
			return Instantiate(prefab, _piecesParent).GetComponent<ChessPiece>();
		}

		///<summary>
		///Converts the given piece into the given piece type
		///</summary>
		///<param name="piece">The piece to be converted</param>
		///<param name="type">The piece type to convert the piece to</param>
		///<returns>The converted piece</returns>
		public ChessPiece ConvertPiece(ChessPiece piece, PieceType type) {
			//if the desired type is the same as the piece's type, return the piece
			if (piece.Type == type) return piece;

			//create the new piece and put it on the old pieces square
			Square sqr = piece.GetSquare();
			sqr.GamePiece = CreatePiece(type.GetPieceTypeColor(piece.IsWhite), sqr);
			sqr.GamePiece.SetAtStart(false);

			//return the piece
			return sqr.GamePiece;
		}
	}
}
