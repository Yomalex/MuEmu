using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Util
{
    public abstract class StateMachine<T>
        where T : struct, IConvertible
    {
        protected T CurrentState { get; private set; }
        private T _nextState;
        private DateTimeOffset _nextStateIn;

        public TimeSpan TimeLeft => _nextStateIn - DateTimeOffset.Now;

        public abstract void Initialize();

        public virtual void Update()
        {
            if (!_nextState.Equals(CurrentState) && _nextStateIn < DateTimeOffset.Now)
            {
                var protectedState = _nextState;
                OnTransition(_nextState);
                CurrentState = protectedState;
            }
        }

        public abstract void OnTransition(T NextState);
        
        public void Trigger(T nextSate)
        {
            _nextState = nextSate;
            _nextStateIn = DateTimeOffset.Now;
        }

        public void Trigger(T nextSate, TimeSpan @in)
        {
            _nextState = nextSate;
            _nextStateIn = DateTimeOffset.Now.Add(@in);
        }
    }
}
