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

    public override int getStars()
    {
        int stars = 0;
        if (hasWon())
        {
            stars += 1;
        }
        if (this.usedLoop())
        {
            stars += 1;
        }
        if (this.numberOfLinesUsed() < 18)
        {
            stars += 1;
        }
        return stars;
    }
}
