using System;
using Localization.WPF.Property;

namespace Localization.WPF.Providers
{
    /// <summary>Форматирует список аргументов (объектов) для получения строки локализации </summary>
    public class FormattedLocalizedValue : LocalizedValue
    {
        /// <summary>Строка форматирования</summary>
        private readonly string _FormatString;

        /// <summary>Аргументы</summary>
        private readonly object[] _Args;

        /// <summary>Инициализация нового <see cref="FormattedLocalizedValue"/></summary>
        /// <param name="property">Свойство</param>
        /// <param name="format">Строка форматирования</param>
        /// <param name="args">Аргументы</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is null</exception>
        public FormattedLocalizedValue(LocalizedProperty property, string format, params object[] args)
            : base(property)
        {
            _FormatString = format ?? throw new ArgumentNullException(nameof(format));
            _Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        /// <summary>Запрашивает локализованное значение из ресурсов или из других источников</summary>
        /// <returns>Локализованное значение</returns>
        protected override object GetLocalizedValue() => string.Format(Property.GetCulture(), _FormatString, _Args);
    }
}
