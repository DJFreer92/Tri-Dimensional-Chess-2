using UnityEngine;
using UnityEngine.UI;
using TMPro;

using TriDimensionalChess.Tools;

namespace TriDimensionalChess.UI {
	[DisallowMultipleComponent]
	public sealed class AttackBoardControls : MonoSingleton<AttackBoardControls> {
		private const string _WHITE_CHAR = "W", _BLACK_CHAR = "B";
		private const float _WHITE_FONT_SIZE = 14f, _BLACK_FONT_SIZE = 16f;

		private static readonly Color _WHITE_COLOR = Color.white, _BLACK_COLOR = Color.black;

		[SerializeField] private Button _rotateButton, _invertButton;
		[SerializeField] private TMP_Text _ownerTxt;
		[SerializeField] private Image _invertButtonImage;
		[SerializeField] private Sprite _invertImage;
		[SerializeField] private Sprite _uninvertImage;
		private bool _rotateEnabled, _invertEnabled;

        protected override void Awake() {
            base.Awake();

			bool[] enabledControls = Game.Game.Instance.GetEnabledOptions();
			_rotateEnabled = enabledControls[0];
			_invertEnabled = enabledControls[1];

			HideControls();
        }

		///<summary>
		///Show the attack board controls
		///</summary>
		///<param name="canRotate">Whether the rotate button can be used</param>
		///<param name="canInvert">Whether the invert button can be used</param>
		public void ShowControls(bool asWhite, bool abIsInverted, bool canRotate, bool canInvert) {
			SetOwner(asWhite);

			_invertButtonImage.sprite = abIsInverted ? _uninvertImage : _invertImage;

			_rotateButton.interactable = _rotateEnabled && canRotate;
			_invertButton.interactable = _invertEnabled && canInvert;

			gameObject.SetActive(true);
		}

		///<summary>
		///Hide the attack board controls
		///</summary>
		public void HideControls() => gameObject.SetActive(false);

		///<summary>
		///Returns whether the attack board controls are visible
		///</summary>
		///<returns>Whether the attack board controls are visible</returns>
		public bool AreControlsEnabled() => gameObject.activeSelf;

		///<summary>
		///Set the owner of the selected attack board in the controls
		///</summary>
		///<param name="isWhite">Whether the owner of the attack board is white</param>
		private void SetOwner(bool isWhite) {
			if (isWhite) {
				_ownerTxt.text = _WHITE_CHAR;
				_ownerTxt.color = _WHITE_COLOR;
				_ownerTxt.fontSize = _WHITE_FONT_SIZE;
				return;
			}

			_ownerTxt.text = _BLACK_CHAR;
			_ownerTxt.color = _BLACK_COLOR;
			_ownerTxt.fontSize = _BLACK_FONT_SIZE;
		}
	}
}
