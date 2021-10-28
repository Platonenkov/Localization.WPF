using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Localization.WPF.Providers;

namespace Localization.WPF.Property
{
    /// <summary>Локализуемое свойство</summary>
    public abstract class LocalizedProperty
    {
        #region Открытые свойства

        /// <summary>Конвертер, используемый для преобразования запрашиваемого ресурса в значение свойства</summary>
        public IValueConverter Converter { get; set; }

        /// <summary>Параметр, передаваемый конвертеру</summary>
        public object ConverterParameter { get; set; }

        /// <summary>Строка форматирования</summary>
        public string FormatString { get; set; }

        #endregion

        #region Защищённые свойства

        /// <summary>Объект, которому принадлежит свойство</summary>
        protected internal DependencyObject Object => (DependencyObject)_Object.Target;

        /// <summary>Локализуемое свойство</summary>
        protected internal object Property { get; }

        #endregion

        #region Внутренние свйоства

        /// <summary>Объект ещё жив (не является клиентом сборщика мусора)</summary>
        internal bool IsAlive => _Object.IsAlive;

        /// <summary>Показывает, что объект находятся в режиме дизайнера</summary>
        internal bool IsInDesignMode => _Object.Target is DependencyObject obj && DesignerProperties.GetIsInDesignMode(obj);

        #endregion

        #region Поля

        /// <summary>Слабая ссылка на объект, свойство которого локализуется</summary>
        private readonly WeakReference _Object;

        /// <summary>Хэш-код</summary>
        private readonly int _HashCode;

        #endregion

        #region Конструктор

        /// <summary>Инициализация нового экземпляра <see cref="obj"/></summary>
        /// <param name="obj">Объект, со свойством которого будем работать</param>
        /// <param name="property">Свойство для локализации</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> == <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> == <see langword="null"/>.</exception>
        protected LocalizedProperty(DependencyObject obj, object property)
        {
            _Object = new WeakReference(obj ?? throw new ArgumentNullException(nameof(obj)));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            _HashCode = obj.GetHashCode() ^ property.GetHashCode();
        }

        #endregion

        #region Абстрактные методы

        /// <summary>Получить значение свойства</summary>
        /// <returns>Значение свойства</returns>
        protected internal abstract object GetValue();

        /// <summary>Установить значение свойства</summary>
        /// <param name="value">Значение, которое нужно установить для свойства</param>
        protected internal abstract void SetValue(object value);

        /// <summary>Получить <see cref="Type"/> значений свойства</summary>
        /// <returns><see cref="Type"/> значений свойства</returns>
        protected internal abstract Type GetValueType();

        #endregion

        #region Присоединённые свойства

        /// <summary>Получить <see cref="ResourceManager"/> установленный для объекта</summary>
        /// <returns>A <see cref="ResourceManager"/> или null если явно не указано значение для объекта</returns>
        public ResourceManager GetResourceManager() =>
            Object is { } obj
                ? (obj.CheckAccess()
                    ? LocalizationScope.GetResourceManager(obj)
                    : (ResourceManager)obj.Dispatcher.Invoke(new DispatcherOperationCallback(x => LocalizationScope.GetResourceManager((DependencyObject)x)), obj))
                ?? LocalizationManager.DefaultResourceManager
                : null;

        /// <summary>Получить <see cref="CultureInfo"/> для объекта</summary>
        /// <returns><see cref="CultureInfo"/> или null если явно не указано значение для объекта</returns>
        public CultureInfo GetCulture() =>
            Object is { } obj
                ? (obj.CheckAccess()
                    ? LocalizationScope.GetCulture(obj)
                    : (CultureInfo)obj.Dispatcher.Invoke(new DispatcherOperationCallback(x => LocalizationScope.GetCulture((DependencyObject)x)), obj))
                ?? obj.Dispatcher.Thread.CurrentCulture
                : null;

        /// <summary>Получить <see cref="CultureInfo"/> для объекта текущего пользовательского интерфейса</summary>
        /// <returns><see cref="CultureInfo"/> или <see langword="null"/> если явно не указано значение для объекта</returns>
        public CultureInfo GetUICulture() =>
            Object is { } obj
                ? (obj.CheckAccess()
                    ? LocalizationScope.GetUICulture(obj)
                    : (CultureInfo)obj.Dispatcher.Invoke(new DispatcherOperationCallback(x => LocalizationScope.GetUICulture((DependencyObject)x)), obj))
                ?? obj.Dispatcher.Thread.CurrentUICulture
                : null;

        #endregion

        #region Хеш-код и сравнение

        /// <inheritdoc />
        public override int GetHashCode() => _HashCode;

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is LocalizedDependencyProperty ldp && _HashCode == ldp._HashCode && (ReferenceEquals(this, obj)
                || Object is { } target_object && ReferenceEquals(target_object, ldp.Object) && ReferenceEquals(Property, ldp.Property));

        #endregion
    }
}
