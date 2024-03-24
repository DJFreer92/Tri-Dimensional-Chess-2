using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Networking.Transport;
using TMPro;

[RequireComponent(typeof(SettingsManager))]
[RequireComponent(typeof(CapturedPiecesController))]
[RequireComponent(typeof(PromotionController))]
[RequireComponent(typeof(TimerManager))]
[RequireComponent(typeof(Server))]
[RequireComponent(typeof(Client))]
[DisallowMultipleComponent]
public sealed class Game : MonoSingleton<Game> {
	//port that the server and client can connect to
	private const int _CONNECTION_PORT = 8005;
	//starting positions
	public const string ORIGINAL_FEN = "b6B6w1W1 nbbn/dddd/4/4|4/4/4/4|4/4/DDDD/NBBN|rq/dd|kr/dd|DD/RQ|DD/KR w KQkq - 0 1";
	public const string NEXT_GEN_FEN = "b6B6n4N3w1W1 nbbn/dddd/4/4|4/4/4/4|4/4/DDDD/NBBN|rq/dd|kr/dd|2/2|2/2|DD/RQ|DD/KR w KQkq - 0 1";
	//listeners
	public Action OnGameStateChange, OnCurrentPlayerChange;
	public Action<Player> OnCurrentPlayerChangeWParam;
	//game variables
	public GameState State {
		get => _state;
		set {
			if (_state == value) return;
			_state = value;
			Client.Instance.SendToServer(new NetGameState() {
				State = _state
			});
			OnGameStateChange?.Invoke();
		}
	}
	public Player CurPlayer {
		get => _curPlayer;
		private set {
			if (_curPlayer == value) return;
			_curPlayer = value;
			OnCurrentPlayerChange?.Invoke();
			OnCurrentPlayerChangeWParam?.Invoke(value);
		}
	}
	public List<Move> MovesPlayed {get; private set;}

	#region Prefab Variables
	[Header("Prefabs")]
	[SerializeField] private GameObject _whiteKingPrefab;
	[SerializeField] private GameObject _whiteQueenPrefab;
	[SerializeField] private GameObject _whiteRookPrefab;
	[SerializeField] private GameObject _whiteBishopPrefab;
	[SerializeField] private GameObject _whiteKnightPrefab;
	[SerializeField] private GameObject _whitePawnPrefab;
	[SerializeField] private GameObject _blackKingPrefab;
	[SerializeField] private GameObject _blackQueenPrefab;
	[SerializeField] private GameObject _blackRookPrefab;
	[SerializeField] private GameObject _blackBishopPrefab;
	[SerializeField] private GameObject _blackKnightPrefab;
	[SerializeField] private GameObject _blackPawnPrefab;
	#endregion

	#region Reference Variables
	[Header("References")]
	[SerializeField] private PlayerControls _whiteCtrls;
	[SerializeField] private PlayerControls _blackCtrls;
	[SerializeField] private TMP_Text _gameStateTxt;
	#endregion

	[SerializeField] private Page _gameUIPage;
	[SerializeField] private TMP_InputField _ipAddressInput;
	[HideInInspector] public string Setup;
	[HideInInspector] public string StartPGN;
	[HideInInspector] public bool AllowMoves;
	[HideInInspector] public bool AllowButtons;
	public CommandHandler MoveCommandHandler {get; private set;}
	public int MoveCount {get; private set;}
	private GameState _state;
	private Player _curPlayer;
	private Player[] _players = new Player[2];
	private PGNBuilder _pgnBuilder;
	private Square _selectedSquare;
	private List<Square> _availableMoves, _availableABMoves;
	private List<string> _prevPositions;
	private List<int> _prevAvailableMovesCount;
	private int _moveRuleCount;

	//used for multiplayer
	private int _playerCount = 0;  //server only
	private Player _myPlayer;  //client only
	private bool _isLocalGame;  //client only

	protected override void Awake() {
		base.Awake();

		//set the game state
		_state = GameState.PRE_GAME;

		//assign prefabs
		King.SetPrefabs(_whiteKingPrefab, _blackKingPrefab);
		Queen.SetPrefabs(_whiteQueenPrefab, _blackQueenPrefab);
		Rook.SetPrefabs(_whiteRookPrefab, _blackRookPrefab);
		Bishop.SetPrefabs(_whiteBishopPrefab, _blackBishopPrefab);
		Knight.SetPrefabs(_whiteKnightPrefab, _blackKnightPrefab);
		Pawn.SetPrefabs(_whitePawnPrefab, _blackPawnPrefab);
	}

