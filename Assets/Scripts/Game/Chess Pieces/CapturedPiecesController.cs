using System;
using System.Collections.Generic;
using UnityEngine;

using TriDimensionalChess.Tools;

namespace TriDimensionalChess.Game.ChessPieces {
	[RequireComponent(typeof(Game))]
	[DisallowMultipleComponent]
	public sealed class CapturedPiecesController : MonoSingleton<CapturedPiecesController> {
		public Action<PieceType, bool> OnPieceCaptured, OnCapturedPieceRemoved;
		public Action OnCapturedPiecesCleared;
		[SerializeField] [Range(0.1f, 1f)] private float _pieceScale;
		[SerializeField] [Range(0.25f, 2f)] private float _xSpacing, _zSpacing;
		private List<ChessPiece> _capturedWhitePieces = new(), _capturedBlackPieces = new(), _placeholderPieces = new();

		///<summary>
		///Adds a piece of the given type and color to the captured pieces
		///</summary>
		///<param name="type">The type of piece to be added</param>
		///<param name="isWhite">Whether the piece being added is white</param>
		///<param name="isPlaceholderPiece">Whether the piece being added is a placeholder for a piece of the opposite color that couldn't be removed</param>
		public void AddPieceOfType(PieceType type, bool isWhite, bool isPlaceholderPiece = false) {
			Debug.Log("AddPieceOfType Called");
			if (type == PieceType.KING) return;
			ChessPiece placeholder = FindInPlaceholders(type, !isWhite);
			List<ChessPiece> capturedPieces;
			if (placeholder != null) {
				capturedPieces = isWhite ? _capturedBlackPieces : _capturedWhitePieces;
				capturedPieces.Remove(placeholder);
				_placeholderPieces.Remove(placeholder);
				Destroy(placeholder.gameObject);
			} else {
				ChessPiece newPiece = PieceCreator.Instance.CreatePiece(type.GetPieceTypeColor(isWhite));
				newPiece.enabled = false;
				capturedPieces = isWhite ? _capturedWhitePieces : _capturedBlackPieces;
				capturedPieces.Add(newPiece);
				if (isPlaceholderPiece) _placeholderPieces.Add(newPiece);
			}
			UpdatePositions(capturedPieces);
			OnPieceCaptured?.Invoke(type, isWhite);
		}

		///<summary>
		///Removes a piece of the given type and color from the list of captured pieces.
		///If that piece does not exist it will create a piece of the opposing color and add it to the list of captured pieces
		///</summary>
		///<param name="type">The type of piece to be removed</param>
		///<param name="isWhite">Whether the piece to be removed is white</param>
		public void RemovePieceOfType(PieceType type, bool isWhite) {
			OnCapturedPieceRemoved?.Invoke(type, isWhite);
			List<ChessPiece> capturedPieces = isWhite ? _capturedWhitePieces : _capturedBlackPieces;
			ChessPiece placeholder = FindInPlaceholders(type, isWhite);
			if (placeholder != null) {
				capturedPieces.Remove(placeholder);
				_placeholderPieces.Remove(placeholder);
				Destroy(placeholder);
				return;
			}
			foreach (ChessPiece piece in capturedPieces) {
				if (piece.Type != type) continue;
				capturedPieces.Remove(piece);
				Destroy(piece.gameObject);
				return;
			}

			AddPieceOfType(type, !isWhite, true);
		}

		///<summary>
		///Clears all of the captured pieces
		///</summary>
		public void ClearCapturedPieces() {
			foreach (ChessPiece piece in _capturedWhitePieces) Destroy(piece.gameObject);
			foreach (ChessPiece piece in _capturedBlackPieces) Destroy(piece.gameObject);

			_capturedWhitePieces = new();
			_capturedBlackPieces = new();
			_placeholderPieces = new();

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
				var position = new Vector3(2f, 0f, _zSpacing * -2);
				position.z += i % 5 * _zSpacing;
				position.x += i / 5 * _xSpacing;
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
				byte highestPriority = (byte) highestPiece.Type;
				for (var j = i + 1; j < capturedPieces.Count; j++) {
					byte priority = (byte) capturedPieces[j].Type;
					if (highestPriority >= priority) continue;
					highestPiece = capturedPieces[j];
					highestPriority = priority;
				capturedPieces.Remove(highestPiece);
				capturedPieces.Insert(i, highestPiece);
				}
			}
		}

		///<summary>
		///Returns a piece of the given type and color in the placeholder pieces, returns null if there isn't a mathcing piece in the placeholders
		///</summary>
		///<param name="type">The type of piece to be found</param>
		///<param name="isWhite">Whether the piece being found is white</param>
		private ChessPiece FindInPlaceholders(PieceType type, bool isWhite) {
			foreach (ChessPiece piece in _placeholderPieces)
				if (piece.Type == type && piece.IsWhite == isWhite) return piece;
			return null;
		}
	}
}
