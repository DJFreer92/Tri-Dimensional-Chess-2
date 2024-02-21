using System;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMakeMove : NetMessage {
	public Vector3Int StartCoordinates {get; set;}
	public Vector3Int EndCoordinates {get; set;}
	public bool IsWhiteMove {get; set;}
	public bool IsABMove {get; set;}

	public NetMakeMove() : base(OpCode.MAKE_MOVE) {}  //creating
	
	public NetMakeMove(DataStreamReader reader) : base(OpCode.MAKE_MOVE) {  //recieving
		Deserialize(reader);
	}

	///<summary>
	///Write the message to the given data stream
	///</summary>
	///<param name="writer">The stream to write the message to</param>
	public override void Serialize(ref DataStreamWriter writer) {
		base.Serialize(ref writer);
		writer.WriteInt(StartCoordinates.x);
		writer.WriteInt(StartCoordinates.y);
		writer.WriteInt(StartCoordinates.z);
		writer.WriteInt(EndCoordinates.x);
		writer.WriteInt(EndCoordinates.y);
		writer.WriteInt(EndCoordinates.z);
		writer.WriteByte(Convert.ToByte(IsWhiteMove));
		writer.WriteByte(Convert.ToByte(IsABMove));
	}

	///<summary>
	///Read the message from the given data stream
	///</summary>
	///<param name="reader">The steam to read the message from</param>
	public override void Deserialize(DataStreamReader reader) {
		StartCoordinates = new Vector3Int(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		EndCoordinates = new Vector3Int(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		IsWhiteMove = Convert.ToBoolean(reader.ReadByte());
		IsABMove = Convert.ToBoolean(reader.ReadByte());
	}

	///<summary>
	///Executes when the message is recieved on the client
	///</summary>
	public override void RecievedOnClient() {
		NetUtility.C_MAKE_MOVE?.Invoke(this);
	}

	///<summary>
	///Executes when the message is recieved on the server
	///</summary>
	///<param name="cnn">The connection the message was recieved on</param>
	public override void RecievedOnServer(NetworkConnection cnn) {
		NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
	}
}