	private void Update() {
		//if it is the pre-game, exit
		if (State == GameState.PRE_GAME) return;

		//if it is neither white nor black's turn
		if (State.Is(GameState.INACTIVE)) {
			//stop the timers
			if (!TimerManager.Instance.TimersEnabled) TimerManager.Instance.StopTimers(true);
			return;
		}

		//if the timers are not enabled, exit
		if (!TimerManager.Instance.TimersEnabled) return;

		//get the remaining times on the timers
		float[] times = TimerManager.Instance.GetTimes();

		//if neither timer has flagged, exit
		if (times[0] != 0 && times[1] != 0) return;

		//if the opponent of the player who's timer has run out has material to win on time
		if (ChessBoard.Instance.HasMaterialToWinOnTime(GetPlayer(times[0] != 0))) {
			//update game state to player timeout
			State = (times[0] == 0) ? GameState.WHITE_TIMEOUT_LOSE : GameState.BLACK_TIMEOUT_LOSE;
			//exit
			return;
		}

		//update game state to draw by timeout vs insufficient material
		State = GameState.DRAW_TIMEOUT_VS_INSUFFICIENT_MATERIAL;
	}

	private void OnEnable() {
		//register to events
		RegisterToEvents();
	}

	private void OnDisable() {
		//unregister to events
		UnregisterToEvents();
	}

	///<summary>
	///Adds local methods to listeners
	///</summary>
	private void RegisterToEvents() {
		OnGameStateChange += UpdateGameStateText;
		OnCurrentPlayerChange += UpdateGameStateText;

		//server
		NetUtility.S_WELCOME += OnWelcomeServer;
		NetUtility.S_GAME_STATE += OnGameStateServer;
		NetUtility.S_MAKE_MOVE += OnMakeMoveServer;

		//client
		NetUtility.C_WELCOME += OnWelcomeClient;
		NetUtility.C_START_GAME += OnStartGameClient;
		NetUtility.C_GAME_STATE += OnGameStateClient;
		NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
	}

	///<summary>
	///Removes local methods from listeners
	///</summary>
	private void UnregisterToEvents() {
		OnGameStateChange -= UpdateGameStateText;
		OnCurrentPlayerChange -= UpdateGameStateText;

		//server
		NetUtility.S_WELCOME -= OnWelcomeServer;
		NetUtility.S_GAME_STATE -= OnGameStateServer;
		NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;

		//client
		NetUtility.C_WELCOME -= OnWelcomeClient;
		NetUtility.C_START_GAME -= OnStartGameClient;
		NetUtility.C_GAME_STATE -= OnGameStateClient;
		NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
	}

	///<summary>
	///Initializes the game
	///</summary>
	public void Init() {
		//set the status of the game to active
		_state = GameState.WHITE_TURN;

		//create a command handler for moves
		MoveCommandHandler = new();

		//set the player of the white pieces to the current turn
		CurPlayer = GetPlayer(true);

		//clear the moves that have been played
		MovesPlayed = new();

		//create lists of the available moves
		_availableMoves = new();
		_availableABMoves = new();

		//clear the selected square
		_selectedSquare = null;

		//create lists for the previous positions and previous avilable moves count
		_prevPositions = new();
		_prevAvailableMovesCount = new();

		//initialize move count
		MoveCount = 0;

		//initialize move rule count
		_moveRuleCount = 0;

		//initialize the PGN builder
		if (!string.IsNullOrEmpty(StartPGN)) LoadPGN(StartPGN);
		else {
			//load the position from the fen
			LoadFEN(Setup);
			_pgnBuilder = new PGNBuilder("Player 1", "Player 2", Setup);
		}

		//clear the captured pieces
		CapturedPiecesController.Instance.ClearCapturedPieces();

		//allow moves to be made
		AllowMoves = true;
		AllowButtons = true;

		Debug.Log(FENBuilder.GetFEN(ChessBoard.Instance, CurPlayer.IsWhite, _moveRuleCount, MoveCount));
		Debug.Log(_pgnBuilder.GetPGN());
	}

