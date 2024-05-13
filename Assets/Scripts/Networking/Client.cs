using System;
using Unity.Networking.Transport;
using UnityEngine;

using TriDimensionalChess.Networking.NetMessages;
using TriDimensionalChess.Tools;

namespace TriDimensionalChess.Networking {
	[DisallowMultipleComponent]
	public sealed class Client : MonoSingleton<Client> {
		[HideInInspector] public Action ConnectionDropped;
		private NetworkDriver _driver;
		private NetworkConnection _connection;
		private bool _isActive = false;

		public void Update() {
			if (!_isActive) return;

			_driver.ScheduleUpdate().Complete();
			CheckAlive();

			UpdateMessagePump();
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			ShutDown();
		}

		///<summary>
		///Inits the client
		///</summary>
		///<param name="ip">The IP address of the server</param>
		///<param name="port">The port to bind to</param>
		public void Init(string ip, ushort port) {
			_driver = NetworkDriver.Create();
			NetworkEndPoint endPoint = NetworkEndPoint.Parse(ip, port);

			_connection = _driver.Connect(endPoint);

			Debug.Log("Attempting to connect to Server on " + endPoint.Address);

			_isActive = true;

			RegisterToEvents();
		}

		///<summary>
		///Shut down the client
		///</summary>
		public void ShutDown() {
			if (_isActive) {
				UnregisterToEvents();
				_driver.Dispose();
				_isActive = false;
				_connection = default;
			}
		}

		///<summary>
		///check if the connection to the server is still active
		///</summary>
		private void CheckAlive() {
			if (_connection.IsCreated || !_isActive) return;
			Debug.Log("Something went wrong, lost connection to server");
			ConnectionDropped?.Invoke();
			ShutDown();
		}

		///<summary>
		///Decode all incoming messages
		///</summary>
		private void UpdateMessagePump() {
			NetworkEvent.Type cmd;
			while ((cmd = _connection.PopEvent(_driver, out DataStreamReader stream)) != NetworkEvent.Type.Empty) {
				switch (cmd) {
					case NetworkEvent.Type.Connect:
						Debug.Log("Connected to server");
						SendToServer(new NetWelcome());
						continue;
					case NetworkEvent.Type.Data:
						NetUtility.OnData(stream, default);
						continue;
					case NetworkEvent.Type.Disconnect:
						Debug.Log("Client got disconnected from server");
						_connection = default;
						ConnectionDropped?.Invoke();
						ShutDown();
						continue;
				}
			}
		}

		///<summary>
		///Send the given message to the server
		///</summary>
		///<param name="msg">The message to send to the server</param>
		public void SendToServer(NetMessage msg) {
			_driver.BeginSend(_connection, out DataStreamWriter writer);
			msg.Serialize(ref writer);
			_driver.EndSend(writer);
		}

		///<summary>
		///Adds local methods to Actions
		///</summary>
		private void RegisterToEvents() {
			NetUtility.C_KEEP_ALIVE += OnKeepAlive;
		}

		///<summary>
		///Removes local methods from Actions
		///</summary>
		private void UnregisterToEvents() {
			NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
		}

		///<summary>
		///Send a keep alive message to the server
		///</summary>
		///<param name="nm">Keep alive message to send to the server</param>
		private void OnKeepAlive(NetMessage nm) {
			SendToServer(nm);
		}
	}
}
