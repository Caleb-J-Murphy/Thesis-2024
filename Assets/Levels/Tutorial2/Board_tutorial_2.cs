using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_tutorial_2 : Board
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
