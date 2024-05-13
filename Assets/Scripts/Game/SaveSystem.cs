using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using TriDimensionalChess.Game.Notation;
using TriDimensionalChess.Tools;

namespace TriDimensionalChess.Game {
	public sealed class SaveSystem : MonoSingleton<SaveSystem> {
		private const string _PGN_SEPERATOR = "\n\n-----\n\n";
		private const string _EXTENSION = "/game_history.save";

		public List<PGN> GameHistory {
			get {
				var hist = new List<PGN>();
				foreach (PGN pgn in _gameHist) hist.Add(new(pgn));
				return hist;
			}
			private set => _gameHist = value;
		}

		private string _gameSavePath;
		private List<PGN> _gameHist;

		protected override void Awake() {
			base.Awake();

			_gameSavePath = Application.persistentDataPath + _EXTENSION;
			LoadGameHist();

			if (File.Exists(_gameSavePath)) Debug.Log($"Game History:\n{string.Join("\n\n", _gameHist)}");
			else Debug.Log("No game history");

		}

		///<summary>
		///Saves the given pgn in the game history
		///</summary>
		///<param name="pgn">Game in PGN</param>
		public void SaveGame(PGN pgn) {
			if (!pgn.IsValid()) {
				Debug.Log("Could not save game");
				return;
			}

			Debug.Log("Saving Game...");

			_gameHist?.Add(pgn);

			bool arePrevGames = File.Exists(_gameSavePath);

			using var stream = new FileStream(_gameSavePath, arePrevGames ? FileMode.Append : FileMode.Create);
			new BinaryFormatter().Serialize(stream, (arePrevGames ? _PGN_SEPERATOR : "") + pgn.GetPGN());
		}

		///<summary>
		///Loads the game history
		///</summary>
		///<returns>The game history</returns>
		private void LoadGameHist() {
			Debug.Log("Loading Game History...");
			if (!File.Exists(_gameSavePath)) {
				Debug.LogError($"Save file not found at {_gameSavePath}");
				return;
			}

			using var stream = new FileStream(_gameSavePath, FileMode.Open);
			var games = new BinaryFormatter().Deserialize(stream) as string;
			string[] strPGNs = games.Split(_PGN_SEPERATOR, System.StringSplitOptions.RemoveEmptyEntries);
			_gameHist = new();
			foreach (string pgn in strPGNs) _gameHist.Add(new(pgn));
		}
	}
}
