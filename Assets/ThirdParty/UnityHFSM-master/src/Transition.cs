using System;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// A class used to determin whether the state machine should transition to another state
    /// </summary>
    public class Transition : TransitionBase
    {
        public Func<Transition, bool> condition;
        public string EventName;

        /// <summary>
        /// Initialises a new instance of the Transition class
        /// </summary>
        /// <param name="from">The name / identifier of the active state</param>
        /// <param name="to">The name / identifier of the next state</param>
        /// <param name="condition">A function that returns true if the state machine
        /// 	should transition to the <c>to</c> state</param>
        /// <param name="forceInstantly">Ignores the needsExitTime of the active state if forceInstantly is true
        /// 	=> Forces an instant transition</param>
        public Transition(
                string from,
                string to,
                string Message,
                Func<Transition, bool> condition = null,
                bool forceInstantly = false) : base(from, to, forceInstantly)
        {
            this.EventName = Message;
            this.condition = condition;
        }

        public override bool ShouldTransition(string Event)
        {
            if (EventName == null)
            {
                UnityEngine.Console.LogError("No Event Specified For This Transition");
                return false;
            }

            //No Guards have been passed
            if (condition == null)
            {
                return (EventName == Event);
            }
            else
            {
                return (EventName == Event && condition(this));
            }
        }
    }
}