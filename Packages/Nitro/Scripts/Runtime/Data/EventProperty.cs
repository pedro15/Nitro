namespace Nitro.Data
{
    [System.Serializable]
    public struct EventProperty<T>
    {
        [UnityEngine.SerializeField]
        private T m_value;

        public System.Action<T> OnChange;

        public void SetValue(T val)
        {
            m_value = val;
            if (OnChange != null) OnChange.Invoke(m_value);
        }

        public void SetValue(System.Func<T, T> func)
        {
            SetValue(func.Invoke(m_value));
        }

        public T GetValue() => m_value;

        public EventProperty(T value , System.Action<T> onchange = null) : this()
        {
            OnChange += onchange;
            SetValue(value);
        }

        public static implicit operator T(EventProperty<T> d)
        {
            return d.GetValue();
        }

        public override string ToString()
        {
            return m_value?.ToString();
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }
    }
}