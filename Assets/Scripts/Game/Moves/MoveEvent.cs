using System;

namespace TriDimensionalChess.Game.Moves {
	[Flags]
	public enum MoveEvent : ushort {
		NONE = 0,
		CAPTURE = 1,
		LOST_DOUBLE_SQUARE_MOVE_RIGHTS = 1 << 1,
		LOST_CASTLING_RIGHTS = 1 << 2,
		CASTLING_KING_SIDE = 1 << 3 | LOST_CASTLING_RIGHTS,
		CASTLING_QUEEN_SIDE = 1 << 4 | LOST_CASTLING_RIGHTS,
		PAWN_DOUBLE_SQUARE = 1 << 5 | LOST_DOUBLE_SQUARE_MOVE_RIGHTS,
		EN_PASSANT = 1 << 6 | CAPTURE,
		PROMOTION = 1 << 7,
		SECONDARY_PROMOTION = 1 << 8,
		ATTACK_BOARD_CLAIM = 1 << 9,
		ATTACK_BOARD_ROTATE = 1 << 10,
		ATTACK_BOARD_INVERSION = 1 << 11,
		PROMOTION_TO_ATTACK_BOARD = 1 << 12,
		CHECK = 1 << 13,
		CHECKMATE = 1 << 14 | CHECK,
		DRAW_OFFERED = 1 << 15,
		CASTLING = (CASTLING_KING_SIDE | CASTLING_QUEEN_SIDE) & ~LOST_CASTLING_RIGHTS,
		ANY_PROMOTION = PROMOTION | SECONDARY_PROMOTION | PROMOTION_TO_ATTACK_BOARD,
		LOST_RIGHTS = LOST_DOUBLE_SQUARE_MOVE_RIGHTS | LOST_CASTLING_RIGHTS
	}

	public static class MoveEventExtensions {
		///<summary>
		///Returns whether the given category contains the given event(s)
		///</summary>
		///<params name="category">The category to check for the event(s) in</params>
		///<params name="moveEvents">The event(s) to look for in the category</params>
		///<returns>Whether the given category contains the given event(s)</returns>
		public static bool Contains(this MoveEvent category, params MoveEvent[] moveEvents) {
			if (moveEvents == null) throw new ArgumentNullException(nameof(moveEvents), "MoveEvents cannot be null");
			foreach (MoveEvent evnt in moveEvents)
				if ((category & evnt) != evnt) return false;
			return true;
		}

		///<summary>
		///Returns whether the given category contains the any of the given event
		///</summary>
		///<params name="category">The category to check for any of the event in</params>
		///<params name="moveEvent">The event to look for part of in the category</params>
		///<returns>Whether the given category contains any of the given event</returns>
		public static bool PartialContains(this MoveEvent category, MoveEvent moveEvent) => (category & moveEvent) > 0;

		///<summary>
		///Adds the given event(s) to the referenced event
		///</summary>
		///<params name="moveEventRef">A reference to an event to add the given event(s) to</params>
		///<params name="moveEvent">The event(s) to add to the given event reference</params>
		public static void Add(this ref MoveEvent moveEventRef, params MoveEvent[] moveEvents) {
			if (moveEvents == null) throw new ArgumentNullException(nameof(moveEvents), "MoveEvents cannot be null");
			foreach (MoveEvent other in moveEvents) moveEventRef |= other;
		}

		///<summary>
		///Removes the given event(s) from the referenced events
		///</summary>
		///<param name="moveEventRef">A reference to an event to add the given event(s) to</param>
		///<param name="moveEvents">The event(s) to add to the given event reference</param>
		public static void Remove(this ref MoveEvent moveEventRef, params MoveEvent[] moveEvents) {
			if (moveEvents == null) throw new ArgumentNullException(nameof(moveEvents), "MoveEvents cannot be null");
			foreach (MoveEvent other in moveEvents) moveEventRef &= ~other;
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
}
