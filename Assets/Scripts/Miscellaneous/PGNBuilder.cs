using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class PGNBuilder {
	private string _event, _site, _date, _round, _whitePlayer, _blackPlayer, _annotator, _result, _timeControl, _startTime, _termination, _mode, _setup;
	private List<string> _moves;

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
		_setup = setup;
		_mode = isOverTheBoard ? "OTB" : "ICS";
		_moves = new List<string>();
		SetDateTime();
	}

	public PGNBuilder(string whitePlayer, string blackPlayer, string timeControl, string setup) {
		if (whitePlayer == null) throw new ArgumentNullException(nameof(whitePlayer), "White Player cannot be null.");
		if (blackPlayer == null) throw new ArgumentNullException(nameof(blackPlayer), "Black Player cannot be null.");
		if (timeControl == null) throw new ArgumentNullException(nameof(timeControl), "Time Control cannot be null.");
		_whitePlayer = whitePlayer;
		_blackPlayer = blackPlayer;
		_timeControl = timeControl;
		_setup = setup;
		_event = "Freeplay";
		_site = "The Internet";
		_round = "N/A";
		_mode = "ICS";
		_moves = new List<string>();
		SetDateTime();
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
		_moves.Add(move);
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
	///Exports the PGN
	///</summary>
	///<param name="filePath">The file path to export the PGN to</param>
	public void Export(string filePath) {
		if (String.IsNullOrEmpty(filePath)) throw new ArgumentException(nameof(filePath), "Invalid file path.");
		if (_moves.Count == 0) return;

		var moves = new StringBuilder("1. ").Append(_moves[0]);
		for (int i = 1; i < _moves.Count; i++) {
			moves.Append(" ");
			if (i % 2 == 0) moves.Append(i / 2).Append(". ");
			moves.Append(_moves[i]);
		}

		using (var outFile = new StreamWriter(Path.Combine(filePath, "test.pgn"))) {
			outFile.WriteLine($"[Event \"{_event}\"]");
			outFile.WriteLine($"[Site \"{_site}\"]");
			outFile.WriteLine($"[Date \"{_date}\"]");
			outFile.WriteLine($"[Round \"{_round}\"]");
			outFile.WriteLine($"[White \"{_whitePlayer}\"]");
			outFile.WriteLine($"[Black \"{_blackPlayer}\"]");
			if (_annotator != null) outFile.WriteLine($"[Annotator \"[{_annotator}]\"]");
			outFile.WriteLine($"[Result \"{_result}\"]");
			outFile.WriteLine($"[PlyCount \"{_moves.Count}\"]");
			outFile.WriteLine($"[TimeControl \"{_timeControl}\"]");
			outFile.WriteLine($"[Time \"{_startTime}\"]");
			outFile.WriteLine($"[Termination \"{_termination}\"]");
			outFile.WriteLine($"[Mode \"{_mode}\"]");
			outFile.WriteLine("");
			outFile.WriteLine(moves);
			outFile.WriteLine($"[SetUp \"{_setup}\"]");
			outFile.WriteLine("[Variant \"Star Trek Tri-Dimensional\"]");
		}
	}
}