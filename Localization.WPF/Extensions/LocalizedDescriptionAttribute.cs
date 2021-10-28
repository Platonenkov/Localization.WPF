using System;
using System.ComponentModel;
using System.Resources;

namespace Localization.WPF.Extensions
{
    /// <summary>
    /// Атрибут для расширения Description с возможностью локализации значения
    /// </summary>
    /// <example>
    /// <code>
    /// public enum Roles
    /// {
    ///     [LocalizedDescription("Administrator", typeof(Resource))]
    ///     Administrator,
    ///     ...
    /// }
    /// </code>
    /// </example>
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _ResourceKey;
        private readonly ResourceManager _Resource;
        public LocalizedDescriptionAttribute(string ResourceKey, Type ResourceType)
        {
            _Resource = new ResourceManager(ResourceType);
            _ResourceKey = ResourceKey;
        }

        public override string Description
        {
            get
            {
                var display_name = _Resource.GetString(_ResourceKey);

                return string.IsNullOrEmpty(display_name)
                    ? $"[[{_ResourceKey}]]"
                    : display_name;
            }
        }
    }
}
