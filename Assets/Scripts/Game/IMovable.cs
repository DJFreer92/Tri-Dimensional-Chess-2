using System.Collections.Generic;
using UnityEngine;

public interface IMovable {
    List<Square> GetAvailableMoves(bool asWhite);
}