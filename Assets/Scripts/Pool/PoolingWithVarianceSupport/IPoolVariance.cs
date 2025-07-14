using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolVariance<T>
{
	void Prewarm(int num);
	T Request(T key, int keyID);
	void Return(T member);
}
