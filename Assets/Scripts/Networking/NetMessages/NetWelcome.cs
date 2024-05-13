using System;
using Unity.Networking.Transport;

namespace TriDimensionalChess.Networking.NetMessages {
	public class NetWelcome : NetMessage {
		public bool IsAssignedWhitePieces;
		public bool StartingWithFEN;
		public string FENOrPGN = "";

		public NetWelcome() : base(OpCode.WELCOME) {}  //creating

		public NetWelcome(bool assignWhitePieces, bool startingWithFEN, string fenOrPGN) : base(OpCode.WELCOME) {  //creating
			IsAssignedWhitePieces = assignWhitePieces;
			StartingWithFEN = startingWithFEN;
			FENOrPGN = fenOrPGN;
		}

		public NetWelcome(DataStreamReader reader) : base(OpCode.WELCOME) {  //recieving
			Deserialize(reader);
		}

		///<summary>
		///Write the message to the given data stream
		///</summary>
		///<param name="writer">The stream to write the message to</param>
		public override void Serialize(ref DataStreamWriter writer) {
			base.Serialize(ref writer);

			writer.WriteByte(Convert.ToByte(IsAssignedWhitePieces));
			writer.WriteByte(Convert.ToByte(StartingWithFEN));
			writer.WriteFixedString4096(FENOrPGN);
		}

		///<summary>
		///Read the message from the given data stream
		///</summary>
		///<param name="reader">The steam to read the message from</param>
		public override void Deserialize(DataStreamReader reader) {
			IsAssignedWhitePieces = Convert.ToBoolean(reader.ReadByte());
			StartingWithFEN = Convert.ToBoolean(reader.ReadByte());
			FENOrPGN = reader.ReadFixedString4096().ToString();
		}

		///<summary>
		///Executes when the message is recieved on the client
		///</summary>
		public override void RecievedOnClient() {
			NetUtility.C_WELCOME?.Invoke(this);
		}

		///<summary>
		///Executes when the message is recieved on the server
		///</summary>
		///<param name="cnn">The connection the message was recieved on</param>
		public override void RecievedOnServer(NetworkConnection cnn) {
			NetUtility.S_WELCOME?.Invoke(this, cnn);
		}
	}
}
