using System;
using System.Resources;
using System.Windows.Markup;
using Localization.WPF.Property;

namespace Localization.WPF.Resources
{
    /// <summary>Расширение разметки XAML, позволяющее создать менеджер ресурсов</summary>
    [MarkupExtensionReturnType(typeof(ResourceManager))]
    public class ResourceManagerExtension : MarkupExtension
    {
        /// <summary>Тип менеджера ресурсов <see cref="ResourceManager"/></summary>
        /// <remarks>
        /// Either <see cref="Type"/> or <see cref="AssemblyName"/> and <see cref="BaseName"/>
        /// must be specified. Depending on the specified properties the corresponding constructor
        /// of the <see cref="ResourceManager"/> class is called. If both kind of values are specified
        /// <see cref="Type"/> is used.
        /// </remarks>
        public Type Type { get; set; }

        /// <summary>Имя сборки, содержащей ресурсы</summary>
        /// <remarks>
        /// Either <see cref="Type"/> or <see cref="AssemblyName"/> and <see cref="BaseName"/>
        /// must be specified. Depending on the specified properties the corresponding constructor
        /// of the <see cref="ResourceManager"/> class is called. If both kind of values are specified
        /// <see cref="Type"/> is used.
        /// </remarks>
        public string AssemblyName { get; set; }

        /// <summary>Корень пространства имён ресурсов</summary>
        /// <remarks>
        /// Either <see cref="Type"/> or <see cref="AssemblyName"/> and <see cref="BaseName"/>
        /// must be specified. Depending on the specified properties the corresponding constructor
        /// of the <see cref="ResourceManager"/> class is called. If both kind of values are specified
        /// <see cref="Type"/> is used.
        /// </remarks>
        public string BaseName { get; set; }

        /// <summary>Менеджер ресурсов</summary>
        private ResourceManager _Manager;

        /// <summary>Инициализация нового <see cref="ResourceManagerExtension"/></summary>
        public ResourceManagerExtension() { }

        /// <summary>Инициализация нового <see cref="ResourceManagerExtension"/></summary>
        /// <param name="type">Тип данных</param>
        public ResourceManagerExtension(Type type) => Type = type;

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider) => _Manager 
            ??= Type is { } type
                ? LocalizationManager.LoadResourceManager(type)
                : AssemblyName is { Length: > 0 } assembly_name && BaseName is { Length: > 0 } base_name
                    ? LocalizationManager.LoadResourceManager(assembly_name, base_name)
                    : null;
    }
}
