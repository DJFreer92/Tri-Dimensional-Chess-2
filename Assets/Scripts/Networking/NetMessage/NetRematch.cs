using System;
using Unity.Networking.Transport;

public class NetRematch : NetMessage {
	public bool FromWhite;
	public bool WantsRematch;

	public NetRematch() : base(OpCode.REMATCH) {}  //creating

	public NetRematch(DataStreamReader reader) : base(OpCode.REMATCH) {  //recieving
		Deserialize(reader);
	}

	///<summary>
	///Write the message to the given data stream
	///</summary>
	///<param name="writer">The stream to write the message to</param>
	public override void Serialize(ref DataStreamWriter writer) {
		base.Serialize(ref writer);
		writer.WriteByte(Convert.ToByte(FromWhite));
		writer.WriteByte(Convert.ToByte(WantsRematch));
	}

	///<summary>
	///Read the message from the given data stream
	///</summary>
	///<param name="reader">The steam to read the message from</param>
	public override void Deserialize(DataStreamReader reader) {
		FromWhite = Convert.ToBoolean(reader.ReadByte());
		WantsRematch = Convert.ToBoolean(reader.ReadByte());
	}

	///<summary>
	///Executes when the message is recieved on the client
	///</summary>
	public override void RecievedOnClient() {
		NetUtility.C_REMATCH?.Invoke(this);
	}

	///<summary>
	///Executes when the message is recieved on the server
	///</summary>
	///<param name="cnn">The connection the message was recieved on</param>
	public override void RecievedOnServer(NetworkConnection cnn) {
		NetUtility.S_REMATCH?.Invoke(this, cnn);
	}
}