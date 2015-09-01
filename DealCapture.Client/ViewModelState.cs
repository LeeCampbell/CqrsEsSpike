namespace DealCapture.Client
{
    public abstract class ViewModelState
    {
        public static ViewModelState Idle = new IdleViewModelState();
        public static ViewModelState Processing = new ProcessingViewModelState();
        public static ViewModelState Terminal = new TerminalViewModelState();

        

        public static ViewModelState Error(string errorMessage)
        {
            return new ErrorViewModelState(errorMessage);
        }
        
        private ViewModelState()
        {
        }

        public virtual bool IsProcessing { get { return false; } }
        public virtual bool IsTerminal { get { return false; } }
        public virtual bool HasError { get { return false; } }
        public virtual string ErrorMessage { get { return null; } }

        private sealed class IdleViewModelState : ViewModelState
        {
        }

        private sealed class ProcessingViewModelState : ViewModelState
        {
            public override bool IsProcessing { get { return true; } }
        }

        private sealed class TerminalViewModelState : ViewModelState
        {
            public override bool IsTerminal { get { return true; } }
        }

        private sealed class ErrorViewModelState : ViewModelState
        {
            private readonly string _errorMessage;

            public ErrorViewModelState(string errorMessage)
            {
                _errorMessage = errorMessage;
            }

            public override bool HasError { get { return true; } }
            public override string ErrorMessage { get { return _errorMessage; } }
        }
    }
}