using System;
using System.Collections.Generic;
using Unity.Networking.Transport;
using System.Linq;

public class NetPGN : NetMessage {
	public string PGN;

	public NetPGN() : base(OpCode.PGN) {}  //creating

	public NetPGN(DataStreamReader reader) : base(OpCode.PGN) {  //recieving
		Deserialize(reader);
	}

	///<summary>
	///Write the message to the given data stream
	///</summary>
	///<param name="writer">The stream to write the message to</param>
	public override void Serialize(ref DataStreamWriter writer) {
		base.Serialize(ref writer);
		List<char> chars = PGN.ToCharArray().ToList();
		writer.WriteByte((byte) (int) Math.Round(chars.Count / 2048.0, MidpointRounding.AwayFromZero));
		for (int i = 0; i < chars.Count; i += 2048)
			writer.WriteFixedString4096(chars.GetRange(0, Math.Clamp(i + 2048, i, chars.Count)).ToArray().ToString());
	}

	///<summary>
	///Read the message from the given data stream
	///</summary>
	///<param name="reader">The steam to read the message from</param>
	public override void Deserialize(DataStreamReader reader) {
		int numFixedStrings = (int) reader.ReadByte();
		for (int i = 0; i < numFixedStrings; i++)
			PGN += reader.ReadFixedString4096().ToString();
	}

	///<summary>
	///Executes when the message is recieved on the client
	///</summary>
	public override void RecievedOnClient() {
		NetUtility.C_PGN?.Invoke(this);
	}

	///<summary>
	///Executes when the message is recieved on the server
	///</summary>
	///<param name="cnn">The connection the message was recieved on</param>
	public override void RecievedOnServer(NetworkConnection cnn) {
		NetUtility.S_PGN?.Invoke(this, cnn);
	}
}