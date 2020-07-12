using System;

namespace Nitro.FSM
{
    internal class CommandTransition<K, T> where K : struct,
        IComparable, IFormattable, IConvertible where T : struct, IComparable, IFormattable, IConvertible
    {
        private readonly K State;
        private readonly T Condition;

        public CommandTransition(K State, T Condition)
        {
            if (!State.GetType().IsEnum || !Condition.GetType().IsEnum)
            {
                throw new ArgumentException("CommandTransition: Only Enums are supported");
            }
            else if (typeof(K).GetCustomAttributes(typeof(FlagsAttribute), false).Length <= 0 ||
               typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Length <= 0)
            {
                throw new ArgumentException("CommandTransition: Enums must have Flags");
            }

            this.State = State;
            this.Condition = Condition;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            CommandTransition<K, T> other = (CommandTransition<K, T>)obj;

            if (other == null) return false;

            int cond_ = (Convert.ToInt32(Condition) & Convert.ToInt32(other.Condition));
            int state_ = (Convert.ToInt32(State) & Convert.ToInt32(other.State));

            return cond_ != 0 & state_ != 0;
        }

        public override string ToString()
        {
            return string.Format("StateFSM:: State: {0} Command: {1}", Convert.ToInt32(State), Convert.ToInt32(Condition));
        }

    }
}