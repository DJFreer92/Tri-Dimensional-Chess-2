using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;
using System.IO;
using SFB;
using System.Linq;

using TriDimensionalChess.Game.Moves;
using TriDimensionalChess.UI;

namespace TriDimensionalChess.Game.Notation {
	public sealed class PGN {
		private const int _NUM_REQUIRED_TAGS = 7;
		private static readonly string[] _TAG_ORDER = {"Event", "Site", "Date", "Round", "White", "Black", "Result", "Annotator", "PlyCount", "TimeControl", "Time", "Termination", "Mode", "FEN", "Variant", "Options"};

		public FEN SetUp {get; private set;}
		private readonly Dictionary<string, string> _tags;
		private readonly List<string> _moves;
		private List<string> _options;
		private string _pgn;
		private bool _hasChanged, _readOnly;

		public PGN(Game game) {
			_tags = new();
			_moves = new();
			_options = new();
			_pgn = null;
			_hasChanged = true;
			_readOnly = false;

			SetUp = game.Setup;
			SetDateTime();
			AddTag("White", "Player 1");
			AddTag("Black", "Player 2");
			AddTag("Event", "Freeplay");
			AddTag("Site", "The Internet");
			AddTag("Mode", "ICS");
			AddTag("Round", "1");
			AddTag("Result", "*");
			AddTag("Variant", "Star Trek Tri-Dimensional");
		}

		public PGN(string pgn = null, bool readOnly = false) {
			SetUp = new();
			_tags = new();
			_moves = new();
			_options = new();
			_pgn = null;
			_hasChanged = true;
			_readOnly = readOnly;

			if (!string.IsNullOrEmpty(pgn)) ReadPGN(pgn);
		}

		public PGN(PGN pgn, bool readOnly = false) {
			SetUp = pgn.SetUp;
			_tags = new(pgn._tags);
			_moves = new(pgn._moves);
			_options = new(pgn._options);
			_pgn = pgn._hasChanged ? null : pgn._pgn;
			_hasChanged = pgn._hasChanged;
			_readOnly = readOnly;
		}

		public static bool operator ==(PGN a, PGN b) => a != null && b != null && a.GetPGN() == b.GetPGN();

		public static bool operator !=(PGN a, PGN b) => a == null || b == null || a.GetPGN() != b.GetPGN();

		///<summary>
		///Returns the formatted PGN
		///</summary>
		///<returns>Formatted PGN</returns>
		public string GetPGN() {
			if (!_hasChanged) return _pgn;

			if (!IsValid()) return null;

			var pgn = new StringBuilder();

			foreach (string tag in _TAG_ORDER) {
				if (tag == "FEN") {
					pgn.Append(FormatTag(tag, SetUp.Fen)).Append('\n');
					continue;
				}

				if (tag == "Options" && _options.Count > 0) {
					pgn.Append(FormatTag("Options", string.Join('/', _options)));
					continue;
				}

				if (ContainsTag(tag)) pgn.Append(FormatTag(tag, _tags[tag])).Append('\n');
			}

			foreach (var tag in _tags) {
				if (_TAG_ORDER.Contains(tag.Key)) continue;

				pgn.Append(FormatTag(tag.Key, tag.Value)).Append('\n');
			}

			pgn.Append('\n');
			for (int i = 0; i < _moves.Count; i++) {
				if (i % 2 == 0) pgn.Append(i / 2 + 1).Append(". ");
				pgn.Append(_moves[i]).Append(' ');
			}

			if (!IsContinuing()) pgn.Append(_tags["Result"]);
			else pgn.Remove(pgn.Length - 1, 1);

			_pgn = pgn.ToString();
			_hasChanged = false;
			return _pgn;
		}

		///<summary>
		///Returns whether the PGN is valid
		///</summary>
		///<returns>Whether the PGN is valid</returns>
		public bool IsValid() => _moves.Count > 0 && HasRequiredTags();

		///<summary>
		///Adds a tag with the given name and data
		///</summary>
		///<param name="name">The name of the tag</param>
		///<param name="data">The data in the tag</param>
		public void AddTag(string name, string data) {
			if (_readOnly) {
				Debug.LogWarning("Cannot edit read-only PGN");
				return;
			}

			if (name == "FEN" || name == "Options") {
				Debug.LogWarning($"{name} tag cannot be added in this manner");
				return;
			}

			if (ContainsTag(name)) {
				if (_tags[name] == data) return;

				_tags[name] = data;
				_hasChanged = true;
				return;
			}

			_tags.Add(name, data);
			_hasChanged = true;
		}

		///<summary>
		///Removes the tag of the given name
		///</summary>
		///<param name="name">The name of the tag to be removed</param>
		public void RemoveTag(string name) {
			if (_readOnly) {
				Debug.LogWarning("Cannot edit read-only PGN");
				return;
			}

			if (name == "Result") {
				if (IsContinuing()) return;
				_tags["Result"] = "*";
				_hasChanged = true;
			}

			if (!ContainsTag(name)) return;

			_tags.Remove(name);
			_hasChanged = true;
		}

		///<summary>
		///Add the given option to the PGN
		///</summary>
		///<param name="option">The option to be added</param>
		public void AddOption(string option) {
			if (!_options.Contains(option)) _options.Add(option);
		}

		///<summary>
		///Removes the given option from the PGN
		///</summary>
		///<param name="option">The option to be removed</param>
		public void RemoveOption(string option) {
			if (_options.Contains(option)) _options.Remove(option);
		}

