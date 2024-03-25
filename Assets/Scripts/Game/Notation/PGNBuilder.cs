using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;

public class PGNBuilder {
	private string _event, _site, _date, _round, _whitePlayer, _blackPlayer, _annotator, _result, _timeControl, _startTime, _termination, _mode;
	public string Setup {get; private set;}
	public List<string> Moves {get; private set;}

	public PGNBuilder(string evnt, string site, string round, string whitePlayer, string blackPlayer, string annotator, string timeControl, string setup, bool isOverTheBoard = false) {
        _event = evnt ?? throw new ArgumentNullException(nameof(evnt), "Event cannot be null.");
		_site = site ?? throw new ArgumentNullException(nameof(site), "Site cannot be null.");
		_round = round ?? throw new ArgumentNullException(nameof(round), "Round cannot be null.");
		_whitePlayer = whitePlayer ?? throw new ArgumentNullException(nameof(whitePlayer), "White Player cannot be null.");
		_blackPlayer = blackPlayer ?? throw new ArgumentNullException(nameof(blackPlayer), "Black Player cannot be null.");
		_annotator = annotator ?? throw new ArgumentNullException(nameof(annotator), "Annotator cannot be null.");
		_timeControl = timeControl ?? throw new ArgumentNullException(nameof(timeControl), "Time Control cannot be null.");
		Setup = setup;
		_mode = isOverTheBoard ? "OTB" : "ICS";
		Moves = new();
		SetDateTime();
		_result = "*";
	}

	public PGNBuilder(string whitePlayer, string blackPlayer, string timeControl, string setup) {
        _whitePlayer = whitePlayer ?? throw new ArgumentNullException(nameof(whitePlayer), "White Player cannot be null.");
		_blackPlayer = blackPlayer ?? throw new ArgumentNullException(nameof(blackPlayer), "Black Player cannot be null.");
		_timeControl = timeControl ?? throw new ArgumentNullException(nameof(timeControl), "Time Control cannot be null.");
		Setup = setup;
		_event = "Freeplay";
		_site = "The Internet";
		_mode = "ICS";
		Moves = new();
		SetDateTime();
		_result = "*";
	}

	public PGNBuilder(string whitePlayer, string blackPlayer, string setup) {
        _whitePlayer = whitePlayer ?? throw new ArgumentNullException(nameof(whitePlayer), "White Player cannot be null.");
		_blackPlayer = blackPlayer ?? throw new ArgumentNullException(nameof(blackPlayer), "Black Player cannot be null.");
		Setup = setup;
		_event = "Freeplay";
		_site = "The Internet";
		_mode = "ICS";
		Moves = new();
		SetDateTime();
		_result = "*";
	}

	private PGNBuilder() {}

	///<summary>
	///Builds the given PGN and returns the object reference
	///</summary>
	///<param name="pgn">The PGN to build</param>
	public static PGNBuilder BuildPGN(string pgn) {
        var builder = new PGNBuilder { Moves = new() };
        string[] sections = pgn.Split('\n');
		foreach (string section in sections) {
			if (string.IsNullOrEmpty(section)) continue;
			if (section[0] == '1') break;

			int spaceIndex = section.IndexOf(' ');
			string data = section.Substring(spaceIndex + 2, section.Length - spaceIndex - 4);
			switch (section[1..spaceIndex]) {
				case "Event": builder._event = data;
				continue;
				case "Site": builder._site = data;
				continue;
				case "Date": builder._date = data;
				continue;
				case "Round": builder._round = data;
				continue;
				case "White": builder._whitePlayer = data;
				continue;
				case "Black": builder._blackPlayer = data;
				continue;
				case "Annotator": builder._annotator = data;
				continue;
				case "Result": builder._result = data;
				continue;
				case "TimeControl": builder._timeControl = data;
				continue;
				case "Time": builder._startTime = data;
				continue;
				case "Termination": builder._termination = data;
				continue;
				case "Mode": builder._mode = data;
				continue;
				case "SetUp": builder.Setup = data;
				continue;
				default: continue;
			}
		}

		string moves = sections[^1];
		while (moves.Length > 0) {
			if (char.IsDigit(moves[0])) moves = moves.Remove(0, moves.IndexOf(' ') + 1);
			int index = moves.IndexOf(' ');
			if (index == -1) {
				if (moves.Contains("e.p.")) builder.Moves[^1] += moves;
				else builder.Moves.Add(moves);
				break;
			}
			if (moves[..index].Contains("e.p.")) builder.Moves[^1] += " " + moves[..index];
			else builder.Moves.Add(moves[..index]);
			moves = moves.Remove(0, index + 1);
		}

		return builder;
	}

