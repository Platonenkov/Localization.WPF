using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Localization.WPF.Property
{
    /// <summary>Локализуемое свойство, не являющееся свойством-зависимости</summary>
    public class LocalizedNonDependencyProperty : LocalizedProperty
    {
        /// <summary>Инициализация нового <see cref="LocalizedNonDependencyProperty"/></summary>
        /// <param name="obj">Объект, содержащий свойство</param>
        /// <param name="property">Описание свойства</param>
        public LocalizedNonDependencyProperty(DependencyObject obj, PropertyInfo property) : base(obj, property) { }

        /// <summary>ОПределение значения свйоства</summary>
        /// <returns>Возвращает значение свйоства</returns>
        protected internal override object GetValue() => Object is { } obj ? ((PropertyInfo)Property).GetValue(obj, null) : null;

        /// <summary>Установка значения свойства</summary>
        /// <param name="value">Устанавливаемое значение свойства</param>
        protected internal override void SetValue(object value)
        {
            if(Object is not { } obj) return;
            if(obj.CheckAccess())
                ((PropertyInfo)Property).SetValue(obj, value, null);
            else
                obj.Dispatcher.Invoke(new SendOrPostCallback(SetValue), value);
        }

        /// <summary>Извлечение типа значения свйоства</summary>
        /// <returns>Тип значения свойства</returns>
        protected internal override Type GetValueType() => ((PropertyInfo)Property).PropertyType;
    }
}
