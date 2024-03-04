using Unity.Networking.Transport;

public class NetFEN : NetMessage {
	public string FEN;

	public NetFEN() : base(OpCode.KEEP_ALIVE) {}  //creating

	public NetFEN(DataStreamReader reader) : base(OpCode.FEN) {  //recieving
		Deserialize(reader);
	}

	///<summary>
	///Write the message to the given data stream
	///</summary>
	///<param name="writer">The stream to write the message to</param>
	public override void Serialize(ref DataStreamWriter writer) {
		base.Serialize(ref writer);
		writer.WriteFixedString512(FEN);
	}

	///<summary>
	///Read the message from the given data stream
	///</summary>
	///<param name="reader">The steam to read the message from</param>
	public override void Deserialize(DataStreamReader reader) {
		FEN = reader.ReadFixedString512().ToString();
	}

	///<summary>
	///Executes when the message is recieved on the client
	///</summary>
	public override void RecievedOnClient() {
		NetUtility.C_FEN?.Invoke(this);
	}

	///<summary>
	///Executes when the message is recieved on the server
	///</summary>
	///<param name="cnn">The connection the message was recieved on</param>
	public override void RecievedOnServer(NetworkConnection cnn) {
		NetUtility.S_FEN?.Invoke(this, cnn);
	}
}