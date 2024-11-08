using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_level_1 : Board
{
    public override bool hasWon() {
        List<Door> doors = getEntities<Door>();
        foreach (Door door in doors) {
            if (!door.isUnlocked()) {
                return false;
            }
        }
        setWinScreen(true);
        return true;
    }
}
