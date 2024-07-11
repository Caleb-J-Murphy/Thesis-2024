using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_AvoidSpikes : Board
{
    public override bool hasWon()
    {
        List<Ladder> ladders = getEntities<Ladder>();

        List<Hero> heros = getEntities<Hero>();
        foreach (Ladder ladder in ladders)
        {
            foreach (Hero hero in heros)
            {
                if (hero.getPosition() != ladder.getPosition()) {
                    return false;
                }
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
        if (this.numberOfLinesUsed() <= 7)
        {
            stars += 1;
        }
        if (this.getNumberCoins() >= 1)
        {
            stars += 1;
        }
        return stars;
    }

    
}
