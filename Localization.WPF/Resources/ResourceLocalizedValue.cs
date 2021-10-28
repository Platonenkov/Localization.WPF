using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Localization.WPF.Property;

namespace Localization.WPF.Resources
{
    /// <summary>Значение, получаемое из локализованных ресурсов</summary>
    public class ResourceLocalizedValue : LocalizedValue
    {
        /// <summary>Ключ ресурса значения</summary>
        private readonly string _ResourceKey;

        /// <summary>Инициализация нового <see cref="ResourceLocalizedValue"/></summary>
        /// <param name="Property">Свойство, локализация которого производится</param>
        /// <param name="ResourceKey">Ключ ресурсов значения свойства</param>
        /// <exception cref="ArgumentNullException"><paramref name="Property"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="ResourceKey"/> is null or empty.</exception>
        public ResourceLocalizedValue(LocalizedProperty Property, string ResourceKey)
            : base(Property)
        {
            if(string.IsNullOrEmpty(ResourceKey)) throw new ArgumentNullException(nameof(ResourceKey));

            _ResourceKey = ResourceKey;
        }

        /// <summary>Запрашивает локализованное значение из ресурсов, или из другого места</summary>
        /// <returns>Локализованное значение</returns>
        protected override object GetLocalizedValue() =>
            Property.GetResourceManager() is { } manager
                ? manager.GetObject(_ResourceKey, Property.GetUICulture()) is { } value
                    ? Property.Converter != null ? value : ChangeValueType(Property.GetValueType(), value)
                    : GetFallbackValue()
                : GetFallbackValue();

        /// <summary>Возвращает значение, когда ресурс не найден</summary>
        /// <returns>"[ResourceKey]" если свойство имеет тип <see cref="string"/>. Иначе, <c>null</c></returns>
        private object GetFallbackValue()
        {
            var type = Property.GetValueType();
            return type != typeof(string) && type != typeof(object) ? null : $"[{_ResourceKey}]";
        }

        /// <summary>Пытается привести тип загруженного ресурса к типу свойства</summary>
        /// <param name="type"></param>
        /// <param name="value">Объект, полученный из ресурсов</param>
        /// <returns>Объект приведённого типа</returns>
        /// <remarks>
        /// Поддерживает следующий набор типов: изображения, иконки, текст, перечисления, числа, булевские величины,
        /// <see cref="DateTime"/> и <see cref="TypeConverter"/>, если хотя бы один определён для типа свойства.
        /// </remarks>
        private object ChangeValueType(Type type, object value)
        {
            if(type is null) throw new ArgumentNullException(nameof(type));

            // Если тип переданного объекта совпадает с требуемым типом, то просто возвращаем его
            if(type == typeof(object) || value.GetType() == type || type.IsInstanceOfType(value)) return value;

            // Если требуемый тип является типом-изображения
            if(type == typeof(ImageSource))
            {
                BitmapSource result;

                // Если значение - растровая картинка
                switch (value)
                {
                    case Bitmap bmp:
                        using(bmp)
                            result = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        break;
                    case Icon ico:
                        using(ico)
                        using(var bmp = ico.ToBitmap())
                            result = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        break;
                    case byte[] bytes:
                        using(var stream = new MemoryStream(bytes, false))
                            result = BitmapFrame.Create(stream); //Создаём новое изображение по указанному массиву байт
                        break;
                    default:
                        return value;
                }

                result.Freeze();

                return result;
            }

            //Если требуемый тип является перечислением, а переданное значение - строка
            if(type.IsEnum && value is string str)
                return Enum.Parse(type, str); //Заставляем указанный тип разобрать переданную строку

            //Если переданное значение поддерживает интерфейс преобразования и требуемый тип - один из примитивных, либо время
            if(value is IConvertible && (type.IsPrimitive || type == typeof(DateTime)))
                return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

            var converter = TypeDescriptor.GetConverter(type);
            if(converter.GetType() != typeof(TypeConverter))
                return converter.ConvertFrom(null!, CultureInfo.InvariantCulture, value);
            // Если не найдено ни одного конвертера, либо если получен конвертер по умолчанию
            // (конвертер по умолчанию использован быть не может)

            if(!Property.IsInDesignMode) return value;
            // Visual Studio "падает" при загрузке некоторых конвертеров в режиме дизайнера
            if(type != typeof(System.Windows.Media.Brush)) return value;
            converter = new BrushConverter();
            return converter.ConvertFrom(null!, CultureInfo.InvariantCulture, value);
        }
    }
}
