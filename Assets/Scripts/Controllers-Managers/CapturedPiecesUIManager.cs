using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class CapturedPiecesUIManager : MonoSingleton<CapturedPiecesUIManager> {
	private static readonly Dictionary<Type, int> _MATERIAL_POINTS = new() {
		{typeof(Queen), 9},
		{typeof(Rook), 5},
		{typeof(Bishop), 3},
		{typeof(Knight), 3},
		{typeof(Pawn), 1}
	};
	private readonly Dictionary<Type, int> _capturedWhitePieces = new() {
		{typeof(Queen), 0},
		{typeof(Rook), 0},
		{typeof(Bishop), 0},
		{typeof(Knight), 0},
		{typeof(Pawn), 0}
	};
	private readonly Dictionary<Type, int> _capturedBlackPieces = new() {
		{typeof(Queen), 0},
		{typeof(Rook), 0},
		{typeof(Bishop), 0},
		{typeof(Knight), 0},
		{typeof(Pawn), 0}
	};
	private readonly Dictionary<Type, GameObject> _whitePieceIconPrefabs = new() {
		{typeof(Queen), null},
		{typeof(Rook), null},
		{typeof(Bishop), null},
		{typeof(Knight), null},
		{typeof(Pawn), null}
	};
	private readonly Dictionary<Type, GameObject> _blackPieceIconPrefabs = new() {
		{typeof(Queen), null},
		{typeof(Rook), null},
		{typeof(Bishop), null},
		{typeof(Knight), null},
		{typeof(Pawn), null}
	};

	#region Constants
	[Header("Constants")]
	[SerializeField] private float _differentIconSpacing;
	[SerializeField] private float _likeIconSpacing;
	[SerializeField] private float _pointTextSpacing;
	#endregion

	#region References
	[Header("References")]
	[SerializeField] private Transform _whiteCapturedPiecesUIParent;
	[SerializeField] private Transform _blackCapturedPiecesUIParent;
	[SerializeField] private TMP_Text _whitePointsText;
	[SerializeField] private TMP_Text _blackPointsText;
	#endregion

	#region Prefabs
	[Header("Prefabs")]
	[SerializeField] private GameObject _whiteQueenIconPrefab;
	[SerializeField] private GameObject _whiteRookIconPrefab;
	[SerializeField] private GameObject _whiteBishopIconPrefab;
	[SerializeField] private GameObject _whiteKnightIconPrefab;
	[SerializeField] private GameObject _whitePawnIconPrefab;
	[SerializeField] private GameObject _blackQueenIconPrefab;
	[SerializeField] private GameObject _blackRookIconPrefab;
	[SerializeField] private GameObject _blackBishopIconPrefab;
	[SerializeField] private GameObject _blackKnightIconPrefab;
	[SerializeField] private GameObject _blackPawnIconPrefab;
	#endregion

	private Dictionary<Type, List<GameObject>> _whitePieceIconGameObjects = new() {
		{typeof(Queen), new List<GameObject>()},
		{typeof(Rook), new List<GameObject>()},
		{typeof(Bishop), new List<GameObject>()},
		{typeof(Knight), new List<GameObject>()},
		{typeof(Pawn), new List<GameObject>()}
	};
	private Dictionary<Type, List<GameObject>> _blackPieceIconGameObjects = new() {
		{typeof(Queen), new List<GameObject>()},
		{typeof(Rook), new List<GameObject>()},
		{typeof(Bishop), new List<GameObject>()},
		{typeof(Knight), new List<GameObject>()},
		{typeof(Pawn), new List<GameObject>()}
	};

	protected override void Awake() {
		_whitePieceIconPrefabs[typeof(Queen)] = _whiteQueenIconPrefab;
		_whitePieceIconPrefabs[typeof(Rook)] = _whiteRookIconPrefab;
		_whitePieceIconPrefabs[typeof(Bishop)] = _whiteBishopIconPrefab;
		_whitePieceIconPrefabs[typeof(Knight)] = _whiteKnightIconPrefab;
		_whitePieceIconPrefabs[typeof(Pawn)] = _whitePawnIconPrefab;

		_blackPieceIconPrefabs[typeof(Queen)] = _blackQueenIconPrefab;
		_blackPieceIconPrefabs[typeof(Rook)] = _blackRookIconPrefab;
		_blackPieceIconPrefabs[typeof(Bishop)] = _blackBishopIconPrefab;
		_blackPieceIconPrefabs[typeof(Knight)] = _blackKnightIconPrefab;
		_blackPieceIconPrefabs[typeof(Pawn)] = _blackPawnIconPrefab;
	}

	private void OnEnable() {
		//register to events
		RegisterToEvents();
	}

	private void OnDisable() {
		//unregister to events
		UnregisterToEvents();
	}

	///<summary>
	///Adds local methods to listeners
	///</summary>
	private void RegisterToEvents() {
		CapturedPiecesController.Instance.OnPieceCaptured += AddPiece;
		CapturedPiecesController.Instance.OnCapturedPieceRemoved += RemovePiece;
		CapturedPiecesController.Instance.OnCapturedPiecesCleared += ClearCapturedPieces;
	}

	///<summary>
	///Removes local methods from listeners
	///</summary>
	private void UnregisterToEvents() {
		if (CapturedPiecesController.IsCreated()) {
			CapturedPiecesController.Instance.OnPieceCaptured -= AddPiece;
			CapturedPiecesController.Instance.OnCapturedPieceRemoved -= RemovePiece;
			CapturedPiecesController.Instance.OnCapturedPiecesCleared -= ClearCapturedPieces;
		}
	}

	///<summary>
	///Adds a captured piece to the UI
	///</summary>
	///<param name="pieceType">The type of piece to add</param>
	///<param name="isWhite">Whether the piece is white</param>
    public void AddPiece(Type pieceType, bool isWhite) {
		var capturedPieces = isWhite ? _capturedWhitePieces : _capturedBlackPieces;
		capturedPieces[pieceType]++;
		UpdateUI();
	}

	///<summary>
	///Removes a piece from the captured pieces, or adds the piece to the opposing player's captured pieces if there isn't one to remove
	///</summary>
	///<param name="pieceType">The type of piece to remove</param>
	///<param name="isWhite">Whether the piece is white</param>
	public void RemovePiece(Type pieceType, bool isWhite) {
		var capturedPieces = isWhite ? _capturedWhitePieces : _capturedBlackPieces;
		if (capturedPieces[pieceType] == 0) {
			AddPiece(pieceType, !isWhite);
			return;
		}
		capturedPieces[pieceType]--;
		UpdateUI();
	}

	///<summary>
	///Clears all the captured pieces
	///</summary>
	public void ClearCapturedPieces() {
		ClearCapturedPieces(true);
		ClearCapturedPieces(false);

		_whitePointsText.text = "";
		_blackPointsText.text = "";
	}

	///<summary>
	///Clear the pieces of the given color
	///</summary>
	///<param name="forWhite">Whether the pieces to clear are white</param>
	private void ClearCapturedPieces(bool forWhite) {
		var capturedPieces = forWhite ? _capturedWhitePieces : _capturedBlackPieces;
		foreach (var kv in capturedPieces) capturedPieces[kv.Key] = 0;

		foreach (var kv in (forWhite ? _whitePieceIconGameObjects : _blackPieceIconGameObjects)) {
			foreach (GameObject obj in kv.Value) Destroy(obj);
			kv.Value.Clear();
		}
	}

	///<summary>
	///Updates the UI with the pieces each player has captured and their point advantage
	///</summary>
	private void UpdateUI() {
		//ADD UI THEN REMOVE!!!!!!!
		return;
		
		UpdateIcons(true);
		UpdateIcons(false);

		SetPointAdvantage();
	}

	///<summary>
	///Updates the icons for the captured pieces of the given color
	///</summary>
	///<param name="forWhite">Whether the icons to update are white</param>
	private void UpdateIcons(bool forWhite) {
		var capturedPieces = forWhite ? _capturedWhitePieces : _capturedBlackPieces;
		var icons = forWhite ? _whitePieceIconGameObjects : _blackPieceIconGameObjects;
		var prefabSet = forWhite ? _whitePieceIconPrefabs : _blackPieceIconPrefabs;
		var parent = forWhite ? _whiteCapturedPiecesUIParent : _blackCapturedPiecesUIParent;

		foreach (var kv in capturedPieces) {
			Type pieceType = kv.Key;
			int numCaptured = kv.Value;
			int numIcons = icons[pieceType].Count;

			if (numCaptured == numIcons) continue;
			
			if (numIcons < numCaptured) {
				icons[pieceType].Add(Instantiate(prefabSet[pieceType], parent, false));
				continue;
			}

			int index = numIcons - 1;
			Destroy(icons[pieceType][index]);
			icons[pieceType].RemoveAt(index);
		}

		SpaceIcons(forWhite);
	}

	///<summary>
	///Properly space the different piece icons apart in the UI
	///</summary>
	///<param name="forWhite">Whether the icons to be spaced are white</param>
	private void SpaceIcons(bool forWhite) {
			var icons = forWhite ? _whitePieceIconGameObjects : _blackPieceIconGameObjects;
			Vector3 position = (forWhite ? _whiteCapturedPiecesUIParent : _blackCapturedPiecesUIParent).transform.position;
			int direction = forWhite ? 1 : -1;
			float likeSpacing = _likeIconSpacing * direction;
			float differentSpacing = _differentIconSpacing * direction;

			foreach (var kv in icons) {
				foreach (GameObject icon in kv.Value) {
					icon.transform.position = position;
					position.x += likeSpacing;
				}
				if (kv.Value.Count > 0) position.x -= likeSpacing;
				position.x += differentSpacing;
			}

			position.x += _pointTextSpacing * direction;
			(forWhite ? _whitePointsText : _blackPointsText).transform.position = position;
	}

	///<summary>
	///Set the point advantage of the players
	///</summary>
	private void SetPointAdvantage() {
		int whitePoints = CalculatePoints(true);
		int blackPoints = CalculatePoints(false);
		if (whitePoints == blackPoints) {
			_whitePointsText.text = "";
			_blackPointsText.text = "";
		} else if (whitePoints > blackPoints) {
			whitePoints -= blackPoints;
			_blackPointsText.text = "";
		} else {
			blackPoints -= whitePoints;
			_whitePointsText.text = "";
		}
	}

	///<summary>
	///Calculates the points of material gained of the given color pieces
	///</summary>
	///<param name="isWhite">Whether the points to calculate are for white</param>
	///<returns>The points of material gained of the given color pieces</returns>
	private int CalculatePoints(bool isWhite) {
		var capturedPieces = isWhite ? _capturedWhitePieces : _capturedBlackPieces;
		int total = 0;
		foreach (var kv in capturedPieces) {
			total += kv.Value * _MATERIAL_POINTS[kv.Key];
		}
		return total;
	}
}