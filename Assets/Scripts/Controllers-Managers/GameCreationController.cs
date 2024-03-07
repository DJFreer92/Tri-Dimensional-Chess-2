using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public sealed class GameCreationController : MonoSingleton<GameCreationController> {
	#region Buttons
	[Header("Buttons")]
	[SerializeField] private Button _localButton;
	[SerializeField] private Button _analysisButton;
	[SerializeField] private Button _hostButton;
	[SerializeField] private Button _originalButton;
	[SerializeField] private Button _nextGenButton;
	[SerializeField] private Button _useFENButton;
	[SerializeField] private Button _usePGNButton;
	[SerializeField] private Button _fiveZeroButton;
	[SerializeField] private Button _fiveThreeButton;
	[SerializeField] private Button _tenZeroButton;
	[SerializeField] private Button _tenFiveButton;
	[SerializeField] private Button _fifteenTenButton;
	[SerializeField] private Button _thirtyZeroButton;
	[SerializeField] private Button _thirtyTwentyButton;
	[SerializeField] private Button _sixtyZeroButton;
	[SerializeField] private Button _sixtyThirtyButton;
	[SerializeField] private Button _noneButton;
	[SerializeField] private Button _customButton;
	#endregion
	#region Input Fields
	[Header("Input Fields")]
	[SerializeField] private TMP_InputField _fenPGNInput;
	[SerializeField] private TMP_InputField _timeInput;
	[SerializeField] private TMP_InputField _incrementInput;
	#endregion
	#region Pages
	[SerializeField] private Page _connectingPage;
	[SerializeField] private Page _gameUIPage;
	#endregion
	private bool _changeAutomatic;

	protected override void Awake() {
		base.Awake();

		_originalButton.onClick.AddListener(() => _fenPGNInput.text = Game.ORIGINAL_FEN);
		_nextGenButton.onClick.AddListener(() => _fenPGNInput.text = Game.NEXT_GEN_FEN);

		if (IsSelected(_originalButton)) _fenPGNInput.text = Game.ORIGINAL_FEN;
		else if (IsSelected(_nextGenButton)) _fenPGNInput.text = Game.NEXT_GEN_FEN;

		_fenPGNInput.onValueChanged.AddListener((text) => {
			if (_changeAutomatic) {
				_changeAutomatic = false;
				return;
			}
			if (IsSelected(_originalButton)) {
				SelectButton(_originalButton, false);
				SelectButton(_useFENButton, true);
				return;
			}
			if (IsSelected(_nextGenButton)) {
				SelectButton(_nextGenButton, false);
				SelectButton(_useFENButton, true);
			}
		});
	}

	///<summary>
	///Selects or deselects the given button
	///</summary>
	private void SelectButton(Button btn, bool select) => btn.GetComponent<Outline>().enabled = select;

	///<summary>
	///Sets whether the FEN change was automatic or not
	///</summary>
	public void SetChangeAutomatic(bool auto) => _changeAutomatic = auto;

	///<summary>
	///Creates the game based off the user selected settings
	///</summary>
	public void CreateGame() {
		if (!VerifySettings()) return;

		if (IsSelected(_originalButton)) Game.Instance.Setup = Game.ORIGINAL_FEN;
		else if (IsSelected(_nextGenButton)) Game.Instance.Setup = Game.NEXT_GEN_FEN;
		else if (IsSelected(_useFENButton)) Game.Instance.Setup = _fenPGNInput.text;
		else Game.Instance.LoadPGN(_fenPGNInput.text);

		if (IsSelected(_localButton)) {
			Game.Instance.StartLocalGame();
			MenuController.Instance.PopAllPages();
			return;
		}
		if (IsSelected(_hostButton)) {
			MenuController.Instance.PushPage(_connectingPage);
			return;
		}
		//TO DO: Handle analysis
		SettingsManager.Instance.ShowAnalysisSettings(true);

	}

	///<summary>
	///Verify that the settings are valid, if they're not display a message to change the settings
	///</summary>
	///<returns>Whether the settings are valid</returns>
	private bool VerifySettings() {
		bool valid = true;
		if (!IsSelected(_localButton) && !IsSelected(_hostButton) && !IsSelected(_analysisButton)) {
			MessageManager.Instance.CreateMessage("Must select game mode");
			valid = false;
		}

		if (!IsSelected(_originalButton) && !IsSelected(_nextGenButton) && !IsSelected(_useFENButton) && !IsSelected(_usePGNButton)) {
			MessageManager.Instance.CreateMessage("Must select a variant");
			valid = false;
		} else if (IsSelected(_useFENButton) && _fenPGNInput.text == "") {
			MessageManager.Instance.CreateMessage("Must enter an FEN");
			valid = false;
		} else if (IsSelected(_usePGNButton) && _fenPGNInput.text == "") {
			MessageManager.Instance.CreateMessage("Must enter a PGN");
			valid = false;
		}

		if (!IsSelected(_fiveZeroButton) &&
			!IsSelected(_fiveThreeButton) &&
			!IsSelected(_tenZeroButton) &&
			!IsSelected(_tenFiveButton) &&
			!IsSelected(_fifteenTenButton) &&
			!IsSelected(_thirtyZeroButton) &&
			!IsSelected(_thirtyTwentyButton) &&
			!IsSelected(_sixtyZeroButton) &&
			!IsSelected(_sixtyThirtyButton) &&
			!IsSelected(_noneButton) &&
			!IsSelected(_customButton)
		) {
			MessageManager.Instance.CreateMessage("Must select a time control");
			valid = false;
		} else if (IsSelected(_customButton) && _timeInput.text == "") {
			MessageManager.Instance.CreateMessage("Must enter custom time control");
			valid = false;
		} else if (IsSelected(_customButton) && _incrementInput.text == "") {
			MessageManager.Instance.CreateMessage("Must enter custom time control");
			valid = false;
		}

		return valid;
	}

	///<summary>
	///Returns whether the given button is selected
	///</summary>
	private bool IsSelected(Button btn) => btn.GetComponent<Outline>().enabled;
}