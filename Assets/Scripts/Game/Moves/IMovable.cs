using System.Collections.Generic;
using UnityEngine;

using TriDimensionalChess.Game.Boards;

namespace TriDimensionalChess.Game.Moves {
    public interface IMovable {
        List<Square> GetAvailableMoves(bool asWhite);
    }
}