	///<summary>
	///Starts a local game
	///</summary>
	public void StartLocalGame() {
		_isLocalGame = true;
		Server.Instance.Init(_CONNECTION_PORT);
		Client.Instance.Init("127.0.0.1", _CONNECTION_PORT);
	}

	///<summary>
	///Loads the given PGN
	///</summary>
	///<param name="pgn">The PGN to load</param>
	public void LoadPGN(string pgn) {
		PGNBuilder builder = PGNBuilder.BuildPGN(pgn);
		Setup = builder.Setup;
		LoadFEN(Setup);
		foreach (string annotatedMove in builder.Moves) {
			Move move = Move.BuildMove(annotatedMove, CurPlayer.IsWhite);
			move.Execute();
			if (!move.MoveEvents.Contains(MoveEvent.PROMOTION)) FinishTurn(move);
		}
		_pgnBuilder = builder;
	}

	///<summary>
	///Load the given FEN
	///</summary>
	///<param name="fen">The FEN to load</param>
	public void LoadFEN(string fen) {
		string[] sections = fen.Split(' ');
		ChessBoard.Instance.ConstructPosition(fen);
		if (sections[2] == "w" != CurPlayer.IsWhite) SwitchCurrentPlayer();
		_moveRuleCount = int.Parse(sections[5]);
		MoveCount = int.Parse(sections[6]);
	}

	///<summary>
	///Gets the player of the given pieces
	///</summary>
	///<param name="isWhite">Whether the desired player is or isn't the white player</param>
	///<returns>The player of the given pieces</returns>
	public Player GetPlayer(bool isWhite) {
		return (isWhite == _players[0].IsWhite) ? _players[0] : _players[1];
	}

	///<summary>
	///Selects the given piece
	///</summary>
	///<param name="piece">The piece to be selected</param>
	public void SelectPiece(ChessPiece piece) {
		//select the square with the given piece on it
		SelectSquare(piece.GetSquare());
	}

	///<summary>
	///Selects the given square
	///</summary>
	///<param name="square">The square to be selected</param>
	public void SelectSquare(Square square) {
		//if moves are currently not allowed to be made
		if (!AllowMoves) return;

		//if it is not this player's turn, exit
		if (_myPlayer != CurPlayer) return;

		//if the square is the same as the one already selected, exit
		if (_selectedSquare == square) return;

		//if there is no selected square
		if (_selectedSquare == null) {
			if (square.HasPiece()) {  //if the square has a piece
				//if the piece on the square does not belong to the current player do not select it
				if (square.GamePiece.IsWhite != CurPlayer.IsWhite) return;

				//Find all the available moves for the piece and highlight them
				FindAndHighlightAvailablePieceMoves(square.GamePiece);
			} else if (square.Coords.y % 2 == 1) {  //if the square is part of an attackboard
				//Find all the available moves for the attackboard and highlight them
				FindAndHighlightAvailableABMoves(square);

				//if there are no available moves, exit
				if (_availableABMoves.Count == 0) return;
			} else {  //the square is empty and not an attackboard
				return;
			}

			//select the square
			_selectedSquare = square;
			Debug.Log($"{square.GetAnnotation()} selected");
			return;
		}

		Debug.Log($"{square.GetAnnotation()} selected");

		bool validMove = false, isABMove = false;
		//find if the square is part of an available standard move
		foreach (Square sqr in _availableMoves) {
			if (!square.Coords.Equals(sqr.Coords)) continue;
			validMove = true;
			break;
		}

		//find if the square is part of an available attackboard move
		foreach (Square sqr in _availableABMoves) {
			if (!square.Coords.Equals(sqr.Coords)) continue;
			validMove = true;
			isABMove = true;
			break;
		}

		//unhighlight the gamepiece on the start square
		_selectedSquare.GamePiece?.ToggleHighlight(false);
		//unhightlight the available moves
		foreach (Square sqr in _availableMoves) sqr.ToggleHighlight(false);
		//unhightlight the available attackboard moves
		foreach (Square sqr in _availableABMoves) sqr.ToggleHighlight(false);

		//if a move is valid, send the move to the server and then execute the move
		if (validMove) {
			//if it is not a local game, send the move to the server
			if (!_isLocalGame) {
				Client.Instance.SendToServer(new NetMakeMove() {
					IsABMove = isABMove,
					StartCoordinates = _selectedSquare.Coords,
					EndCoordinates = square.Coords,
					IsWhiteMove = CurPlayer.IsWhite
				});
			}

			//execute the move
			MakeMove(_selectedSquare, square, isABMove);

			//deselect square
			_selectedSquare = null;
			return;
		}

		if (square.GamePiece?.IsWhite == CurPlayer.IsWhite) {
			//Find all the available piece moves and highlight them
			FindAndHighlightAvailablePieceMoves(square.GamePiece);

			//select the square
			_selectedSquare = square;
			return;
		} else if (square.Coords.y % 2 == 1) {
			FindAndHighlightAvailableABMoves(square);

			//select the square
			_selectedSquare = square;
			return;
		}

		//deselect the square
		_selectedSquare = null;

		//inner functions
		//finds all the available moves for the given piece and highlights their squares
		void FindAndHighlightAvailablePieceMoves(ChessPiece piece) {
			//get the moves from the selected square available to the player
			_availableMoves = piece.GetAvailableMoves(CurPlayer.IsWhite);
			_availableABMoves.Clear();
			//highlight the game piece
			piece.ToggleHighlight(true);
			//highlight all the available moves
			foreach (Square sqr in _availableMoves) sqr.ToggleHighlight(true);
		}

		//finds all the available moves for the given attackboard and highlights their squares
		void FindAndHighlightAvailableABMoves(Square square) {
			//get all the attackboard moves available from the selected attackboard
			_availableABMoves = (ChessBoard.Instance.GetBoardWithSquare(square) as AttackBoard).GetAvailableMoves(CurPlayer.IsWhite);
			_availableMoves.Clear();

			//highlight all the available attackboard moves
			foreach (Square sqr in _availableABMoves) sqr.ToggleHighlight(true);
		}
	}

