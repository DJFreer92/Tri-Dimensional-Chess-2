using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Game))]
[DisallowMultipleComponent]
public class CapturedPiecesController : MonoSingleton<CapturedPiecesController> {
	private static readonly Type[] _SORT_ORDER = {typeof(Queen), typeof(Rook), typeof(Bishop), typeof(Knight), typeof(Pawn)};
	public Action<Type, bool> OnPieceCaptured, OnCapturedPieceRemoved;
	public Action OnCapturedPiecesCleared;
	[SerializeField] [Range(0.1f, 1f)] private float _pieceScale;
	[SerializeField] [Range(0.25f, 2f)] private float _xSpacing, _zSpacing;
	private List<ChessPiece> _capturedWhitePieces = new(), _capturedBlackPieces = new();
	private List<ChessPiece> _createdPieces = new();

	///<summary>
	///Adds the given piece to the captured pieces
	///</summary>
	///<param name="piece">Captured piece</param>
	public void AddPiece(ChessPiece piece) {
		if (piece is King) return;
		List<ChessPiece> capturedPieces = piece.IsWhite ? _capturedWhitePieces : _capturedBlackPieces;
		capturedPieces.Add(piece);
		UpdatePositions(capturedPieces);
		OnPieceCaptured?.Invoke(piece.GetType(), piece.IsWhite);
	}

	///<summary>
	///Removes a piece of the given type and color from the list of captured pieces.
	///If that piece does not exist it will create a piece of the opposing color and add it to the list of captured pieces
	///</summary>
	///<param name="type">The type of piece to be removed</param>
	///<param name="isWhite">Whether the piece to be removed is white</param>
	public void RemovePieceOfType(Type type, bool isWhite) {
		OnCapturedPieceRemoved?.Invoke(type, isWhite);
		List<ChessPiece> capturedPieces = isWhite ? _capturedWhitePieces : _capturedBlackPieces;
		foreach (ChessPiece piece in capturedPieces) {
			if (piece.GetType() != type) continue;
			Destroy(piece.gameObject);
			capturedPieces.Remove(piece);
			return;
		}
		
		ChessPiece newPiece = null;
		switch (Array.IndexOf(_SORT_ORDER, type)) {
			case 0: newPiece = Instantiate((isWhite ? Queen.BlackPrefab : Queen.WhitePrefab)).GetComponent<Queen>();
			break;
			case 1: newPiece = Instantiate((isWhite ? Rook.BlackPrefab : Rook.WhitePrefab)).GetComponent<Rook>();
			break;
			case 2: newPiece = Instantiate((isWhite ? Bishop.BlackPrefab : Bishop.WhitePrefab)).GetComponent<Bishop>();
			break;
			case 3: newPiece = Instantiate((isWhite ? Knight.BlackPrefab : Knight.WhitePrefab)).GetComponent<Knight>();
			break;
			default: return;
		}
		_createdPieces.Add(newPiece);
		newPiece.SetCaptured();
	}

	///<summary>
	///Clears all of the captured pieces
	///</summary>
	public void ClearCapturedPieces() {
		foreach (ChessPiece piece in _capturedWhitePieces) Destroy(piece.gameObject);
		foreach (ChessPiece piece in _capturedBlackPieces) Destroy(piece.gameObject);
		foreach (ChessPiece piece in _createdPieces) Destroy(piece.gameObject);

		_capturedWhitePieces = new();
		_capturedBlackPieces = new();
		_createdPieces = new();

		OnCapturedPiecesCleared?.Invoke();
	}

	///<summary>
	///Updates the positions of all the captured pieces
	///</summary>
	///<param name="capturedPieces">The list of captured pieces</param>
	private void UpdatePositions(List<ChessPiece> capturedPieces) {
		SortPieces(capturedPieces);
		for (var i = 0; i < capturedPieces.Count; i++) {
			GameObject obj = capturedPieces[i].gameObject;
			obj.transform.localScale = _pieceScale * Vector3.one;
			var position = new Vector3(2f, 0f, (_zSpacing * -2));
			position.z += (i % 5) * _zSpacing;
			position.x += (i / 5) * _xSpacing;
			if (capturedPieces[i].IsWhite) position.x *= -1;
			obj.transform.SetPositionAndRotation(position, obj.transform.rotation);
		}
	}

	///<summary>
	///Sorts the captured pieces in order of value
	///</summary>
	///<param name="capturedPieces">The list of captured pieces</param>
	private void SortPieces(List<ChessPiece> capturedPieces) {
		for (var i = 0; i < capturedPieces.Count - 1; i++) {
			ChessPiece highestPiece = capturedPieces[i];
			int highestIndex = GetTypePriority(highestPiece);
			for (var j = i + 1; j < capturedPieces.Count; j++) {
				int index = GetTypePriority(capturedPieces[j]);
				if (highestIndex >= index) continue;
				highestPiece = capturedPieces[j];
				highestIndex = index;
			}
			capturedPieces.Remove(highestPiece);
			capturedPieces.Insert(i, highestPiece);
		}
	}

	///<summary>
	///Returns the sort priority of the given piece
	///</summary>
	///<param name="piece">The piece</param>
	///<returns>The sort priority of the given piece</returns>
	private int GetTypePriority(ChessPiece piece) {
		for (var i = 0; i < _SORT_ORDER.Length; i++) {
			if (piece.GetType() == _SORT_ORDER[i]) return _SORT_ORDER.Length - i;
		}
		return -1;
	}
}