using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PieceCreator : MonoSingleton<PieceCreator> {
	[SerializeField] private GameObject[] _piecesPrefabs;
	[SerializeField] private Transform _whiteParent;
	[SerializeField] private Transform _blackParent;

	private readonly Dictionary<PieceTypeColor, GameObject> _typeToPieceDict = new();

    protected override void Awake() {
        base.Awake();

		foreach (var piece in _piecesPrefabs) {
			ChessPiece component = piece.GetComponent<ChessPiece>();
			_typeToPieceDict.Add(component.Type.GetPieceTypeColor(component.IsWhite), piece);
		}
    }

	///<summary>
	///Creates a piece of the given type and color and places it on the given square
	///</summary>
	///<param name="tc">The type and color of the piece to be created</param>
	///<param name="sqr">The square to place the piece on</param>
	///<returns>The piece created</returns>
	public ChessPiece CreatePiece(PieceTypeColor tc, Square sqr) {
		//get the prefab and color of the piece
		GameObject prefab = _typeToPieceDict[tc];
		bool isWhite = tc.GetColor();

		//if the prefab was not found
		if (prefab == null) return null;

		//create the piece
		GameObject newPiece = Instantiate(
			prefab,
			sqr.gameObject.transform.position,
			Quaternion.identity,
			isWhite ? _whiteParent : _blackParent
		);

		//orient the piece, enable the collider, and place it on its square
		if (!isWhite) newPiece.transform.Rotate(Vector3.up * 180);
		newPiece.GetComponent<MeshCollider>().enabled = true;
		sqr.GamePiece = newPiece.GetComponent<ChessPiece>();

		//return the piece
		return sqr.GamePiece;
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
