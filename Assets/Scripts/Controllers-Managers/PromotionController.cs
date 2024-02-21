using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PromotionController : MonoSingleton<PromotionController> {
	[Header("References")]
	[SerializeField] private Page _promotionPage;
	[SerializeField] private GameObject _whiteBtns, _blackBtns;
	[SerializeField] private Button _whiteQueenBtn, _whiteRookBtn, _whiteBishopBtn, _whiteKnightBtn, _blackQueenBtn, _blackRookBtn, _blackBishopBtn, _blackKnightBtn;

	private Move _moveInProgress;

	protected override void Awake() {
		base.Awake();

		_whiteQueenBtn.onClick.AddListener(() => Promote(typeof(Queen)));
		_whiteRookBtn.onClick.AddListener(() => Promote(typeof(Rook)));
		_whiteBishopBtn.onClick.AddListener(() => Promote(typeof(Bishop)));
		_whiteKnightBtn.onClick.AddListener(() => Promote(typeof(Knight)));
		_blackQueenBtn.onClick.AddListener(() => Promote(typeof(Queen)));
		_blackRookBtn.onClick.AddListener(() => Promote(typeof(Rook)));
		_blackBishopBtn.onClick.AddListener(() => Promote(typeof(Bishop)));
		_blackKnightBtn.onClick.AddListener(() => Promote(typeof(Knight)));
	}

	///<summary>
	///Opens the promotation panel
	///</summary>
	///<param name="move">The move</param>
	public void ShowPromotionOptions(Move move) {
		_moveInProgress = move;

		//if auto queen is enabled, automatically promote to a queen
		if (SettingsManager.Instance.AutoQueen) {
			Promote(typeof(Queen));
			return;
		}

		_whiteBtns.SetActive(move.Player.IsWhite);
		_blackBtns.SetActive(!move.Player.IsWhite);

		MenuController.Instance.PushPage(_promotionPage);
	}

	///<summary>
	///Cancels the promotion in progress
	///</summary>
	public void OnCancelPromotionButton() {
		_moveInProgress = null;
	}

	///<summary>
	///Promotes the pawn to the given piece type
	///</summary>
	///<param name="type">The type of piece<param>
	private void Promote(Type type) {
		_moveInProgress.Promotion = type;
		_moveInProgress.Execute();
		_moveInProgress = null;
	}
}