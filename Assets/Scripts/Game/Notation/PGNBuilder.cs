using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class PGNBuilder {
	private string _event, _site, _date, _round, _whitePlayer, _blackPlayer, _annotator, _result, _timeControl, _startTime, _termination, _mode;
	public string Setup {get; private set;}
	public List<string> Moves {get; private set;}

	public PGNBuilder(string evnt, string site, string round, string whitePlayer, string blackPlayer, string annotator, string timeControl, string setup, bool isOverTheBoard = false) {
		if (evnt == null) throw new ArgumentNullException(nameof(evnt), "Event cannot be null.");
		if (site == null) throw new ArgumentNullException(nameof(site), "Site cannot be null.");
		if (round == null) throw new ArgumentNullException(nameof(round), "Round cannot be null.");
		if (whitePlayer == null) throw new ArgumentNullException(nameof(whitePlayer), "White Player cannot be null.");
		if (blackPlayer == null) throw new ArgumentNullException(nameof(blackPlayer), "Black Player cannot be null.");
		if (annotator == null) throw new ArgumentNullException(nameof(annotator), "Annotator cannot be null.");
		if (timeControl == null) throw new ArgumentNullException(nameof(timeControl), "Time Control cannot be null.");
		_event = evnt;
		_site = site;
		_round = round;
		_whitePlayer = whitePlayer;
		_blackPlayer = blackPlayer;
		_annotator = annotator;
		_timeControl = timeControl;
		Setup = setup;
		_mode = isOverTheBoard ? "OTB" : "ICS";
		Moves = new();
		SetDateTime();
	}

	public PGNBuilder(string whitePlayer, string blackPlayer, string timeControl, string setup) {
		if (whitePlayer == null) throw new ArgumentNullException(nameof(whitePlayer), "White Player cannot be null.");
		if (blackPlayer == null) throw new ArgumentNullException(nameof(blackPlayer), "Black Player cannot be null.");
		if (timeControl == null) throw new ArgumentNullException(nameof(timeControl), "Time Control cannot be null.");
		_whitePlayer = whitePlayer;
		_blackPlayer = blackPlayer;
		_timeControl = timeControl;
		Setup = setup;
		_event = "Freeplay";
		_site = "The Internet";
		_round = "N/A";
		_mode = "ICS";
		Moves = new();
		SetDateTime();
	}

	public PGNBuilder(string whitePlayer, string blackPlayer, string setup) {
		if (whitePlayer == null) throw new ArgumentNullException(nameof(whitePlayer), "White Player cannot be null.");
		if (blackPlayer == null) throw new ArgumentNullException(nameof(blackPlayer), "Black Player cannot be null.");
		_whitePlayer = whitePlayer;
		_blackPlayer = blackPlayer;
		Setup = setup;
		_event = "Freeplay";
		_site = "The Internet";
		_round = "N/A";
		_mode = "ICS";
		Moves = new();
		SetDateTime();
	}

	private PGNBuilder() {}

	///<summary>
	///Builds the given PGN and returns the object reference
	///</summary>
	///<param name="pgn">The PGN to build</param>
	public static PGNBuilder BuildPGN(string pgn) {
		var builder = new PGNBuilder();
		string[] sections = pgn.Split('\n');
		foreach (string section in sections) {
			if (section[0] == '\n') continue;
			if (section[0] != '1') {
				int spaceIndex = section.IndexOf(' ');
				string data = section.Substring(spaceIndex + 2, section.Length - spaceIndex - 5);
				switch (section.Substring(1, spaceIndex)) {
					case "Event": builder._event = data;
					continue;
					case "Site": builder._site = data;
					continue;
					case "Date": builder._date = data;
					continue;
					case "Round": builder._round = data;
					continue;
					case "WhitePlayer": builder._whitePlayer = data;
					continue;
					case "BlackPlayer": builder._blackPlayer = data;
					continue;
					case "Annotator": builder._annotator = data;
					continue;
					case "Result": builder._result = data;
					continue;
					case "TimeControl": builder._timeControl = data;
					continue;
					case "StartTime": builder._startTime = data;
					continue;
					case "Termination": builder._termination = data;
					continue;
					case "Mode": builder._mode = data;
					continue;
					case "Setup": builder.Setup = data;
					continue;
					default: continue;
				}
			}

			while (section.Length > 0) {
				section.Remove(0, section.IndexOf(' ') + 1);
				int index = section.IndexOf(' ');
				if (index == -1) {
					builder.Moves.Add(section.Substring(0, section.Length));
					break;
				}
				builder.Moves.Add(section.Substring(0, index));
				section.Remove(0, index + 1);
			}
			break;
		}
		return builder;
	}

	///<summary>
	///Sets the date and time of the start of the game
	///</summary>
	private void SetDateTime() {
		DateTime dateTime = DateTime.Now;
		_date = dateTime.ToString("yyyy.mm.dd");
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
	///Sets the result of the game
	///</summary>
	///<param name="result">The result of the game</param>
	///<param name="termination">How the game terminated</param>
	public void SetResult(string result, string termination) {
		_result = result;
		_termination = termination;
	}

	///<summary>
	///Returns the PGN
	///</summary>
	///<returns>PGN</returns>
	public string GetPGN() {
		if (Moves.Count == 0) return null;

		var str = new StringBuilder();
		str.Append($"[Event \"{_event}\"]");
		str.Append($"[Site \"{_site}\"]");
		str.Append($"[Date \"{_date}\"]");
		str.Append($"[Round \"{_round}\"]");
		str.Append($"[White \"{_whitePlayer}\"]");
		str.Append($"[Black \"{_blackPlayer}\"]");
		if (_annotator != null) str.Append($"[Annotator \"[{_annotator}]\"]");
		str.Append($"[Result \"{_result}\"]");
		str.Append($"[PlyCount \"{Moves.Count}\"]");
		if (_timeControl != null) str.Append($"[TimeControl \"{_timeControl}\"]");
		str.Append($"[Time \"{_startTime}\"]");
		str.Append($"[Termination \"{_termination}\"]");
		str.Append($"[Mode \"{_mode}\"]\n");
		str.Append($"[SetUp \"{Setup}\"]");
		str.Append("[Variant \"Star Trek Tri-Dimensional\"]");
		str.Append("1. ").Append(Moves[0]);
		for (int i = 1; i < Moves.Count; i++) {
			str.Append(" ");
			if (i % 2 == 0) str.Append(i / 2).Append(". ");
			str.Append(Moves[i]);
		}
		return str.ToString();
	}

	///<summary>
	///Exports the PGN
	///</summary>
	///<param name="filePath">The file path to export the PGN to</param>
	public void Export(string filePath) {
		if (String.IsNullOrEmpty(filePath)) throw new ArgumentException(nameof(filePath), "Invalid file path.");

		using (var outFile = new StreamWriter(Path.Combine(filePath, "test.pgn"))) {
			outFile.Write(GetPGN());
		}
	}
}