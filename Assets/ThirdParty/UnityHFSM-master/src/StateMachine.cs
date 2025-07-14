using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Hierarchichal finite state machine for Unity
 * by LavaAfterburner
 *
 * Version: 1.4.0
 */

namespace FSM
{
    /// <summary>
    /// A finite state machine
    /// </summary>
    public class StateMachine : StateBase
    {
        public event Action<StateBase> StateChanged;

        private string startState;
        private string pendingState;
        public StateBase activeState;
        public StateBase pendingStateToBeChangedTo;
        private TransitionBase[] activeTransitions;

        private Dictionary<string, StateBase> states = new Dictionary<string, StateBase>();
        private Dictionary<string, List<TransitionBase>> transitions = new Dictionary<string, List<TransitionBase>>();

        /// <summary>
        /// DeSubscribe the state machine from any registered events
        /// </summary>
        public void DeSubscribeEvents()
        {
            //   StateMachineEventsSender.StateMachineEvent -= StateMachineEventReceieved;
        }

        /// <summary>
        /// Initialises a new instance of the StateMachine class
        /// </summary>
        /// <param name="mono">The MonoBehaviour of the script that created the state machine</param>
        /// <param name="needsExitTime">(Only for hierarchical states):
        /// 	Determins whether the state machine as a state of a parent state machine is allowed to instantly
        /// 	exit on a transition (false), or if it should wait until the active state is ready for a
        /// 	state change (true)</param>
        //public StateMachine(MonoBehaviour mono, bool needsExitTime = true) : base(needsExitTime)
        //{
        //    this.mono = mono;
        //}

        /// <summary>
        /// Notifies the state machine that the state can cleanly exit,
        /// and if a state change is pending, it will execute it
        /// </summary>
        public void StateCanExit()
        {
            if (pendingState != null)
            {
                ChangeState(pendingState);
                pendingState = null;
            }

            if (fsm != null)
            {
                fsm.StateCanExit();
            }
        }

        public override void RequestExit()
        {
            activeState.RequestExit();
        }

        /// <summary>
        /// Request a state change, respecting the <c>needsExitTime</c> property of the active state
        /// </summary>
        /// <param name="name">The name / identifier of the target state</param>
        /// <param name="forceInstantly">Overrides the needsExitTime of the active state if true,
        /// therefore forcing an immediate state change</param>
        public void RequestStateChange(string name, bool forceInstantly = false)
        {
            if (!activeState.needsExitTime || forceInstantly)
            {
                ChangeState(name);
            }
            else
            {
                pendingState = name;
                activeState.RequestExit();
                /**
				 * If it can exit, the activeState would call
            	 * -> state.fsm.StateCanExit()
            	 * -> fsm.ChangeState(...)
				 */
            }
        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            activeState.OnTriggerEnter(other);
        }

        public override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);

            activeState.OnTriggerExit(other);
        }

        /// <summary>
        /// Instantly changes to the target state
        /// </summary>
        /// <param name="name">The name / identifier of the active state</param>
        private void ChangeState(string name)
        {
            if (!states.ContainsKey(name))
            {
                System.Exception exception = new System.Exception(
                    $"The state '{name}' has not been defined yet / doesn't exist"
                );
            }

            pendingStateToBeChangedTo = states[name];

            if (activeState != null)
            {
                activeState.OnExit();
            }

            activeState = states[name];

            StateChanged?.Invoke(activeState);

            activeState.OnEnter();

            pendingStateToBeChangedTo = null;

            if (transitions.ContainsKey(name))
            {
                activeTransitions = transitions[name].ToArray();

                for (int i = 0; i < activeTransitions.Length; i++)
                {
                    activeTransitions[i].OnEnter();
                }
            }
            else
            {
                activeTransitions = new Transition[] { };
            }
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += Reset;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= Reset;
        }

        private void Reset(Scene from, Scene To)
        {
            StateMachineEventsSender.StateMachineEvent -= StateMachineEventReceieved;
        }

        public override void OnEnter()
        {
            StateMachineEventsSender.StateMachineEvent += StateMachineEventReceieved;
            ChangeState(startState);
        }

        private void StateMachineEventReceieved(string Event)
        {
            //Dont forget to desubscribe to the event

            if (activeState == null)
            {
                throw new System.Exception("The FSM has not been initialised yet! "
                    + "Call fsm.SetStartState(...) and fsm.OnEnter() to initialise");
            }
            foreach (TransitionBase transition in activeTransitions)
            {
                if (!transition.ShouldTransition(Event))
                    continue;

                if (!activeState.needsExitTime || transition.forceInstantly)
                {
                    ChangeState(transition.to);
                }
                else
                {
                    RequestStateChange(transition.to);
                }

                break;
            }

            // activeState.OnLogic();
        }

        public override void OnLogic()
        {
            if (activeState == null)
            {
                throw new System.Exception("The FSM has not been initialised yet! "
                    + "Call fsm.SetStartState(...) and fsm.OnEnter() to initialise");
            }
            //foreach (TransitionBase transition in activeTransitions)
            //{
            //    if (!transition.ShouldTransition())
            //        continue;

            //    if (!activeState.needsExitTime || transition.forceInstantly)
            //    {
            //        ChangeState(transition.to);
            //    }
            //    else
            //    {
            //        RequestStateChange(transition.to);
            //    }

            //    break;
            //}

            activeState.OnLogic();
        }

        public override void OnFixedLogic()
        {
            if (activeState == null)
            {
                throw new System.Exception("The FSM has not been initialised yet! "
                    + "Call fsm.SetStartState(...) and fsm.OnEnter() to initialise");
            }
            //foreach (TransitionBase transition in activeTransitions)
            //{
            //    if (!transition.ShouldTransition())
            //        continue;

            //    if (!activeState.needsExitTime || transition.forceInstantly)
            //    {
            //        ChangeState(transition.to);
            //    }
            //    else
            //    {
            //        RequestStateChange(transition.to);
            //    }

            //    break;
            //}

            activeState.OnFixedLogic();
        }

        /// <summary>
        /// Adds a new node / state to the state machine
        /// </summary>
        /// <param name="name">The name / identifier of the new state</param>
        /// <param name="state">The new state instance, e.g. <c>State</c>, <c>CoState</c>, <c>StateMachine</c></param>
        public void AddState(string name, StateBase state)
        {
            state.fsm = this;
            state.name = name;
            state.mono = mono;

            states[name] = state;

            if (states.Count == 1 && startState == null)
            {
                SetStartState(name);
            }
        }

        /// <summary>
        /// Defines the entry point of the state machine
        /// </summary>
        /// <param name="name">The name / identifier of the start state</param>
        public void SetStartState(string name)
        {
            startState = name;
        }

        /// <summary>
        /// Adds a new transition between two states
        /// </summary>
        /// <param name="transition">The transition instance</param>
        public void AddTransition(TransitionBase transition)
        {
            if (!transitions.ContainsKey(transition.from))
            {
                transitions[transition.from] = new List<TransitionBase>();
            }

            transition.fsm = this;
            transition.mono = mono;

            transitions[transition.from].Add(transition);
        }

        public StateMachine this[string name]
        {
            get
            {
                if (!states.ContainsKey(name))
                {
                    System.Exception exception = new System.Exception(
                        $"The state '{name}' has not been defined yet / doesn't exist"
                    );
                }

                StateBase selectedNode = states[name];

                if (!(selectedNode is StateMachine))
                {
                    System.Exception exception = new System.Exception(
                        $"The state '{name}' is not a StateMachine"
                    );
                }

                return (StateMachine)selectedNode;
            }
        }
    }
}