using UnityEngine;
using UnityEngine.Events;

public class IntelligentDecisionMaker : MonoBehaviour
{
    public static int NextBlock;

    public class MyUnityEvent : UnityEvent<int>
    {
    }

    public static MyUnityEvent decision = new MyUnityEvent();

    private void OnTriggerExit(Collider other)
    {
        DecisionFun(other);
    }

    public void DecisionFun(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            NextBlock++;
            if (NextBlock < 6)
                decision.Invoke(NextBlock);
            else
            {
                decision.Invoke(0);
            }
        }
    }
}