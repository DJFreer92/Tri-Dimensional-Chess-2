using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerControls : MonoBehaviour {
	public bool HasDrawOffer = false;
	[SerializeField] private Button _drawButton;
	[SerializeField] private bool _isWhitePieces;

	private void Start() {
		if (!_isWhitePieces) _drawButton.interactable = false;
	}

	private void Update() {
		if (HasDrawOffer) _drawButton.interactable = true;
	}

	///<summary>
	///Enable or disable the draw button in the UI
	///</summary>
	///<param name="enable">Whether the enable or disable the draw button</param>
	public void EnableDraw(bool enable) {
		if (!enable) HasDrawOffer = false;

		_drawButton.interactable = enable;
	}

	///<summary>
	///Resign the player from the game
	///</summary>
	public void ResignGame() {
		Game.Instance.State = _isWhitePieces ? GameState.WHITE_RESIGNATION : GameState.BLACK_RESIGNATION;
	}

	///<summary>
	///Offer a draw to the opposing player
	///</summary>
	public void RequestDraw() {
		if (HasDrawOffer) Game.Instance.State = GameState.DRAW_MUTUAL_AGREEMENT;
		else Game.Instance.OfferDraw(!_isWhitePieces);

		EnableDraw(false);
	}
}