		///<summary>
		///Returns whether the PGN contains the given option
		///</summary>
		///<param name="option">The option to search for</param>
		///<returns>Whether the PGN contains the given option</returns>
		public bool ContainsOption(string option) => _options.Contains(option);

		///<summary>
		///Adds the given move
		///</summary>
		///<param name="move">The move to be added</param>
		public void AddMove(Move move) {
			if (_readOnly) {
				Debug.LogWarning("Cannot edit read-only PGN");
				return;
			}

			//_moves.Add(move.GetLongNotation());
			_moves.Add(move.GetShortNotation());
			_hasChanged = true;
		}

		///<summary>
		///Adds the given move
		///</summary>
		///<param name="move">The move to be added</param>
		public void AddMove(string move) {
			if (_readOnly) {
				Debug.LogWarning("Cannot edit read-only PGN");
				return;
			}

			_moves.Add(move);
			_hasChanged = true;
		}

		///<summary>
		///Returns the moves in the PGN
		///</summary>
		///<returns>The moves in the PGN</returns>
		public List<string> GetMoves() => new(_moves);

		///<summary>
		///Removes the last move
		///</summary>
		public void RemoveLastMove() {
			if (_readOnly) {
				Debug.LogWarning("Cannot edit read-only PGN");
				return;
			}

			if (_moves.Count == 0) return;

			_moves.RemoveAt(_moves.Count - 1);
			_hasChanged = true;
		}

		///<summary>
		///Sets the PGN as read only so that it cannot be edited
		///</summary>
		public void SetReadOnly() => _readOnly = true;

		///<summary>
		///Constructs the PGN from the given string PGN
		///</summary>
		///<param name="pgn">The string PGN</param>
		private void ReadPGN(string pgn) {
			string[] sections = pgn.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

			string name, data;
			foreach (string section in sections) {
				if (section[0] != '[') break;

				int space = section.IndexOf(' ');
				name = section[1..space];
				data = section[(space + 2)..(section.Length - 2)];

				if (name == "FEN") {
					SetUp = new(data);
					continue;
				}

				if (name == "Options") {
					_options = data.Split('/').ToList();
					continue;
				}

				AddTag(name, data);
			}

			string[] moves = sections[^1].Split(' ');
			for (int i = 0; i < moves.Length; i++) {
				if (i % 3 == 0) continue;
				if (i == moves.Length - 1 && !IsContinuing()) break;
				AddMove(moves[i]);
			}
		}

		///<summary>
		///Sets the date and time of the start of the game
		///</summary>
		private void SetDateTime() {
			if (_readOnly) {
				Debug.LogWarning("Cannot edit read-only PGN");
				return;
			}

			DateTime dateTime = DateTime.Now;
			AddTag("Date", dateTime.ToString("yyyy.MM.dd"));
			AddTag("Time", dateTime.ToString("hh:mm:ss"));
		}

		///<summary>
		///Returns whether the PGN contains the given tag
		///</summary>
		///<param name="name">The name of the tag to be searched for</param>
		///<returns>Whether the PGN contains the given tag</returns>
		private bool ContainsTag(string name) => _tags.ContainsKey(name);

		///<summary>
		///Returns whether the PGN contains all the required tags
		///</summary>
		///<returns>Whether the PGN contains all the required tags</returns>
		private bool HasRequiredTags() {
			for (int i = 0; i < _NUM_REQUIRED_TAGS; i++)
				if (!ContainsTag(_TAG_ORDER[i])) return false;
			return true;
		}

		///<summary>
		///Returns whether the game is still continuing
		///</summary>
		///<returns>Whether the game is still continuing</returns>
		private bool IsContinuing() => _tags["Result"] == "*";

		///<summary>
		///Returns a formatted string containing the given tag name and data
		///</summary>
		///<param name="name">The tag name</param>
		///<param name="data">The tag data</param>
		///<returns>A formatted string containing the given tag name and data</returns>
		private string FormatTag(string name, string data) => $"[{name} \"{data}\"]";

		///<summary>
		///Sets the result and termination of the game
		///</summary>
		public PGN ClosePGN(GameState endState) {
			if (_readOnly) {
				Debug.LogWarning("Cannot edit read-only PGN");
				return this;
			}

			if (endState.Is(GameState.ACTIVE)) {
				SetReadOnly();
				return this;
			}

			string result;
			if (endState.Is(GameState.WHITE_WIN)) result = "1-0";
			else if (endState.Is(GameState.BLACK_WIN)) result = "0-1";
			else result = "1/2-1/2";
			AddTag("Result", result);

			string termination = endState switch {
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

			AddTag("Termination", termination);

			SetReadOnly();

			return this;
		}

		///<summary>
		///Exports the PGN
		///</summary>
		public void Export() {
			string path = StandaloneFileBrowser.SaveFilePanel("Select Save Location", "", $"Tri-D_{DateTime.Now:yyyy-MM-dd_hhmmss}", "pgn");

			if (string.IsNullOrEmpty(path)) {
				MessageManager.Instance.CreateMessage("Export Aborted");
				return;
			}

			using var outFile = new StreamWriter(path);
			outFile.Write(GetPGN());

			MessageManager.Instance.CreateMessage("Exported PGN");
		}

		public override bool Equals(object o) => base.Equals(o);

		public override int GetHashCode() => base.GetHashCode();

		public override string ToString() => GetPGN();
	}
}
