using Unity.Networking.Transport;

using TriDimensionalChess.Game;

namespace TriDimensionalChess.Networking.NetMessages {
	public class NetGameState : NetMessage {
		public GameState State;

		public NetGameState() : base(OpCode.GAME_STATE) {}  //creating

		public NetGameState(DataStreamReader reader) : base(OpCode.GAME_STATE) {  //recieving
			Deserialize(reader);
		}

		///<summary>
		///Write the message to the given data stream
		///</summary>
		///<param name="writer">The stream to write the message to</param>
		public override void Serialize(ref DataStreamWriter writer) {
			base.Serialize(ref writer);
			writer.WriteUShort((ushort) State);
		}

		///<summary>
		///Read the message from the given data stream
		///</summary>
		///<param name="reader">The steam to read the message from</param>
		public override void Deserialize(DataStreamReader reader) {
			State = (GameState) reader.ReadUShort();
		}

		///<summary>
		///Executes when the message is recieved on the client
		///</summary>
		public override void RecievedOnClient() {
			NetUtility.C_GAME_STATE?.Invoke(this);
		}

		///<summary>
		///Executes when the message is recieved on the server
		///</summary>
		///<param name="cnn">The connection the message was recieved on</param>
		public override void RecievedOnServer(NetworkConnection cnn) {
			NetUtility.S_GAME_STATE?.Invoke(this, cnn);
		}
	}
}
