using System;
using Unity.Networking.Transport;
using UnityEngine;

public static class NetUtility {
	public static Action<NetMessage> C_KEEP_ALIVE, C_WELCOME, C_FEN, C_PGN, C_START_GAME, C_GAME_STATE, C_TIMERS, C_MAKE_MOVE, C_REMATCH;
	public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE, S_WELCOME, S_FEN, S_PGN, S_START_GAME, S_GAME_STATE, S_TIMERS, S_MAKE_MOVE, S_REMATCH;

	///<summary>
	///Decodes the incoming data stream
	///</summary>
	///<param name="stream">The incoming data stream</param>
	///<param name="cnn">The connection the data stream is be recieved on</param>
	///<param name="server">The server which recieved the data stream</param>
	public static void OnData(DataStreamReader stream, NetworkConnection cnn, Server server = null) {
		NetMessage msg = null;
		OpCode opCode = (OpCode) stream.ReadByte();
		switch (opCode) {
			case OpCode.KEEP_ALIVE: msg = new NetKeepAlive(stream);
			break;
			case OpCode.WELCOME: msg = new NetWelcome(stream);
			break;
			case OpCode.FEN: msg = new NetFEN(stream);
			break;
			case OpCode.PGN: msg = new NetPGN(stream);
			break;
			case OpCode.START_GAME: msg = new NetStartGame(stream);
			break;
			case OpCode.GAME_STATE: msg = new NetGameState(stream);
			break;
			case OpCode.TIMERS: msg = new NetTimers(stream);
			break;
			case OpCode.MAKE_MOVE: msg = new NetMakeMove(stream);
			break;
			case OpCode.REMATCH: msg = new NetRematch(stream);
			break;
			default: Debug.LogError("Message recieved has no OpCode");
			break;
		}

		if (server != null) msg.RecievedOnServer(cnn);
		else msg.RecievedOnClient();
	}
}