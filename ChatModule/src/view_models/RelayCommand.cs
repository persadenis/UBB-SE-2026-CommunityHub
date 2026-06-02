using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatModule.ViewModels
{
    /// <summary>
    /// A Command implementation that allows for Task-based execution and testing.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _running;

        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => !_running && (_canExecute?.Invoke() ?? true);

        /// <summary>
        /// Explicit async execution for Unit Testing.
        /// </summary>
        public async Task ExecuteAsync(object? parameter = null)
        {
            _running = true;
            RaiseCanExecuteChanged();
            try
            {
                await _execute();
            }
            finally
            {
                _running = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Standard ICommand execution for the UI.
        /// </summary>
        public async void Execute(object? parameter)
        {
            try
            {
                await ExecuteAsync(parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RelayCommand execution failed: {ex}");
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// A Generic Command implementation for parameters (e.g., CommandParameter).
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool>? _canExecute;
        private bool _running;

        public RelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            if (_running) 
                return false;
            if (_canExecute == null)
                return true;
            return parameter is T t && _canExecute(t);
        }

        public async Task ExecuteAsync(object? parameter)
        {
            if (parameter is T t)
            {
                _running = true;
                RaiseCanExecuteChanged();
                try
                {
                    await _execute(t);
                }
                finally
                {
                    _running = false;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public async void Execute(object? parameter)
        {
            try
            {
                await ExecuteAsync(parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RelayCommand<{typeof(T).Name}> execution failed: {ex}");
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}