	///<summary>
	///Moves a piece or attackboard from one square to another square
	///</summary>
	///<param name="startSqr">The square a piece is on or of an attackboard</param>
	///<param name="endSpr">The square to move a piece or attackboard to</param>
	private void MakeMove(Square startSqr, Square endSqr, bool isABMove) {
		Move move = isABMove ? new AttackBoardMove(CurPlayer, startSqr, endSqr) : new PieceMove(CurPlayer, startSqr, endSqr);
		try {
			MoveCommandHandler.RedoAllCommands();
			MoveCommandHandler.AddCommand(move);
		} catch (Exception ex) {
			Debug.Log(ex.Message);
			return;
		}
		FinishTurn(move);
	}

	///<summary>
	///Finish the player's turn
	///</summary>
	///<param name="move">The move to be finished</param>
	public void FinishTurn(Move move) {
		//record the move
		MovesPlayed.Add(move);

		//update the move count
		if (!CurPlayer.IsWhite) MoveCount++;

		//check if the game was won or is a draw
		UpdateGameState();

		//add the move to the pgn
		_pgnBuilder?.AddMove(move.GetAnnotation());

		//switch to the next player's turn
		if (State.Is(GameState.ACTIVE)) NextTurn();
		else {
			Debug.Log("State: " + State);
			Debug.Log(FENBuilder.GetFEN(ChessBoard.Instance, CurPlayer.IsWhite, _moveRuleCount, MoveCount));
			Debug.Log(_pgnBuilder?.GetPGN());
		}
	}

