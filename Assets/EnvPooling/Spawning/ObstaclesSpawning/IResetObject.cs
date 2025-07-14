using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResetObject<T>
{
    T OriginalComponent { get; set; }
}
