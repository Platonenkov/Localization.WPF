using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;
using Localization.WPF.Extensions;
using Localization.WPF.Property;

namespace Localization.WPF
{
    /// <summary>
    /// Enables localization of data-bound dependency properties.
    /// </summary>
    [ContentProperty("Bindings")]
    [MarkupExtensionReturnType(typeof(object))]
    public class LocBindingExtension : MarkupExtension
    {
        /// <summary>The resource key to use to format the values.</summary>
        /// <remarks>
        /// If both <see cref="ResourceKey"/> and <see cref="StringFormat"/> is specified
        /// <see cref="ResourceKey"/> has a priority.
        /// </remarks>
        public string ResourceKey { get; set; }

        /// <summary>The string to use to format the values.</summary>
        /// <remarks>
        /// If both <see cref="ResourceKey"/> and <see cref="StringFormat"/> is specified
        /// <see cref="ResourceKey"/> has a priority.
        /// </remarks>
        public string StringFormat { get; set; }

        private Collection<BindingBase> _Bindings;

        /// <summary>The bindings to pass as arguments to the format string.</summary>
        public Collection<BindingBase> Bindings => _Bindings ??= new Collection<BindingBase>();

        /// <summary>Initializes a new instance of the <see cref="LocBindingExtension"/> class.</summary>
        public LocBindingExtension() { }

        /// <summary>Initializes a new instance of the <see cref="LocBindingExtension"/> class.</summary>
        /// <param name="ResourceKey">The resource key to use to obtain the localized value.</param>
        public LocBindingExtension(string ResourceKey) => this.ResourceKey = ResourceKey;

        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="sp">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider sp)
        {
            if (sp.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget service) return null;

            if (service.TargetObject is not DependencyObject obj)
                return service.TargetProperty is DependencyProperty or PropertyInfo
                    ? this // The extension is used in a template
                    : null;

            if (service.TargetProperty is not DependencyProperty dependency_property)
                throw new InvalidOperationException("This extension can be used only with dependency properties.");

            var property = new LocalizedDependencyProperty(obj, dependency_property);

            // Check if the property supports binding localization
            BindingLocalizedValue.CheckPropertySupported(property);

            // Either a resource key of a format string must be specified
            if (string.IsNullOrEmpty(ResourceKey) && string.IsNullOrEmpty(StringFormat))
                return null;

            if (_Bindings is null || _Bindings.Count == 0) // At least one binding must be specified
                return null;

            var localized_value =
                new BindingLocalizedValue(property, ResourceKey, StringFormat, _Bindings);

            LocalizationManager.InternalAddLocalizedValue(localized_value);

            if (!property.IsInDesignMode) return localized_value.GetValue();

            // At design time VS designer does not set the parent of any control
            // before its properties are set. For this reason the correct values
            // of inherited attached properties cannot be obtained.
            // Therefore, to display the correct localized value it must be updated
            // later ater the parrent of the control has been set.
            obj.Dispatcher.BeginInvoke(
                new SendOrPostCallback(x => ((LocalizedValue)x)!.UpdateValue()),
                DispatcherPriority.ApplicationIdle,
                localized_value
            );

            return localized_value.GetValue();

        }
    }
}