	///<summary>
	///Psuedo verifies the given PGN
	///</summary>
	///<param name="pgn">The PGN to be verified</param>
	///<returns>Whether the PGN is psuedo valid</returns>
	public static bool VerifyPGN(string pgn) {
		return true;
	}

	///<summary>
	///Sets the date and time of the start of the game
	///</summary>
	private void SetDateTime() {
		DateTime dateTime = DateTime.Now;
		_date = dateTime.ToString("yyyy.MM.dd");
		_startTime = dateTime.ToString("hh:mm:ss");
	}

	///<summary>
	///Records a move
	///</summary>
	///<param name="move">The move to record</param>
	public void AddMove(string move) {
		Moves.Add(move);
	}

	///<summary>
	///Sets the result and termination of the game
	///</summary>
	public void EndGame() {
		if (string.IsNullOrEmpty(_result)) {
			if (Game.Instance.State.Is(GameState.WHITE_WIN)) _result = "1-0";
			else if (Game.Instance.State.Is(GameState.BLACK_WIN)) _result = "0-1";
			else _result = "1/2-1/2";
		}

		if (!string.IsNullOrEmpty(_termination)) return;

		_termination = Game.Instance.State switch {
			GameState.WHITE_WIN_NORMAL => "Normal",
			GameState.BLACK_WIN_NORMAL => "Normal",
			GameState.WHITE_RESIGNATION => "Resignation",
			GameState.BLACK_RESIGNATION => "Resignation",
			GameState.DRAW_MUTUAL_AGREEMENT => "Mutual Agreement",
			GameState.DRAW_STALEMATE => "Stalemate",
			GameState.DRAW_THREEFOLD_REPETITION => "Threefold Repetition",
			GameState.DRAW_FIFTY_MOVE_RULE => "Fifty Move Rule",
			GameState.DRAW_DEAD_POSITION => "Dead Position",
			GameState.DRAW_TIMEOUT_VS_INSUFFICIENT_MATERIAL => "Timeout",
			_ => "Unknown"
		};
	}

	///<summary>
	///Returns the PGN
	///</summary>
	///<returns>PGN</returns>
	public string GetPGN() {
		if (Moves.Count == 0) return null;

		var str = new StringBuilder();
		str.Append($"[Event \"{_event}\"]\n");
		str.Append($"[Site \"{_site}\"]\n");
		str.Append($"[Date \"{_date}\"]\n");
		if (_round != null) str.Append($"[Round \"{_round}\"]\n");
		str.Append($"[White \"{_whitePlayer}\"]\n");
		str.Append($"[Black \"{_blackPlayer}\"]\n");
		if (_annotator != null) str.Append($"[Annotator \"[{_annotator}]\"]\n");
		if (_result != null) str.Append($"[Result \"{_result}\"]\n");
		str.Append($"[PlyCount \"{Moves.Count}\"]\n");
		if (_timeControl != null) str.Append($"[TimeControl \"{_timeControl}\"]\n");
		str.Append($"[Time \"{_startTime}\"]\n");
		if (_termination != null) str.Append($"[Termination \"{_termination}\"]\n");
		str.Append($"[Mode \"{_mode}\"]\n");
		str.Append($"[SetUp \"{Setup}\"]\n");
		str.Append("[Variant \"Star Trek Tri-Dimensional\"]\n\n");
		str.Append("1. ").Append(Moves[0]);
		for (int i = 1; i < Moves.Count; i++) {
			str.Append(" ");
			if (i % 2 == 0) str.Append(i / 2 + 1).Append(". ");
			str.Append(Moves[i]);
		}
		return str.ToString();
	}

	///<summary>
	///Exports the PGN
	///</summary>
	///<param name="filePath">The file path to export the PGN to</param>
	public void Export() {
		string path = EditorUtility.OpenFolderPanel("Select Save Location", "", "");

		if (string.IsNullOrEmpty(path)) {
			MessageManager.Instance.CreateMessage("Export Aborted");
			return;
		}

        using var outFile = new StreamWriter(Path.Combine(path, $"Tri-D {DateTime.Now.ToString("yyyy-MM-dd_hhmmss")}.pgn"));
        outFile.Write(GetPGN());

		MessageManager.Instance.CreateMessage("Exported PGN");
    }
}
