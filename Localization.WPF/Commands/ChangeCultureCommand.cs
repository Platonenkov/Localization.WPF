using System;
using System.Globalization;
using System.Windows.Input;
using Localization.WPF.Property;

namespace Localization.WPF.Commands
{
    /// <summary>Команда изменения культуры приложения</summary>
    public class ChangeCultureCommand : ICommand
    {
        /// <summary>Событие, происходящее когда меняется состояние возможности исполнения команды</summary>
        public event EventHandler CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }

        /// <summary>Проверка - можно ли выполнить команду (всегда возвращает истину)</summary>
        /// <param name="parameter">Параметр команды</param>
        /// <returns>Возвращает истину</returns>
        public bool CanExecute(object parameter) => true;

        /// <summary>Метод выполнения команды</summary>
        /// <param name="parameter">Параметр команды, содержащий строковое обозначение культуры</param>
        public void Execute(object parameter) =>
            LocalizationManager.ChangeCulture(parameter switch
            {
                null => CultureInfo.CurrentUICulture,
                string str => CultureInfo.GetCultureInfo(str),
                CultureInfo ci => ci,
                _ => throw new ArgumentException("Параметр должен указывать культуру")
            });
    }
}