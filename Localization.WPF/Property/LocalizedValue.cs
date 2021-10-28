using System;
using System.Diagnostics;

namespace Localization.WPF.Property
{
    /// <summary>Локализуемое значение</summary>
    public abstract class LocalizedValue
    {
        /// <summary>Локализуемое свойство</summary>
        public LocalizedProperty Property { get; }

        /// <summary>Инициализация нового экземпляра <see cref="LocalizedValue"/></summary>
        /// <param name="property">Локализуемое свойство</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        protected LocalizedValue(LocalizedProperty property) => Property = property ?? throw new ArgumentNullException(nameof(property));

        /// <summary>Получает локализованное значение из ресурсов, или из другого места</summary>
        /// <returns>Локализованное значение</returns>
        protected abstract object GetLocalizedValue();

        /// <summary>Обновляет локализованное значение свойства</summary>
        protected internal void UpdateValue()
        {
            try
            {
                Property.SetValue(GetValue());
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>Возвращает локализованное значение</summary>
        /// <returns>Локализованное значение</returns>
        internal object GetValue()
        {
            try
            {
                var value = GetLocalizedValue();
                return Property.Converter is { } converter
                    ? converter.Convert(value, Property.GetValueType(), Property.ConverterParameter, Property.GetCulture())
                    : value;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Localization Error: {e.Message}");
                return null;
            }
        }
    }
}
