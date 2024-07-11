using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class Sign : Interactable
{

    private string[] directions = { "Up", "Down", "Left", "Right" };
    protected System.Random random = new System.Random();
    public string currentDirection = "Up";

    public override string getName()
    {
        return "sign";
    }

    public virtual string GetDirection()
    {
        int index = random.Next(this.directions.Length);
        return this.directions[index];
    }

    
}
