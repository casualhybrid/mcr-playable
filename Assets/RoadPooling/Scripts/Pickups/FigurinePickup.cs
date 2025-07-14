using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigurinePickup : SpecialPickup
{
    [SerializeField] private CharacterConfigData characterData;

    public CharacterConfigData CharacterData => characterData;
}
