using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Localization.WPF.Property;

namespace Localization.WPF.Extensions
{
    /// <summary>Retrieves a localized value from resources</summary>
    public class BindingLocalizedValue : LocalizedValue, IServiceProvider, IProvideValueTarget
    {
        private readonly string _ResourceKey;

        private readonly string _StringFormat;

        private readonly IEnumerable<BindingBase> _Bindings;

        /// <summary>Initializes a new instance of the <see cref="BindingLocalizedValue"/> class.</summary>
        /// <param name="property">The property.</param>
        /// <param name="key">The resource key.</param>
        /// <param name="format">The string format.</param>
        /// <param name="bindings">The bindings.</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        /// <exception cref="ArgumentException">The type of the <paramref name="property"/> is neither
        /// <see cref="string"/> nor <see cref="object"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> is null or empty
        /// AND <paramref name="format"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="bindings"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="bindings"/> is empty.</exception>
        public BindingLocalizedValue(LocalizedDependencyProperty property, string key, string format, ICollection<BindingBase> bindings)
            : base(property)
        {
            CheckPropertySupported(property);

            if(string.IsNullOrEmpty(key) && string.IsNullOrEmpty(format))
                throw new ArgumentException("Either a resource key of a format string must be specified.");

            if(bindings is null)
                throw new ArgumentNullException(nameof(bindings));

            if(bindings.Count == 0)
                throw new ArgumentException("At least one binding must be specified.", nameof(bindings));

            _ResourceKey = key;
            _StringFormat = format;
            _Bindings = bindings;
        }

        /// <summary>Retrieves the localized value from resources or by other means</summary>
        /// <returns>The localized value.</returns>
        protected override object GetLocalizedValue()
        {
            var obj = Property.Object;

            if(obj is null) return null;

            string format_string;
            if(string.IsNullOrEmpty(_ResourceKey))
                format_string = _StringFormat;
            else
            {
                var resource_manager = Property.GetResourceManager();

                format_string = resource_manager is null
                            ? GetFallbackValue()
                            : (resource_manager.GetString(_ResourceKey, Property.GetUICulture())
                            ?? GetFallbackValue());
            }

            var binding = new MultiBinding
            {
                StringFormat = format_string,
                Mode = BindingMode.OneWay,
                // The "MultiBinding" type internally uses the converter culture both 
                // with converters and format strings
                ConverterCulture = Property.GetCulture(),
            };

            foreach (var b in _Bindings)
                binding.Bindings.Add(b);

            return binding.ProvideValue(this);
        }

        /// <summary>Returns a value when a resource is not found</summary>
        /// <returns>"[ResourceKey]"</returns>
        private string GetFallbackValue() => $"[{_ResourceKey}]";

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="ServiceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="ServiceType"/>.
        /// -or-
        /// null if there is no service object of type <paramref name="ServiceType"/>.
        /// </returns>
        object IServiceProvider.GetService(Type ServiceType) => ServiceType == typeof(IProvideValueTarget) ? this : null;

        #endregion

        #region IProvideValueTarget Members

        /// <summary>
        /// Gets the target object being reported.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The target object being reported.
        /// </returns>
        object IProvideValueTarget.TargetObject => Property.Object;

        object IProvideValueTarget.TargetProperty => (DependencyProperty)Property.Property;

        #endregion

        #region Internal static methods

        /// <summary>
        /// Checks if binding localization can be used on the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>
        /// 	<c>true</c> binding localization can be used; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        internal static void CheckPropertySupported(LocalizedProperty property)
        {
            var type = (property ?? throw new ArgumentNullException(nameof(property))).GetValueType();
            if(type != typeof(string) && type != typeof(object)) 
                throw new InvalidOperationException("Only properties of type 'System.String' and 'System.Object' are supported.");
        }

        #endregion
    }
}
