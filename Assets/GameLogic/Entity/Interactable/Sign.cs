using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class Sign : Interactable
{

    private static readonly string[] directions = { "Up", "Down", "Left", "Right" };
    private static readonly System.Random random = new System.Random();
    public string currentDirection = "Up";

    public override string getName()
    {
        return "sign";
    }

    public string GetDirection()
    {
        int index = random.Next(directions.Length);
        return directions[index];
    }

    
}
