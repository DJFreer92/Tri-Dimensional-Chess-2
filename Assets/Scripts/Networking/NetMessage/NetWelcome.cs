using System;
using Unity.Networking.Transport;

public class NetWelcome : NetMessage {
	public bool IsAssignedWhitePieces {get; set;}
	
	public NetWelcome() : base(OpCode.WELCOME) {}  //creating

	public NetWelcome(bool assignWhitePieces) : base(OpCode.WELCOME) {  //creating
		IsAssignedWhitePieces = assignWhitePieces;
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
	}

	///<summary>
	///Read the message from the given data stream
	///</summary>
	///<param name="reader">The steam to read the message from</param>
	public override void Deserialize(DataStreamReader reader) {
		IsAssignedWhitePieces = Convert.ToBoolean(reader.ReadByte());
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