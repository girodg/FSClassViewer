using Prism.Commands;
using System;

namespace FSClassViewer
{
    public class AppCommand<T> : DelegateCommand<T>
    {
        public AppCommand(Action<T> actionMethod) : base(actionMethod)
        {
        }
        public AppCommand(Action<T> actionMethod, Func<T, bool> canExecuteMethod) : base(actionMethod, canExecuteMethod)
        {
        }
    }
}
