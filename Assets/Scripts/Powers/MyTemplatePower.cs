using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MyTemplatePower : ScriptableObject
{
    public int currentCharges;

    public int totalCharges;

    public void Activate(Player player)
    {
        if (currentCharges > 0)
        {
            currentCharges--;
            ActivateOnce(player);
        }
        else
        {
            //An animation where the player to show the player does not have enough charges
        }
    }

    public abstract void ActivateOnce(Player player);
}