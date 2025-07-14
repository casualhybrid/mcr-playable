using System;

namespace FSM
{
    public static class StateMachineEventsSender
    {
        public static event Action<string> StateMachineEvent;

        public static void SendStateMachineEvent(string name)
        {
            StateMachineEvent(name);
        }
    }
}