using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

[RequireComponent(typeof(Highlight))]
[DisallowMultipleComponent]
public sealed class Square : MonoBehaviour {
	//whether an attackboard can rest on this square
	[field: SerializeField] public bool HasAttackBoardPin {get; private set;}
	//x, y, and z coordinates on the board
	[field: SerializeField] public Vector3Int Coords {get; set;}
	//the piece on the square
	[field: SerializeField] public ChessPiece GamePiece {get; set;}
	//whether the square has an attackboard on it
	[field: SerializeField] public bool IsOccupiedByAB {get; set;}
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
				if (brd.Y == Coords.y && brd.Squares.Contains(this)) return brd;
			return null;
		}
	}
	//the square's board notation
	public string BrdNotation { get => Brd.Notation; }
	//the square's board height
	public int BrdHeight { get => Coords.y; }
	//the square's notation
	public string Notation { get => Coords.VectorToNotation(); }
	//the highlight component
	private Highlight _highlight;

	private void Awake() => _highlight = GetComponent<Highlight>();

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
	public void Clear() {
		if (HasPiece()) Destroy(GamePiece.gameObject);
		IsOccupiedByAB = false;
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
	public bool IsWhite() => (Coords.x + Coords.z) % 2 == 1;

	///<summary>
	///Returns whether the files of the two squares are the same
	///</summary>
	///<param name="other">The other square</param>
	///<returns>Whether the files of the two squares are the same</returns>
	public bool FileMatch(Square other) => Coords.x == other.Coords.x;

	///<summary>
	///Returns whether the ranks of the two squares are the same
	///</summary>
	///<param name="other">The other square</param>
	///<returns>Whether the ranks of the two squares are the same</returns>
	public bool RankMatch(Square other) => Coords.z == other.Coords.z;

	///<summary>
	///Returns whether the boards of the two squares are the same
	///</summary>
	///<param name="other">The other square</param>
	///<returns>Whether the boards of the two squares are the same</returns>
	public bool BoardMatch(Square other) => Brd == other.Brd;

	///<summary>
	///Toggle whether the square is highlighted
	///</summary>
	///<param name="toggle">Whether to toggle the highlight on or off</param>
	public void ToggleHighlight(bool toggle) => _highlight.ToggleHighlight(toggle);

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
