using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

using TriDimensionalChess.Game.ChessPieces;
using TriDimensionalChess.Game.Notation;
using TriDimensionalChess.UI;

namespace TriDimensionalChess.Game.Boards {
	[RequireComponent(typeof(Highlight))]
	[DisallowMultipleComponent]
	public class Square : MonoBehaviour {
		//the highlight component
		private SquareHighlight _highlight;

		//x, y, and z coordinates on the board
		[field: SerializeField] public Vector3Int Coords {get; set;}
		//the piece on the square
		[field: SerializeField] public ChessPiece GamePiece {get; set;}
		//the square's file index
		public int FileIndex { get => Coords.x; }
		//the square's file
		public char File { get => FileIndex.IndexToFile(); }
		//the square's rank
		public int Rank { get => Coords.z; }
		//the square's board
		public Board Brd {
			get {
				foreach (Board brd in ChessBoard.Instance)
					if (brd.Y == BrdHeight && brd.Squares.Contains(this)) return brd;
				return null;
			}
		}
		//the square's board height
		public int BrdHeight { get => Coords.y; }
		//the square's board notation
		public string BrdNotation { get => Brd.Notation; }
		//the square's notation
		public string Notation { get => $"{File}{Rank}{Brd.Notation}"; }

		protected virtual void Awake() => _highlight = GetComponent<SquareHighlight>();

		private void OnMouseUpAsButton() {
			//if the mouse is over a UI element, exit
			if (EventSystem.current.IsPointerOverGameObject()) return;

			//if the game is not active, exit
			if (Game.Instance.State.Is(GameState.INACTIVE)) return;

			//select this square
			Game.Instance.SelectSquare(this);
		}

		///<summary>
		///Clear the square
		///</summary>
		public virtual void Clear() {
			if (HasPiece()) Destroy(GamePiece.gameObject);
		}

		///<summary>
		///Returns whether the square has a piece on it
		///</summary>
		///<returns>Whether the square has a piece on it</returns>
		public bool HasPiece() => GamePiece != null;

		///<summary>
		///Returns whether the square is white
		///</summary>
		///<returns>Wether the square is white</returns>
		public bool IsLightSquare() => (Coords.x + Coords.z) % 2 == 1;

		///<summary>
		///Returns whether the files of the two squares are the same
		///</summary>
		///<param name="other">The other square</param>
		///<returns>Whether the files of the two squares are the same</returns>
		public bool FileMatch(Square other) => FileIndex == other.FileIndex;

		///<summary>
		///Returns whether the ranks of the two squares are the same
		///</summary>
		///<param name="other">The other square</param>
		///<returns>Whether the ranks of the two squares are the same</returns>
		public bool RankMatch(Square other) => Rank == other.Rank;

		///<summary>
		///Returns whether the boards of the two squares are the same
		///</summary>
		///<param name="other">The other square</param>
		///<returns>Whether the boards of the two squares are the same</returns>
		public bool BoardMatch(Square other) => Brd == other.Brd;

		///<summary>
		///Returns whether the the heights of the two squares are the same
		///</summary>
		///<param name="other">The other square</param>
		///<returns>Whether the heights of the two squares are the same</returns>
		public bool HeightMatch(Square other) => BrdHeight == other.BrdHeight;

		///<summary>
		///Toggle whether the square is highlighted as selected
		///</summary>
		///<param name="toggle">Whether to toggle the highlight on or off</param>
		public void ToggleSelectedHighlight(bool toggle) => _highlight.ToggleSelectedHighlight(toggle);

		///<summary>
		///Toggle whether the square is highlighted as available
		///</summary>
		///<param name="toggle">Whether to toggle the highlight on or off</param>
		public void ToggleAvailableHighlight(bool toggle) => _highlight.ToggleAvailableHighlight(toggle);

		///<summary>
		///Toggle whether the square is highlighted as an available capture
		///</summary>
		///<param name="toggle">Whether to toggle the highlight on or off</param>
		public void ToggleCaptureHighlight(bool toggle) => _highlight.ToggleCaptureHighlight(toggle);

		///<summary>
		///Returns a copy of the square
		///</summary>
		///<returns>A copy of the square</returns>
		public object Clone() => MemberwiseClone();

		///<summary>
		///Returns a string of data about the square
		///</summary>
		///<returns>A string of data about the square</returns>
		public override string ToString() {
			var str = new StringBuilder(base.ToString());
			str.Append("\nCoordinates: ").Append(Coords.ToString());
			str.Append("\nNotation: ").Append(Notation);
			str.Append("\nPiece:\n").Append(HasPiece() ? GamePiece.ToString() : "null");
			return str.ToString();
		}
	}
}
