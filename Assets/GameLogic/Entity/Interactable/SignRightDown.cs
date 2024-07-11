using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignRightDown : Sign
{
    private string[] directions = { "Down", "Right" };

    public override string GetDirection()
    {
        int index = this.random.Next(this.directions.Length);
        return this.directions[index];
    }
}
