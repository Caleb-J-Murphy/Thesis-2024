using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_StairMapLevel : Board
{
    public override bool hasWon()
    {
        List<Ladder> ladders = getEntities<Ladder>();

        List<Hero> heros = getEntities<Hero>();
        foreach (Ladder ladder in ladders)
        {
            foreach (Hero hero in heros)
            {
                if (hero.getPosition() == ladder.getPosition()) {
                    setWinScreen(true);
                    return true;
                }
            }
        }
        return false;
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
        if (this.numberOfLinesUsed() <= 7)
        {
            stars += 1;
        }
        return stars;
    }
}
