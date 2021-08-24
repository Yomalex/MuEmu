using MU.Resources;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Util
{
    public abstract class StateMachine<T>
        where T : struct, IConvertible
    {
        public T CurrentState { get; private set; }
        private T _nextState;
        private DateTimeOffset _nextStateIn;
        private DateTimeOffset _currentState;
        protected ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(StateMachine<T>));

        public TimeSpan TimeLeft => _nextStateIn - DateTimeOffset.Now;
        public TimeSpan Time => DateTimeOffset.Now - _currentState;

        public abstract void Initialize();

        public virtual void Update()
        {
            if (!_nextState.Equals(CurrentState) && _nextStateIn < DateTimeOffset.Now)
            {
                var protectedState = _nextState;
                _logger.Information(ServerMessages.GetMessage(Messages.Server_EventStateChange), CurrentState, _nextState);
                OnTransition(_nextState);
                CurrentState = protectedState;
            }
        }

        public abstract void OnTransition(T NextState);
        
        public void Trigger(T nextSate)
        {
            _nextState = nextSate;
            _nextStateIn = DateTimeOffset.Now;
            _currentState = DateTimeOffset.Now;
        }

        public void Trigger(T nextSate, TimeSpan @in)
        {
            _nextState = nextSate;
            _currentState = DateTimeOffset.Now;
            _nextStateIn = DateTimeOffset.Now.Add(@in);
        }

        public void ChangeState(T nextSate)
        {
            if (!nextSate.Equals(CurrentState))
            {
                var protectedState = nextSate;
                OnTransition(_nextState);
                CurrentState = protectedState;
            }
        }
    }
}
