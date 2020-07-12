using System.Collections.Generic;
using System;

namespace Nitro.FSM
{
    /// <summary>
    /// Command based Finite State Machine.
    /// 
    /// Enums must be marqued with System.Flags Attribute and it's elements must begin with 1.
    /// </summary>
    /// <typeparam name="K">Enum States</typeparam>
    /// <typeparam name="T">Enum commands</typeparam>
    public class CommandFSM<K, T> where K : struct, IComparable, IFormattable, IConvertible
        where T : struct, IComparable, IFormattable, IConvertible
    {
        public Action<K> OnStateChanged = null;

        public Action<T> OnCommandChanged = null;

        public K CurrentState { get; private set; }
        public K LastState { get; private set; }

        public T CurrentCommand { get; private set; }


        private Dictionary<CommandTransition<K, T>, Func<K>> Transitions;

        public CommandFSM()
        {
            if (!typeof(K).IsEnum || !typeof(T).IsEnum)
            {
                throw new ArgumentException("CommandFSM: Only enums are supported");
            }else if (typeof(K).GetCustomAttributes(typeof(FlagsAttribute), false).Length <= 0 ||
                typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Length <= 0)
            {
                throw new ArgumentException("CommandFSM: Enums must be marqued with System.Flags Attribute");
            }

            Transitions = new Dictionary<CommandTransition<K, T>, Func<K>>();
            
            CurrentCommand = (T)Enum.ToObject(typeof(T) , 1);
            CurrentState = (K)Enum.ToObject(typeof(K) , 1);
            LastState = CurrentState;
        }

        public void AddTransition(K CurrentEvent, T condition , Func<K> result)
        {
            CommandTransition<K, T> transition = new CommandTransition<K, T>(CurrentEvent, condition);
            if (!Transitions.ContainsKey(transition))
                Transitions.Add(transition, result);
            else
                throw new ArgumentException("CommandFSM: Transition already added");
        }

        /// <summary>
        /// Executes the fsm with the given command
        /// </summary>
        /// <returns>Returns true if the transition goes correctly otherwise false</returns>
        public bool MoveNext(T command)
        {
            CommandTransition<K, T> transition = new CommandTransition<K, T>(CurrentState, command);
            Func<K> state;
            if (Transitions.TryGetValue(transition, out state))
            {
                if (!CurrentState.Equals(state))
                {
                    LastState = CurrentState;
                    CurrentState = state.Invoke();

                    if (!CurrentCommand.Equals(command))
                    {
                        CurrentCommand = command;

                        if (OnCommandChanged != null)
                            OnCommandChanged.Invoke(CurrentCommand);
                    }
                    
                    if (OnStateChanged != null)
                        OnStateChanged.Invoke(CurrentState);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Executes the FSM using the current command
        /// </summary>
        /// <returns></returns>
        public bool Step()
        {
            return MoveNext(CurrentCommand);
        }
    }
}