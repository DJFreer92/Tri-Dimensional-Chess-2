using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class MoveLogManager : MonoSingleton<MoveLogManager> {
	[SerializeField] private GameObject _rowPrefab, _firstRow;
	[SerializeField] private Transform _moveLogBodyTransform;
	private readonly List<GameObject> _rows = new();

	private void OnEnable() => RegisterToEvents();

	private void OnDisable() => UnregisterToEvents();

	///<summary>
	///Adds local methods to listeners
	///</summary>
	private void RegisterToEvents() {
		SettingsManager.Instance.OnFigurineNotationChange += UpdateNotation;
	}

	///<summary>
	///Removes local methods from listeners
	///</summary>
	private void UnregisterToEvents() {
		if (SettingsManager.IsCreated()) SettingsManager.Instance.OnFigurineNotationChange -= UpdateNotation;
	}

	///<summary>
	///Updates the notation of all the moves in the log
	///</summary>
	///<param name="figurineNotation">Whether the notation should be figurine notation</param>
	private void UpdateNotation(bool figurineNotation) {
		List<Move> moveLog = Game.Instance.MovesPlayed;
		foreach (Move move in moveLog) move.UseFigurineNotation = figurineNotation;
		UpdateEntireLog(moveLog);
	}

	///<summary>
	///Adds the given move to the move log
	///</summary>
	///<param name="move">Move to add to the log</param>
	public void AddMove(Move move) {
		GameObject row = move.Player.IsWhite ? AddNewRow() : _rows[^1];
		row.transform.Find(move.Player.ColorPieces + " Move").GetComponent<TMP_Text>().text = move.GetLongNotation();
	}

	///<summary>
	///Updates the notation of all the moves in the move log
	///</summary>
	///<param name="moveLog">The move log</param>
	private void UpdateEntireLog(List<Move> moveLog) {
		foreach (Move move in moveLog)
			_rows[(int) Math.Ceiling(moveLog.Count / 2f) - 1].
			transform.
			Find(move.Player.ColorPieces + " Move").
			GetComponent<TMP_Text>().
			text
			= move.GetLongNotation();
	}

	///<summary>
	///Add a new row to the move log UI
	///</summary>
	public GameObject AddNewRow() {
		GameObject row = Instantiate(_rowPrefab, _moveLogBodyTransform, false);
		_rows.Add(row);
		row.name = "Row " + _rows.Count;
		row.transform.Find("Turn").GetComponent<TMP_Text>().text = _rows.Count + ")";
		return row;
	}
}
