using Unity.Networking.Transport;

public abstract class NetMessage {
	public readonly OpCode Code;

	public NetMessage(OpCode code) {
		Code = code;
	}

	///<summary>
	///Write the message to the given data stream
	///</summary>
	///<param name="writer">The stream to write the message to</param>
	public virtual void Serialize(ref DataStreamWriter writer) {
		writer.WriteByte((byte) Code);
	}

	///<summary>
	///Read the message from the given data stream
	///</summary>
	///<param name="reader">The steam to read the message from</param>
	public virtual void Deserialize(DataStreamReader reader) {}

	///<summary>
	///Executes when the message is recieved on the client
	///</summary>
	public virtual void RecievedOnClient() {}

	///<summary>
	///Executes when the message is recieved on the server
	///</summary>
	///<param name="cnn">The connection the message was recieved on</param>
	public virtual void RecievedOnServer(NetworkConnection cnn) {}
}