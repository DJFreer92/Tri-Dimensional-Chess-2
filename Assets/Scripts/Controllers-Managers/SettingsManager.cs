using System;
using UnityEngine;

[RequireComponent(typeof(Game))]
[DisallowMultipleComponent]
public class SettingsManager : MonoSingleton<SettingsManager> {
	//listeners
	public Action<bool> OnAutoQueenChange, OnFigurineNotationChange, OnTestModeChange;

	[field: Header("Settings")]
	[field: SerializeField] public bool AutoQueen {get; private set;}
	[field: SerializeField] public bool FigurineNotation {get; private set;}
	[field: SerializeField] public bool TestMode {get; private set;}
	[SerializeField] private GameObject _analysisSettings;

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
}