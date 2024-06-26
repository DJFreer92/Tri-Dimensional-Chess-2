using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using System.Linq;
using System.Net;

using TriDimensionalChess.Networking.NetMessages;
using TriDimensionalChess.Tools;

namespace TriDimensionalChess.Networking {
	[DisallowMultipleComponent]
	public sealed class Server : MonoSingleton<Server> {
		private const float _KEEP_ALIVE_TICK_RATE = 20f;
		public Action ConnectionDropped;
		private NetworkDriver _driver;
		private NativeList<NetworkConnection> _connections;
		private bool _isActive;
		private float _lastKeepAlive;

		private void Update() {
			if (!_isActive) return;

			KeepAlive();

			_driver.ScheduleUpdate().Complete();

			CleanUpConnections();
			AcceptNewConnections();
			UpdateMessagePump();
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			ShutDown();
		}

		///<summary>
		///Inits the server
		///</summary>
		///<param name="port">The port to bind to</param>
		public void Init(ushort port) {
			_driver = NetworkDriver.Create();
			NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
			endPoint.Port = port;

			if (_driver.Bind(endPoint) != 0) {
				Debug.Log("Unable to bind on port " + endPoint.Port);
				return;
			}

			_driver.Listen();
			Debug.Log("Currently listening on port " + endPoint.Port);

			_connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
			_isActive = true;
		}

		///<summary>
		///Shuts down the server
		///</summary>
		public void ShutDown() {
			if (!_isActive) return;
			_driver.Dispose();
			_connections.Dispose();
			_isActive = false;
		}

		///<summary>
		///Broadcasts a keep alive message to all the clients at a fixed interval
		///</summary>
		private void KeepAlive() {
			if (Time.time - _lastKeepAlive <= _KEEP_ALIVE_TICK_RATE) return;
			_lastKeepAlive = Time.time;
			Broadcast(new NetKeepAlive());
		}

		///<summary>
		///Removes inactive connections
		///</summary>
		private void CleanUpConnections() {
			for (var i = 0; i < _connections.Length; i++)
				if (!_connections[i].IsCreated) _connections.RemoveAtSwapBack(i--);
		}

		///<summary>
		///Accepts all incoming connections
		///</summary>
		private void AcceptNewConnections() {
			NetworkConnection c;
			while ((c = _driver.Accept()) != default) _connections.Add(c);
		}

		///<summary>
		///Decodes all incoming messages
		///</summary>
		private void UpdateMessagePump() {
			for (var i = 0; i < _connections.Length; i++) {
				NetworkEvent.Type cmd;
				while ((cmd = _driver.PopEventForConnection(_connections[i], out DataStreamReader stream)) != NetworkEvent.Type.Empty) {
					if (cmd == NetworkEvent.Type.Data) {
						NetUtility.OnData(stream, _connections[i], this);
						continue;
					}
					if (cmd != NetworkEvent.Type.Disconnect) continue;
					Debug.Log("Client disconnected from server");
					_connections[i] = default;
					ConnectionDropped?.Invoke();
					ShutDown();
				}
			}
		}

		///<summary>
		///Boardcasts the given message to all the clients
		///</summary>
		///<param name="msg">The message to broadcast</param>
		public void Broadcast(NetMessage msg) {
			foreach (NetworkConnection connection in _connections) {
				if (!connection.IsCreated) continue;
				//Debug.Log($"Sending {msg.Code} to: {connection.InternalId}");
				SendToClient(connection, msg);
			}
		}

		///<summary>
		///Boardcasts the given message to all the clients except the given clients to be excluded
		///</summary>
		///<param name="msg">The message to broadcast</param>
		///<param name="excludedCnns">Connections to exclude from the broadcast</param>
		public void Broadcast(NetMessage msg, params NetworkConnection[] excludedCnns) {
			foreach (NetworkConnection connection in _connections) {
				if (!connection.IsCreated) continue;
				foreach (NetworkConnection excludedCnn in excludedCnns)
					if (connection.Equals(excludedCnn)) continue;
				Debug.Log($"Sending {msg.Code} to: {connection.InternalId}");
				SendToClient(connection, msg);
			}
		}

		///<summary>
		///Send the given message to the client across the given connection
		///</summary>
		///<param name="connection">The connection to the client</param>
		///<param name="msg">The message to be sent</param>
		public void SendToClient(NetworkConnection connection, NetMessage msg) {
			_driver.BeginSend(connection, out DataStreamWriter writer);
			msg.Serialize(ref writer);
			_driver.EndSend(writer);
		}

		///<summary>
		///returns the ip address of the server
		///</summary>
		public string GetIPAddress() {
			return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(
				f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
			).ToString();
		}
	}
}
