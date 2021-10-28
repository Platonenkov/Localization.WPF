using System;
using Localization.WPF.Property;

namespace Localization.WPF.Commands
{
    /// <summary>Значение, локализуемое путём вызова метода</summary>
    public class MethodLocalizedValue : LocalizedValue
    {
        /// <summary>Метод локализации</summary>
        private readonly LocalizationCallback _Method;

        /// <summary>Параметр локализации</summary>
        private readonly object _Parameter;

        /// <summary>Инициализация нового экземпляра <see cref="MethodLocalizedValue"/></summary>
        /// <param name="property">Локализуемое свойство</param>
        /// <param name="method">Метод локализации</param>
        /// <param name="parameter">Параметр, передаваемый методу локализации</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is null.</exception>
        public MethodLocalizedValue(LocalizedProperty property, LocalizationCallback method, object parameter)
            : base(property)
        {
            _Method = method ?? throw new ArgumentNullException(nameof(method));

            _Parameter = parameter;
        }

        /// <summary>Возвращает локализованное значение из ресурсов, или из других средств</summary>
        /// <returns>Локализованное значение</returns>
        protected override object GetLocalizedValue() => _Method(Property.GetCulture(), Property.GetUICulture(), _Parameter);
    }
}