	///<summary>
	///Update the status of the game, whether a player has won or if the game is a draw
	///</summary>
	private void UpdateGameState() {
		//get the last move
		Move lastMove = MovesPlayed[^1];

		ChessBoard.Instance.UpdateKingCheckState(!CurPlayer.IsWhite);

		bool isInCheck = ChessBoard.Instance.IsKingInCheck(!CurPlayer.IsWhite);
		bool hasAMove = ChessBoard.Instance.DoesPlayerHaveMove(!CurPlayer.IsWhite);

		//if the opposing player does not have a move
		if (isInCheck && !hasAMove) {
			//set the last move as having made a checkmate
			lastMove.MoveEvents.Add(MoveEvent.CHECKMATE);

			//update the game status
			State = CurPlayer.IsWhite ? GameState.WHITE_WIN_NORMAL : GameState.BLACK_WIN_NORMAL;
			return;
		}

		if (isInCheck) lastMove.MoveEvents.Add(MoveEvent.CHECK);  //if the opposing player's king is in check, set the last move as having made a check
		else if (!hasAMove) State = GameState.DRAW_STALEMATE;  //if the opposing player's king is not in check and has no moves, draw by stalemate

		//update the number of consecutive moves
		UpdateMoveRuleCount(lastMove);

		//add the number of available moves to the previous number of available moves count
		_prevAvailableMovesCount.Add(_availableMoves.Count + _availableABMoves.Count);

		//check draw conditions
		//50 move rule
		if (_moveRuleCount >= 100) {
			State = GameState.DRAW_FIFTY_MOVE_RULE;
			return;
		}

		//dead position
		if (ChessBoard.Instance.IsDeadPosition()) {
			State = GameState.DRAW_DEAD_POSITION;
			return;
		}

		//add the last position to the previous positions
		_prevPositions.Add(FENBuilder.GetFEN(ChessBoard.Instance, CurPlayer.IsWhite, _moveRuleCount, MoveCount));

		//threefold repetition
		//if a piece was captured on the last move
		if (lastMove is not AttackBoardMove && lastMove.MoveEvents.Contains(MoveEvent.CAPTURE)) {
			//remove all positions from the previous positions
			_prevPositions = new() {_prevPositions[^1]};
			return;
		}

		if (IsThreeFoldRepitition()) State = GameState.DRAW_THREEFOLD_REPETITION;
	}

	///<summary>
	///Update the number of consecutive moves that have occured
	///</summary>
	///<param name="move">Move to consider for consecutive moves</param>
	///<returns></returns>
	private void UpdateMoveRuleCount(Move move) {
		//if it was an attackboard move and there were no pieces on the board, a piece was captured or the piece moved was a pawn
		if ((move is AttackBoardMove && (move as AttackBoardMove).BoardMoved.GetPieceCount() != 0) ||
			move.MoveEvents.Contains(MoveEvent.CAPTURE) ||
			(move is PieceMove && (move as PieceMove).PieceMoved is Pawn))
		{
			//clear the move rule count
			_moveRuleCount = 0;
			return;
		}

		//increment the move rule count
		_moveRuleCount++;
	}

	///<summary>
	///Returns whether the current position has been repeated three times
	///</summary>
	///<returns>Whether the current position has been repeated three times</returns>
	private bool IsThreeFoldRepitition() {
		if (_prevPositions.Count < 5) return false;
		string lastPosition = _prevPositions[^1].Split(' ')[0] + _prevPositions[^1].Split(' ')[1];
		int lastAvailMoveCount = _prevAvailableMovesCount[^1];
		int count = 1;
		for (var i = _prevPositions.Count - 3; i >= 0 && count < 3; i -= 2) {
			string position = _prevPositions[i].Split(' ')[0] + _prevPositions[i].Split(' ')[1];
			if (_prevAvailableMovesCount[i] == lastAvailMoveCount && position.Equals(lastPosition)) count++;
		}
		return count == 3;
	}

	///<summary>
	///Switches to the next player's turn
	///</summary>
	public void NextTurn() {
		//switch the current turn
		SwitchCurrentPlayer();

		Debug.Log(FENBuilder.GetFEN(ChessBoard.Instance, CurPlayer.IsWhite, _moveRuleCount, MoveCount));
		Debug.Log(_pgnBuilder?.GetPGN());
	}

	///<summary>
	///Updates UI elements for the next turn
	///</summary>
	private void SwitchCurrentPlayer() {
		//set the current turn to the next player
		CurPlayer = GetPlayer(!CurPlayer.IsWhite);

		//update which player can offer a draw
		_whiteCtrls.EnableDraw(CurPlayer.IsWhite);
		_blackCtrls.EnableDraw(!CurPlayer.IsWhite);

		//if it is a local game, swap which player the client is
		if (_isLocalGame) _myPlayer = CurPlayer;
	}

