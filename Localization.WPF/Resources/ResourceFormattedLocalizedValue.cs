using System;
using Localization.WPF.Property;
using Localization.WPF.Providers;

namespace Localization.WPF.Resources
{
    /// <summary>Форматирует список объектов для получения строки значения. Форматирование строки определяется из ресурсов.</summary>
    public class ResourceFormattedLocalizedValue : ResourceLocalizedValue
    {
        /// <summary>Форматируемые объекты</summary>
        private readonly object[] _Args;

        /// <summary>Инициализация нового <see cref="FormattedLocalizedValue"/></summary>
        /// <param name="property">Свойство</param>
        /// <param name="resourceKey">Ключ ресурсов</param>
        /// <param name="args">Аргументы</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="resourceKey"/> is null или пусто</exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is null</exception>
        public ResourceFormattedLocalizedValue(LocalizedProperty property, string resourceKey, params object[] args)
            : base(property, resourceKey) =>
            _Args = args ?? throw new ArgumentNullException(nameof(args));

        /// <summary>Возвращает локализованное значение из ресурсов, или из другого источника</summary>
        /// <returns>Локализованное значение</returns>
        protected override object GetLocalizedValue() =>
            base.GetLocalizedValue() is string result 
                ? string.Format(Property.GetCulture(), result, _Args) 
                : null;
    }
}
