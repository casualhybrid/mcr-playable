using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFigurineValue : SetKeyValues
{
    public override void GiveReward()
    {
        gamePlaySessionInventory.AddCharacterFigurine(int.Parse(keyValue), rewardValue);
    }
}
