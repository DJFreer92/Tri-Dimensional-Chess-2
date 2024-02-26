using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PromotionController : MonoSingleton<PromotionController> {
	[Header("References")]
	[SerializeField] private Page _promotionPage;
	[SerializeField] private GameObject _whiteBtns, _blackBtns;
	[SerializeField] private Button _whiteQueenBtn, _whiteRookBtn, _whiteBishopBtn, _whiteKnightBtn, _blackQueenBtn, _blackRookBtn, _blackBishopBtn, _blackKnightBtn;
	public bool SelectionInProgress {get => _moveInProgress != null;}
	private Move _moveInProgress;

	protected override void Awake() {
		base.Awake();

		_whiteQueenBtn.onClick.AddListener(() => Promote(PieceType.QUEEN));
		_whiteRookBtn.onClick.AddListener(() => Promote(PieceType.ROOK));
		_whiteBishopBtn.onClick.AddListener(() => Promote(PieceType.BISHOP));
		_whiteKnightBtn.onClick.AddListener(() => Promote(PieceType.KNIGHT));
		_blackQueenBtn.onClick.AddListener(() => Promote(PieceType.QUEEN));
		_blackRookBtn.onClick.AddListener(() => Promote(PieceType.ROOK));
		_blackBishopBtn.onClick.AddListener(() => Promote(PieceType.BISHOP));
		_blackKnightBtn.onClick.AddListener(() => Promote(PieceType.KNIGHT));
	}

	///<summary>
	///Opens the promotation panel
	///</summary>
	///<param name="move">The move</param>
	public void ShowPromotionOptions(Move move) {
		_moveInProgress = move;

		//if auto queen is enabled, automatically promote to a queen
		if (SettingsManager.Instance.AutoQueen) {
			Promote(PieceType.QUEEN);
			return;
		}

		_whiteBtns.SetActive(move.Player.IsWhite);
		_blackBtns.SetActive(!move.Player.IsWhite);

		//focus the queen button
		_promotionPage.SetFirstFocusItem((move.Player.IsWhite ? _whiteQueenBtn : _blackQueenBtn).gameObject);

		//open the promotion page
		MenuController.Instance.PushPage(_promotionPage);
	}

	///<summary>
	///Stops the selection process
	///</summary>
	public void StopSelection() {
		_moveInProgress = null;
	}

	///<summary>
	///Promotes the pawn to the given piece type
	///</summary>
	///<param name="type">The type of piece<param>
	private void Promote(PieceType type) {
		_moveInProgress.Promotion = type;
		_moveInProgress = null;
	}
}