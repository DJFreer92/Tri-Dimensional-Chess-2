using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public sealed class CapturedPiecesUIManager : MonoSingleton<CapturedPiecesUIManager> {
	private static readonly Dictionary<PieceType, int> _MATERIAL_POINTS = new() {
		{PieceType.QUEEN, 9},
		{PieceType.ROOK, 5},
		{PieceType.BISHOP, 3},
		{PieceType.KNIGHT, 3},
		{PieceType.PAWN, 1}
	};
	private readonly Dictionary<PieceType, int> _capturedWhitePieces = new() {
		{PieceType.QUEEN, 0},
		{PieceType.ROOK, 0},
		{PieceType.BISHOP, 0},
		{PieceType.KNIGHT, 0},
		{PieceType.PAWN, 0}
	};
	private readonly Dictionary<PieceType, int> _capturedBlackPieces = new() {
		{PieceType.QUEEN, 0},
		{PieceType.ROOK, 0},
		{PieceType.BISHOP, 0},
		{PieceType.KNIGHT, 0},
		{PieceType.PAWN, 0}
	};
	private readonly Dictionary<PieceType, GameObject> _whitePieceIconPrefabs = new() {
		{PieceType.QUEEN, null},
		{PieceType.ROOK, null},
		{PieceType.BISHOP, null},
		{PieceType.KNIGHT, null},
		{PieceType.PAWN, null}
	};
	private readonly Dictionary<PieceType, GameObject> _blackPieceIconPrefabs = new() {
		{PieceType.QUEEN, null},
		{PieceType.ROOK, null},
		{PieceType.BISHOP, null},
		{PieceType.KNIGHT, null},
		{PieceType.PAWN, null}
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

	private Dictionary<PieceType, List<GameObject>> _whitePieceIconGameObjects = new() {
		{PieceType.QUEEN, new List<GameObject>()},
		{PieceType.ROOK, new List<GameObject>()},
		{PieceType.BISHOP, new List<GameObject>()},
		{PieceType.KNIGHT, new List<GameObject>()},
		{PieceType.PAWN, new List<GameObject>()}
	};
	private Dictionary<PieceType, List<GameObject>> _blackPieceIconGameObjects = new() {
		{PieceType.QUEEN, new List<GameObject>()},
		{PieceType.ROOK, new List<GameObject>()},
		{PieceType.BISHOP, new List<GameObject>()},
		{PieceType.KNIGHT, new List<GameObject>()},
		{PieceType.PAWN, new List<GameObject>()}
	};

	protected override void Awake() {
		_whitePieceIconPrefabs[PieceType.QUEEN] = _whiteQueenIconPrefab;
		_whitePieceIconPrefabs[PieceType.ROOK] = _whiteRookIconPrefab;
		_whitePieceIconPrefabs[PieceType.BISHOP] = _whiteBishopIconPrefab;
		_whitePieceIconPrefabs[PieceType.KNIGHT] = _whiteKnightIconPrefab;
		_whitePieceIconPrefabs[PieceType.PAWN] = _whitePawnIconPrefab;

		_blackPieceIconPrefabs[PieceType.QUEEN] = _blackQueenIconPrefab;
		_blackPieceIconPrefabs[PieceType.ROOK] = _blackRookIconPrefab;
		_blackPieceIconPrefabs[PieceType.BISHOP] = _blackBishopIconPrefab;
		_blackPieceIconPrefabs[PieceType.KNIGHT] = _blackKnightIconPrefab;
		_blackPieceIconPrefabs[PieceType.PAWN] = _blackPawnIconPrefab;
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
    public void AddPiece(PieceType pieceType, bool isWhite) {
		var capturedPieces = isWhite ? _capturedWhitePieces : _capturedBlackPieces;
		capturedPieces[pieceType]++;
		UpdateUI();
	}

	///<summary>
	///Removes a piece from the captured pieces, or adds the piece to the opposing player's captured pieces if there isn't one to remove
	///</summary>
	///<param name="pieceType">The type of piece to remove</param>
	///<param name="isWhite">Whether the piece is white</param>
	public void RemovePiece(PieceType pieceType, bool isWhite) {
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
		//ADD UI THEN UNCOMMENT!!!!!!!

		/*ClearCapturedPieces(true);
		ClearCapturedPieces(false);

		_whitePointsText.text = "";
		_blackPointsText.text = "";*/
	}

	///<summary>
	///Clear the pieces of the given color
	///</summary>
	///<param name="forWhite">Whether the pieces to clear are white</param>
	private void ClearCapturedPieces(bool forWhite) {
		var capturedPieces = forWhite ? _capturedWhitePieces : _capturedBlackPieces;
		for (int i = 0; i < capturedPieces.Count; i++) capturedPieces[capturedPieces.ElementAt(i).Key] = 0;

		foreach (var kv in (forWhite ? _whitePieceIconGameObjects : _blackPieceIconGameObjects)) {
			foreach (GameObject obj in kv.Value) Destroy(obj);
			kv.Value.Clear();
		}
	}

	///<summary>
	///Updates the UI with the pieces each player has captured and their point advantage
	///</summary>
	private void UpdateUI() {
		//ADD UI THEN UNCOMMENT!!!!!!!

		/*UpdateIcons(true);
		UpdateIcons(false);

		SetPointAdvantage();*/
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
			PieceType pieceType = kv.Key;
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