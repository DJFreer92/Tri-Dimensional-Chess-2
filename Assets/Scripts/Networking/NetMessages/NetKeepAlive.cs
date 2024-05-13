using Unity.Networking.Transport;

namespace TriDimensionalChess.Networking.NetMessages {
	public class NetKeepAlive : NetMessage {
		public NetKeepAlive() : base(OpCode.KEEP_ALIVE) {}  //creating

		public NetKeepAlive(DataStreamReader reader) : base(OpCode.KEEP_ALIVE) {  //recieving
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
			NetUtility.C_KEEP_ALIVE?.Invoke(this);
		}

		///<summary>
		///Executes when the message is recieved on the server
		///</summary>
		///<param name="cnn">The connection the message was recieved on</param>
		public override void RecievedOnServer(NetworkConnection cnn) {
			NetUtility.S_KEEP_ALIVE?.Invoke(this, cnn);
		}
	}
}