	///<summary>
	///Updates the game status text UI element
	///</summary>
	private void UpdateGameStateText() {
		string txt = null;
		switch (State) {
			case GameState.PRE_GAME:
			return;
			case GameState.WHITE_TURN: case GameState.BLACK_TURN: txt = CurPlayer.ColorPieces + "'s Turn";
			break;
			case GameState.WHITE_WIN_NORMAL: txt = "White Wins!";
			break;
			case GameState.BLACK_RESIGNATION: txt = "White Wins by Resignation!";
			break;
			case GameState.BLACK_TIMEOUT_LOSE: txt = "White Wins on Time!";
			break;
			case GameState.BLACK_WIN_NORMAL: txt = "Black Wins!";
			break;
			case GameState.WHITE_RESIGNATION: txt = "Black Wins by Resignation!";
			break;
			case GameState.WHITE_TIMEOUT_LOSE: txt = "Black Wins on Time";
			break;
			case GameState.DRAW_MUTUAL_AGREEMENT: txt = "Draw by Mutual Agreement";
			break;
			case GameState.DRAW_STALEMATE: txt = "Draw by Stalemate";
			break;
			case GameState.DRAW_THREEFOLD_REPETITION: txt = "Draw by Threefold Repetition";
			break;
			case GameState.DRAW_FIFTY_MOVE_RULE: txt = "Draw by 50-Move Rule";
			break;
			case GameState.DRAW_DEAD_POSITION: txt = "Draw by Dead Position";
			break;
			case GameState.DRAW_TIMEOUT_VS_INSUFFICIENT_MATERIAL: txt = "Draw by Timeout vs Insufficient Material";
			break;
		}
		_gameStateTxt.text = txt ?? "ERROR";
	}

	///<summary>
	///Returns whether it is currently the first move of either player
	///</summary>
	///<returns>Whether it is currently the first move of either player</returns>
	public bool IsFirstMove() {
		return MoveCount == 1;
	}

	///<summary>
	///Extends a draw offer to the given player
	///</summary>
	///<param name="isWhite">Whether the player is white</param>
	public void OfferDraw(bool toWhite) {
		(toWhite ? _whiteCtrls : _blackCtrls).HasDrawOffer = true;
	}

	//Buttons
	///<summary>
	///Initializes the server and both clients
	///</summary>
	public void OnLocalGameButton() {
		_isLocalGame = true;
		Server.Instance.Init(_CONNECTION_PORT);
		Client.Instance.Init("127.0.0.1", _CONNECTION_PORT);
	}

	///<summary>
	///Initializes the server and the client
	///</summary>
	public void OnOnlineHostButton() {
		_isLocalGame = false;
		Server.Instance.Init(_CONNECTION_PORT);
		Client.Instance.Init("127.0.0.1", _CONNECTION_PORT);
	}

	///<summary>
	///Connects the client to the entered ip address
	///</summary>
	public void OnOnlineConnectButton() {
		_isLocalGame = false;
		Client.Instance.Init(_ipAddressInput.text, _CONNECTION_PORT);
	}

	///<summary>
	///Shutdown both the server and the client
	///</summary>
	public void OnHostCancelButton() {
		Server.Instance.ShutDown();
		Client.Instance.ShutDown();
	}

	///<summary>
	///Force swap which player's turn it is
	///</summary>
	public void OnSwapTurnsButton() {
		if (!_isLocalGame) return;

		NextTurn();
	}

	///<summary>
	///Undoes the last move
	///</summary>
	public void OnUndoLastMoveButton() {
		if (!AllowButtons) return;
		MoveCommandHandler.UndoCommand();
		AllowMoves = false;
	}

	///<summary>
	///Undoes all the moves
	///</summary>
	public void OnUndoAllMovesButton() {
		if (!AllowButtons) return;
		MoveCommandHandler.UndoAllCommands();
		AllowMoves = false;
	}

	///<summary>
	///Redoes the next move
	///</summary>
	public void OnRedoNextMoveButton() {
		if (!AllowButtons) return;
		MoveCommandHandler.RedoCommand();
		if (!MoveCommandHandler.AreCommandsWaiting()) AllowMoves = true;
	}

	///<summary>
	///Redoes all the moves
	///</summary>
	public void OnRedoAllMovesButton() {
		if (!AllowButtons) return;
		MoveCommandHandler.RedoAllCommands();
		AllowMoves = true;
	}

	///<summary>
	///Takeback the last move
	///</summary>
	public void OnTakebackMoveButton() {
		if (!AllowButtons) return;
		MoveCommandHandler.RedoAllCommands();
		if (MoveCommandHandler.UndoAndRemoveCommand()) SwitchCurrentPlayer();
		if (_prevPositions.Count > 0) _prevPositions.RemoveAt(_prevPositions.Count - 1);
		if (_prevAvailableMovesCount.Count > 0) _prevAvailableMovesCount.RemoveAt(_prevAvailableMovesCount.Count - 1);
		if (_moveRuleCount > 0) _moveRuleCount--;
		if (CurPlayer.IsWhite) MoveCount--;
		MovesPlayed.RemoveAt(MovesPlayed.Count - 1);
		_pgnBuilder.Moves.RemoveAt(_pgnBuilder.Moves.Count - 1);
		AllowMoves = true;
	}

