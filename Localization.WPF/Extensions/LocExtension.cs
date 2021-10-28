using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;
using Localization.WPF.Commands;
using Localization.WPF.Property;
using Localization.WPF.Resources;

namespace Localization.WPF
{
    /// <summary>Локализатор свойств WPF-объектов</summary>
    [ContentProperty("ResourceKey")]
    [MarkupExtensionReturnType(typeof(object))]
    public class LocExtension : MarkupExtension
    {
        /// <summary>Ключ словаря ресурсов локализуемого значения</summary>
        /// <remarks>Если оба <see cref="ResourceKey"/> и <see cref="Callback"/> определены, последний имеет высший приоритет</remarks>
        public string ResourceKey { get; set; }

        /// <summary>Метод, используемый для получения локализованного значения</summary>
        /// <remarks>
        /// <para>Если оба <see cref="ResourceKey"/> и <see cref="Callback"/> определены, последний имеет высший приоритет.</para>
        /// <para>Если <see cref="Converter"/> определён, он применяется после того, как метод <see cref="Callback"/> вернёт значение.</para>
        /// </remarks>
        public LocalizationCallbackReference Callback { get; set; }

        /// <summary>Параметр, передаваемый методу <see cref="Callback"/></summary>
        public object CallbackParameter { get; set; }

        /// <summary>Конвертер, используемый для преобразования значения до того, как оно будет присвоено свойству</summary>
        public IValueConverter Converter { get; set; }

        /// <summary>Параметр, передаваемй конвертеру</summary>
        public object ConverterParameter { get; set; }

        /// <summary>Строка форматирования</summary>
        public string FormatString { get; set; }

        /// <summary>Инициализация нового экземпляра <see cref="LocExtension"/></summary>
        public LocExtension() { }

        /// <summary>Инициализация нового экземпляра <see cref="LocExtension"/></summary>
        /// <param name="key">Ключ словаря ресурсов, применяемый для извлечения локализованного значения</param>
        public LocExtension(string key) => ResourceKey = key;

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider sp)
        {
            if (sp.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget service) return null;

            if (service.TargetObject is not DependencyObject obj)
                return service.TargetProperty is DependencyProperty or PropertyInfo ? this : null;

            LocalizedProperty property;
            switch (service.TargetProperty)
            {
                case DependencyProperty dp: property = new LocalizedDependencyProperty(obj, dp); break;
                case PropertyInfo pi: property = new LocalizedNonDependencyProperty(obj, pi); break;
                default: return null;
            }

            property.Converter = Converter;
            property.ConverterParameter = ConverterParameter;
            property.FormatString = FormatString;

            var localized_value = CreateLocalizedValue(property);
            if (localized_value is null) return null;

            LocalizationManager.InternalAddLocalizedValue(localized_value);

            if (!property.IsInDesignMode) return localized_value.GetValue();

            // В режиме дизайнера - дизайнер VS не устанавливает предка какого-либо элемента управления до того, 
            // как его свойства получат значения. В этом случае корректные значения унаследованных приложенных 
            // свойств не могут быть получены. Тем не менее, для отображения корректных локализованных значений 
            // это значение должно быть обновлено позже, после того как предки элемента управления буду установлены.
            obj.Dispatcher.BeginInvoke
            (
                new SendOrPostCallback(x => ((LocalizedValue)x)!.UpdateValue()),
                DispatcherPriority.ApplicationIdle,
                localized_value
            );

            return localized_value.GetValue();
        }

        /// <summary>Создать локализованное значение</summary>
        /// <param name="property">Локализуемое свойство</param>
        /// <returns>Локализованное значение</returns>
        private LocalizedValue CreateLocalizedValue(LocalizedProperty property) =>
            Callback?.GetCallback() is { } callback
                ? new MethodLocalizedValue(property, callback, CallbackParameter)
                : ResourceKey is { Length: > 0 } key ? new ResourceLocalizedValue(property, key) : null;
    }
}
