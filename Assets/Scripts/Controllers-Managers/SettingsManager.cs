using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Game))]
[DisallowMultipleComponent]
public class SettingsManager : MonoSingleton<SettingsManager> {
	//listeners
	public Action<bool> OnAutoQueenChange, OnFigurineNotationChange, OnTestModeChange;

	[field: Header("Settings")]
	[field: SerializeField] public bool AutoQueen {get; private set;}
	[field: SerializeField] public bool FigurineNotation {get; private set;}
	[field: SerializeField] public bool TestMode {get; private set;}
	[Header("References")]
	[SerializeField] private GameObject _analysisSettings;
	[SerializeField] private TMP_InputField _fenPGNInput;
	[SerializeField] private Button _setFENButton;
	[SerializeField] private Button _setPGNButton;

    protected override void Awake() {
		base.Awake();

		_setFENButton.onClick.AddListener(SetFEN);
		_setPGNButton.onClick.AddListener(SetPGN);
    }

    protected override void OnDestroy() {
        base.OnDestroy();

		_setFENButton.onClick.RemoveListener(SetFEN);
		_setPGNButton.onClick.RemoveListener(SetPGN);
    }

    ///<summary>
    ///Toggle the auto queen setting
    ///</summary>
    public void ToggleAutoQueen() {
		AutoQueen = !AutoQueen;
		OnAutoQueenChange?.Invoke(AutoQueen);
	}

	///<summary>
	///Toggle the figurine notation setting
	///</summary>
	public void ToggleFigurineNotation() {
		FigurineNotation = !FigurineNotation;
		OnFigurineNotationChange?.Invoke(FigurineNotation);
	}

	///<summary>
	///Toggle the test mode setting
	///</summary>
	public void ToggleTestMode() {
		TestMode = !TestMode;
		OnTestModeChange?.Invoke(FigurineNotation);
	}

	///<summary>
	///Shows or hides the analysis settings
	///</summary>
	///<param name="show">Whether to show or hide the analysis settings</param>
	public void ShowAnalysisSettings(bool show) {
		_analysisSettings.SetActive(show);
	}

	///<summary>
	///Sets the fen in the input field as the game setup
	///</summary>
	private void SetFEN() {
		if (!FENBuilder.VerifyFEN(_fenPGNInput.text)) {
			MessageManager.Instance.CreateMessage("Invalid FEN");
			return;
		}

		Game.Instance.Setup = _fenPGNInput.text;
		Game.Instance.StartPGN = null;
		Game.Instance.Init();
		MessageManager.Instance.CreateMessage("FEN Set");
	}

	///<summary>
	///Sets the pgn in the input field as the current game state
	///</summary>
	private void SetPGN() {
		if (!PGNBuilder.VerifyPGN(_fenPGNInput.text)) {
			MessageManager.Instance.CreateMessage("Invalid PGN");
			return;
		}

		Game.Instance.Setup = null;
		Game.Instance.StartPGN = _fenPGNInput.text;
		Game.Instance.Init();
		MessageManager.Instance.CreateMessage("PGN Set");
	}

	///<summary>
	///Allows the user to select a FEN or PGN file and puts the content of the file into the FEN/PGN input field
	///</summary>
	public void ImportFENPGN() {
		string contents = Game.Instance.ImportFENPGN();
		if (!string.IsNullOrEmpty(contents)) _fenPGNInput.text = contents;
	}
}