	//Server
	///<summary>
	///Assigns the client their team and sends a start message when both clients have connected
	///</summary>
	///<param name="msg">Incoming message</param>
	///<param name="cnn">Receving connection</param>
	private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn) {
		NetWelcome welcomeMsg = msg as NetWelcome;
		welcomeMsg.IsAssignedWhitePieces = (++_playerCount == 1);
		Server.Instance.SendToClient(cnn, welcomeMsg);
		if (_playerCount == 2) Server.Instance.Broadcast(new NetStartGame());
	}

	///<summary>
	///Sends the game state to the clients
	///</summary>
	///<param name="msg">Incoming message</param>
	///<param name="cnn">Receving connection</param>
	private void OnGameStateServer(NetMessage msg, NetworkConnection cnn) {
		//relay the game state to the clients, excluding the connection the message was recieved from
		Server.Instance.Broadcast(msg, cnn);
	}

	///<summary>
	///Broadcasts a move from the server to all the clients
	///</summary>
	///<param name="msg">Incoming message</param>
	///<param name="cnn">Recieving connection</param>
	private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn) {
		//relay the make move message to the clients, excluding the connection the message was recieved from
		Server.Instance.Broadcast(msg, cnn);
	}

	//Client
	///<summary>
	///Adds a player to the game, broadcasts the start of the game from the server to all the clients
	///</summary>
	///<param name="msg">Incoming message</param>
	private void OnWelcomeClient(NetMessage msg) {
		NetWelcome welcomeMsg = msg as NetWelcome;
		_players = new Player[2] {new(true), new(false)};
		_myPlayer = GetPlayer(welcomeMsg.IsAssignedWhitePieces);
		if (_isLocalGame) Server.Instance.Broadcast(new NetStartGame());
	}

	///<summary>
	///Starts the game on the client
	///</summary>
	///<param name="msg">Incoming message</param>
	private void OnStartGameClient(NetMessage msg) {
		//disable the timers
		TimerManager.Instance.ToggleTimers(false);
		//initialize the game
		Init();
		//close all the menus
		MenuController.Instance.PopAllPages();
		//enable the game UI
		MenuController.Instance.PushPage(_gameUIPage);

		//start white's timer
		//TimerManager.Instance.StartFirstTimer();
	}

	///<summary>
	///Updates the status of the game on the client
	///</summary>
	///<param name="msg">Incoming message</param>
	private void OnGameStateClient(NetMessage msg) {
		NetGameState stateMsg = msg as NetGameState;
		if (State == stateMsg.State) return;
		_state = stateMsg.State;
		OnGameStateChange?.Invoke();
	}

	///<summary>
	///Makes a move on the client
	///</summary>
	///<param name="msg">Incoming message</param>
	private void OnMakeMoveClient(NetMessage msg) {
		NetMakeMove moveMsg = msg as NetMakeMove;
		if (moveMsg.IsWhiteMove == CurPlayer.IsWhite) return;
		MakeMove(
			ChessBoard.Instance.GetSquareAt(moveMsg.StartCoordinates),
			ChessBoard.Instance.GetSquareAt(moveMsg.EndCoordinates),
			moveMsg.IsABMove
		);
	}

	///<summary>
	///Returns data about the game
	///</summary>
	///<returns>Data about the game</returns>
	public override string ToString() {
		var str = new StringBuilder(base.ToString());
		str.Append("\nState: ").Append(State).Append("\nPlayers:");
		foreach (Player player in _players) str.Append("\n").Append(player.ToString());
		str.Append("\nCurrent Turn: ").Append(CurPlayer.ToString());
		str.Append("\nGame Board:\n").Append(ChessBoard.Instance.ToString());
		str.Append("\nMoves Made:");
		foreach (Move move in MovesPlayed) {
			str.Append("\n");
			if (move is AttackBoardMove) str.Append((move as AttackBoardMove).ToString());
			else str.Append((move as PieceMove).ToString());
			str.Append("\n");
		}
		return str.ToString();
	}
}