using System;
using Unity.Networking.Transport;

public class NetTimers : NetMessage {
	public float WhiteTime;
	public float BlackTime;
	public bool WhiteTimePaused;
	public bool BlackTimePaused;
	public bool TimersStopped;

	public NetTimers() : base(OpCode.TIMERS) {}  //creating

	public NetTimers(DataStreamReader reader) : base(OpCode.TIMERS) {  //recieving
		Deserialize(reader);
	}

	///<summary>
	///Write the message to the given data stream
	///</summary>
	///<param name="writer">The stream to write the message to</param>
	public override void Serialize(ref DataStreamWriter writer) {
		base.Serialize(ref writer);
		writer.WriteFloat(WhiteTime);
		writer.WriteFloat(BlackTime);
		writer.WriteByte(Convert.ToByte(WhiteTimePaused));
		writer.WriteByte(Convert.ToByte(BlackTimePaused));
		writer.WriteByte(Convert.ToByte(TimersStopped));
	}

	///<summary>
	///Read the message from the given data stream
	///</summary>
	///<param name="reader">The steam to read the message from</param>
	public override void Deserialize(DataStreamReader reader) {
		WhiteTime = reader.ReadFloat();
		BlackTime = reader.ReadFloat();
		WhiteTimePaused = Convert.ToBoolean(reader.ReadByte());
		BlackTimePaused = Convert.ToBoolean(reader.ReadByte());
		TimersStopped = Convert.ToBoolean(reader.ReadByte());
	}

	///<summary>
	///Executes when the message is recieved on the client
	///</summary>
	public override void RecievedOnClient() {
		NetUtility.C_TIMERS?.Invoke(this);
	}

	///<summary>
	///Executes when the message is recieved on the server
	///</summary>
	///<param name="cnn">The connection the message was recieved on</param>
	public override void RecievedOnServer(NetworkConnection cnn) {
		NetUtility.S_TIMERS?.Invoke(this, cnn);
	}
}