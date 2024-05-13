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

		_originalButton.onClick.AddListener(() => _fenPGNInput.text = Game.ORIGINAL_FEN.Fen);
		_nextGenButton.onClick.AddListener(() => _fenPGNInput.text = Game.NEXT_GEN_FEN.Fen);

		if (IsSelected(_originalButton)) _fenPGNInput.text = Game.ORIGINAL_FEN.Fen;
		else if (IsSelected(_nextGenButton)) _fenPGNInput.text = Game.NEXT_GEN_FEN.Fen;

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
	///Lets the user select a FEN or PGN file and if one is selected loads the content into the creation page
	///</summary>
	public void ImportFENPGN() {
		string contents = Game.Instance.ImportFENPGN();

		if (string.IsNullOrEmpty(contents)) return;

		bool isFEN = contents[0] != '[';

		_fenPGNInput.text = contents;
		SelectButton(_originalButton, false);
		SelectButton(_nextGenButton, false);
		SelectButton(_useFENButton, isFEN);
		SelectButton(_usePGNButton, !isFEN);
	}

	///<summary>
	///Creates the game based off the user selected settings
	///</summary>
	public void CreateGame() {
		if (!VerifySettings()) return;

		FEN fen = new();

		if (IsSelected(_originalButton)) fen = Game.ORIGINAL_FEN;
		else if (IsSelected(_nextGenButton)) fen = Game.NEXT_GEN_FEN;
		else if (IsSelected(_useFENButton)) fen = new(_fenPGNInput.text);
		else Game.Instance.StartPGN = new(_fenPGNInput.text);

		Game.Instance.Setup = fen;

		SettingsManager.Instance.ShowExportSettings(true);

		if (IsSelected(_localButton)) {
			Game.Instance.StartLocalGame();
			MenuController.Instance.PopAllPages();
			return;
		}
		if (IsSelected(_hostButton)) {
			MenuController.Instance.PushPage(_connectingPage);
			Game.Instance.InitializeHost();
			return;
		}

		SettingsManager.Instance.ShowAnalysisSettings(true);
		Game.Instance.StartLocalGame();
		MenuController.Instance.PopAllPages();
	}

	///<summary>
	///Verifies that the settings are valid, if they're not displays a message to change the settings
	///</summary>
	///<returns>Whether the settings are valid</returns>
	private bool VerifySettings() {
		bool valid = true;

		if (!IsSelected(_usePGNButton) && new FEN(_fenPGNInput.text).IsEmpty()) {
			MessageManager.Instance.CreateMessage("Must enter a valid FEN");
			valid = false;
		} else if (IsSelected(_usePGNButton) && !new PGN(_fenPGNInput.text).IsValid()) {
			MessageManager.Instance.CreateMessage("Must enter a valid PGN");
			valid = false;
		}

		if (IsSelected(_analysisButton)) return valid;

		if (IsSelected(_customButton) && _timeInput.text == "") {
			MessageManager.Instance.CreateMessage("Must enter time");
			valid = false;
		} else if (IsSelected(_customButton) && _incrementInput.text == "") {
			MessageManager.Instance.CreateMessage("Must enter increment");
			valid = false;
		}

		return valid;
	}

	///<summary>
	///Returns whether the given button is selected
	///</summary>
	private bool IsSelected(Button btn) => btn.GetComponent<Outline>().enabled;
}
