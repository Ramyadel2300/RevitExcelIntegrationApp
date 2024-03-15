using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitExcelIntegrationApp.Command
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExeucte;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute, Func<object, bool> canExeucte = null)
        {
            _execute = execute;
            _canExeucte = canExeucte;
        }

        public void RaiseCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty); 

        public bool CanExecute(object parameter) => _canExeucte is null || _canExeucte(parameter);

        public void Execute(object parameter) => _execute(parameter);
    }
}
