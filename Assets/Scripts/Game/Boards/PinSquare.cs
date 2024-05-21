using UnityEngine;

namespace TriDimensionalChess.Game.Boards {
	public sealed class PinSquare : Square {
		//the attack boards pinned to the top and bottom of the square
		[field: SerializeField] public AttackBoard TopPin {get; set;}
		[field: SerializeField] public AttackBoard BottomPin {get; set;}
		//the level of an attack board pinned to the square
		public string Level {get; private set;}

		protected override void Awake() {
			base.Awake();
			InitializeLevel();
		}

		///<summary>
		///Clear the square
		///</summary>
        public override void Clear() {
            base.Clear();
			TopPin?.Clear();
			BottomPin?.Clear();
        }

		///<summary>
		///Sets the level of any attack board that is pinned to this square
		///</summary>
		private void InitializeLevel() =>
			Level = (FileIndex == 1 ? "QL" : "KL") + (Rank % 2 == 0 ? Rank - 2 : Rank);

        ///<summary>
        ///Returns whether the square has an attack board pinned to it
        ///</summary>
        ///<returns></returns>
        public bool IsOccupiedByAB() => IsTopPinOccupied() || IsBottomPinOccupied();

		///<summary>
		///Returns whether both the top and bottom pins have attack boards
		///</summary>
		///<returns>Whether both the top and bottom pins have attack boards</returns>
		public bool IsFullyOccupiedByABs() => IsTopPinOccupied() && IsBottomPinOccupied();

		///<summary>
		///Returns whether there is an attack board pinned to the top of the square
		///</summary>
		///<returns>Whether there is an attack board pinned to the top of the square</returns>
		public bool IsTopPinOccupied() => TopPin != null;

		///<summary>
		///Returns whether there is an attack board pinned to the bottom of the square
		///</summary>
		///<returns>Whether there is an attack board pinned to the bottom of the square</returns>
		public bool IsBottomPinOccupied() => BottomPin != null;
	}
}
