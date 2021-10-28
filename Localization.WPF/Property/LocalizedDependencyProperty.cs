using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Localization.WPF.Property
{
    /// <summary>Локализуемое свойство-зависимости</summary>
    public class LocalizedDependencyProperty : LocalizedProperty
    {
        /// <summary>Инициализация нового экземпляра <see cref="LocalizedDependencyProperty"/></summary>
        /// <param name="obj">Объект-зависимости, содержащий локализуемое свойство-зависимости</param>
        /// <param name="property">Локализуемое свойство-зависимости</param>
        public LocalizedDependencyProperty(DependencyObject obj, DependencyProperty property) : base(obj, property) { }

        /// <inheritdoc />
        protected internal override object GetValue() =>
            Object is { } obj
                ? obj.CheckAccess()
                    ? obj.GetValue((DependencyProperty)Property)
                    : obj.Dispatcher.Invoke(new DispatcherOperationCallback(GetValue))
                : null;

        /// <summary>Получить значение</summary>
        /// <param name="parameter">Параметр метода получения значения</param>
        /// <returns>Значение свйоства</returns>
        private object GetValue(object parameter) => GetValue();

        /// <inheritdoc />
        protected internal override void SetValue(object value)
        {
            if(Object is not { } obj) return;
            if(obj.CheckAccess())
                obj.SetValue((DependencyProperty)Property, value);
            else
                obj.Dispatcher.Invoke(new SendOrPostCallback(SetValue), value);
        }

        /// <inheritdoc />
        protected internal override Type GetValueType() => ((DependencyProperty)Property).PropertyType;
    }
}
