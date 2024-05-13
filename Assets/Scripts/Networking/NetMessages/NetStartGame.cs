using Unity.Networking.Transport;

namespace TriDimensionalChess.Networking.NetMessages {
	public class NetStartGame : NetMessage {
		public NetStartGame() : base(OpCode.START_GAME) {}  //creating

		public NetStartGame(DataStreamReader reader) : base(OpCode.START_GAME) {  //recieving
			Deserialize(reader);
		}

		///<summary>
		///Write the message to the given data stream
		///</summary>
		///<param name="writer">The stream to write the message to</param>
		public override void Serialize(ref DataStreamWriter writer) {
			base.Serialize(ref writer);
		}

		///<summary>
		///Read the message from the given data stream
		///</summary>
		///<param name="reader">The steam to read the message from</param>
		public override void Deserialize(DataStreamReader reader) {}

		///<summary>
		///Executes when the message is recieved on the client
		///</summary>
		public override void RecievedOnClient() {
			NetUtility.C_START_GAME?.Invoke(this);
		}

		///<summary>
		///Executes when the message is recieved on the server
		///</summary>
		///<param name="cnn">The connection the message was recieved on</param>
		public override void RecievedOnServer(NetworkConnection cnn) {
			NetUtility.S_START_GAME?.Invoke(this, cnn);
		}
	}
}
