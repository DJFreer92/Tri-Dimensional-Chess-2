using System;

[Flags]
public enum MoveEvent : ushort {
	NONE = 0,
	CAPTURE = 1,
	CASTLING_KING_SIDE = 1 << 1,
	CASTLING_QUEEN_SIDE = 1 << 2,
	PAWN_DOUBLE_SQUARE = 1 << 3,
	EN_PASSANT = 1 << 4 | CAPTURE,
	PROMOTION = 1 << 5,
	OPPONENT_PROMOTION = 1 << 6,
	ATTACKBOARD_CLAIM = 1 << 7,
	CHECK = 1 << 8,
	CHECKMATE = 1 << 9 | CHECK,
	DRAW_OFFERED = 1 << 10,
	CASTLING = CASTLING_KING_SIDE | CASTLING_QUEEN_SIDE
}

public static class MoveEventExtensions {
	///<summary>
	///Returns whether the given category contains the given event
	///</summary>
	///<params name="category">The category to check for the event in</params>
	///<params name="moveEvent">The event to look for in the category</params>
	///<returns>Whether the given category contains the given event</returns>
	public static bool Contains(this MoveEvent category, MoveEvent moveEvent) {
		return (category & moveEvent) == moveEvent;
	}

	///<summary>
	///Returns whether the given category contains the any of the given event
	///</summary>
	///<params name="category">The category to check for any of the event in</params>
	///<params name="moveEvent">The event to look for part of in the category</params>
	///<returns>Whether the given category contains any of the given event</returns>
	public static bool PartialContains(this MoveEvent category, MoveEvent moveEvent) {
		return (category & moveEvent) > 0;
	}

	///<summary>
	///Adds the given event to the referenced event
	///</summary>
	///<params name="moveEventRef">A reference to an event to add the given event to</params>
	///<params name="moveEvent">The event to add to the given event reference</params>
	public static void Add(this ref MoveEvent moveEventRef, params MoveEvent[] moveEvents) {
		if (moveEvents == null) throw new ArgumentNullException(nameof(moveEvents), "MoveEvents cannot be null");
		foreach (MoveEvent other in moveEvents) moveEventRef |= other;
	}

	///<summary>
	///Returns the MoveEvent of all the given events combined
	///</summary>
	///<params name="moveEvent">The event to add the given events to</params>
	///<params name="moveEvents">The events to be combined with the given event</params>
	///<returns>The MoveEvent of all the given events combined</returns>
	public static MoveEvent Combine(this MoveEvent moveEvent, params MoveEvent[] moveEvents) {
		if (moveEvents == null) throw new ArgumentNullException(nameof(moveEvents), "MoveEvents cannot be null");
		MoveEvent result = moveEvent;
		foreach (MoveEvent other in moveEvents) result |= other;
		return result;
	}
}