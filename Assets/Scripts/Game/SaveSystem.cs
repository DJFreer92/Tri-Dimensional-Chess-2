using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

public sealed class SaveSystem : MonoSingleton<SaveSystem> {
	private const string _PGN_SEPERATOR = "\n\n-----\n\n";
	private const string _EXTENSION = "/game_history.save";

	public List<string> GameHistory {
		get => _gameHist ??= LoadGameHist();
		private set => _gameHist = value;
	}

	private string _gameSavePath;
	private List<string> _gameHist;

	protected override void Awake() {
		base.Awake();

		_gameSavePath = Application.persistentDataPath + _EXTENSION;
	}

	///<summary>
	///Saves the given pgn in the game history
	///</summary>
	///<param name="pgn">Game in PGN</param>
	public void SaveGame(string pgn) {
		Debug.Log("Saving Game...");

		_gameHist?.Add(pgn);

		bool arePrevGames = File.Exists(_gameSavePath);

		using var stream = new FileStream(_gameSavePath, arePrevGames ? FileMode.Append : FileMode.Create);
		new BinaryFormatter().Serialize(stream, arePrevGames ? _PGN_SEPERATOR + pgn : pgn);
	}

	///<summary>
	///Loads the game history
	///</summary>
	///<returns>The game history</returns>
	private List<string> LoadGameHist() {
		Debug.Log("Loading Game History...");
		if (!File.Exists(_gameSavePath)) {
			Debug.LogError($"Save file not found at {_gameSavePath}");
			return null;
		}

		using var stream = new FileStream(_gameSavePath, FileMode.Open);
		var games = new BinaryFormatter().Deserialize(stream) as string;
		return games.Split(_PGN_SEPERATOR, System.StringSplitOptions.RemoveEmptyEntries).ToList();
	}
}
