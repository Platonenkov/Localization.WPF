using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using Localization.WPF.Commands;
using Localization.WPF.Extensions;
using Localization.WPF.Property;
using Localization.WPF.Providers;
using Localization.WPF.Resources;

namespace Localization.WPF
{
    /// <summary>Менеджер локализации</summary>
    /// <remarks>
    /// Этот класс потоко-безопасный. ДА, ВЫ МОЖЕТЕ устанавливать локализованные ресурсы из потока, отличного от потока UI! 
    /// Несколько потоков UI также поддерживается.
    /// </remarks>
    public static class LocalizationManager
    {
        #region Команды

        /// <summary>Команда смены культуры</summary>
        public static ChangeCultureCommand ChangeCultureCommand { get; } = new();

        #endregion

        #region Константы

        /// <summary>Порог очистки невостребованных значений пула локализованных ресурсов</summary>
        private const int PurgeLimit = 100;

        #endregion

        #region Свойства

        /// <summary>Объект межпотоковой синхронизации доступа к менеджеру ресурсов</summary>
        private static readonly object __DefaultResourceManagerSyncRoot = new();

        /// <summary>Менеджер ресурсов по умолчанию</summary>
        private static ResourceManager __DefaultResourceManager;

        /// <summary>Менеджер ресурсов по умолчанию создан</summary>
        private static bool __DefaultResourceManagerSet;

        /// <summary> Используемый по умолчанию менеджер ресурсов</summary>
        public static ResourceManager DefaultResourceManager
        {
            get
            {
                if (__DefaultResourceManagerSet)
                    return __DefaultResourceManager;
                lock (__DefaultResourceManagerSyncRoot)
                {
                    if (__DefaultResourceManagerSet) return __DefaultResourceManager;
                    __DefaultResourceManager = GetDefaultResourceManager();

                    __DefaultResourceManagerSet = true;
                    return __DefaultResourceManager;
                }
            }
            set
            {
                lock (__DefaultResourceManagerSyncRoot)
                {
                    __DefaultResourceManager = value;
                    __DefaultResourceManagerSet = true;
                }
            }
        }

        #endregion

        #region События

        /// <summary>Событие возникает при обновлении значений// </summary>
        public static event EventHandler ValuesUpdated;

        /// <summary>Генерация события изменения значений</summary>
        private static void OnValuesUpdated() => ValuesUpdated?.Invoke(null, EventArgs.Empty);

        public class CultureChangedEventArgs : EventArgs
        {
            public CultureInfo NewCulture { get; set; }
            public CultureChangedEventArgs(CultureInfo NewCulture) => this.NewCulture = NewCulture;
        }

        public static event EventHandler<CultureChangedEventArgs> CultureChanging;
        private static void OnCultureChanging(ref CultureInfo NewCulture)
        {
            var handlers = CultureChanging;
            if (handlers is null) return;
            var arg = new CultureChangedEventArgs(NewCulture);
            handlers(null, arg);
            if (Equals(arg.NewCulture, NewCulture)) 
                return;
            NewCulture = arg.NewCulture;
        }

        public static event EventHandler<CultureChangedEventArgs> CultureChanged;
        private static void OnCultureChanged(CultureInfo NewCulture) => CultureChanged?.Invoke(null, new CultureChangedEventArgs(NewCulture));

        #endregion


        #region Методы интерфейса класса

        /// <summary>Обновить все локализованные значения</summary>
        /// <remarks>Этот метод должен быть вызван когда культура потока интерфейса пользователя меняется</remarks>
        public static void UpdateValues()
        {
            var v = __LocalizedValues;

            // Блокируем объект что бы предотвратить добавление новых значений из других потоков
            lock (v) 
                v.Values.Foreach(i => i.UpdateValue());
            OnValuesUpdated();
        }

        #endregion

        #region Локализованные значения

        /// <summary>Словарь локализованных значений</summary>
        private static readonly Dictionary<LocalizedProperty, LocalizedValue> __LocalizedValues = new();

        /// <summary>Количество локализованных значений, добавленных в словарь после последней очистки</summary>
        private static int __LocalizedValuesPurgeCount;

        /// <summary>Добавление нового локализованного значения</summary>
        /// <param name="value">Локализованное значение, добавляемое в словарь</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> == <c>null</c>.</exception>
        public static void AddLocalizedValue(LocalizedValue value)
        {
            InternalAddLocalizedValue(value);

            // Обновление состояния добавленного значения
            value.UpdateValue();
        }

        /// <summary>Добавление нового локализованного значения</summary>
        /// <param name="value">Локализованное значение, добавляемое в словарь</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> == <c>null</c>.</exception>
        internal static void InternalAddLocalizedValue(LocalizedValue value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            if (__LocalizedValuesPurgeCount > PurgeLimit)
            {
                // Удаление локализованных значений владельцы которых удалены сборщиком мусора
                lock (__LocalizedValues)
                {
                    var keys_to_remove = __LocalizedValues.Keys.Where(i => !i.IsAlive).ToArray();
                    if (keys_to_remove.Length > 0)
                        keys_to_remove.Foreach(k => __LocalizedValues.Remove(k));
                    __LocalizedValuesPurgeCount = 0;
                }
            }

            lock (__LocalizedValues)
            {
                var count = __LocalizedValues.Count;

                __LocalizedValues[value.Property] = value;

                if (count < __LocalizedValues.Count) // Если новое значение было добавлено в словарь
                    __LocalizedValuesPurgeCount++;  // то увеличиваем количество локализованных значений
            }
        }

        /// <summary>Удаление всех локализованных значений, ассоциированных с указанным свойством</summary>
        /// <param name="property">Очищаемое свойство</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> == <c>null</c>.</exception>
        /// <remarks>
        /// Этот метод удаляет свойство из списка свойств, которые обновляются автоматически при смене культуры. 
        /// Текущее значение свойства остаётся неизменным.
        /// </remarks>
        public static void RemoveLocalizedValue(LocalizedProperty property)
        {
            if (property is null) throw new ArgumentNullException(nameof(property));

            lock (__LocalizedValues) 
                __LocalizedValues.Remove(property);
        }

        #endregion

        #region Ресурсы

        /// <summary>Список менеджеров ресурсов</summary>
        private static readonly List<ResourceManagerData> __ResourceManagers = new();

        /// <summary>Загрузка менеджера ресурсов для указанного типа</summary>
        /// <param name="type">Тип, для которого требуется произвести загрузку менеджера ресурсов</param>
        /// <returns>Загруженный менеджер ресурсов</returns>
        internal static ResourceManager LoadResourceManager(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));

            lock (__ResourceManagers)
            {
                if (__ResourceManagers.Find(x => x.Type == type) is { } result) 
                    return result.Manager;
                __ResourceManagers.Add(result = new ResourceManagerData
                {
                    Type = type, 
                    Manager = new ResourceManager(type)
                });
                return result.Manager;
            }
        }

        /// <summary>Загрузить менеджер ресурсов</summary>
        /// <param name="AssemblyName">Имя сборки</param>
        /// <param name="BaseName">Корень пространства имён ресурсов</param>
        /// <returns>Менеджер ресурсов сборки</returns>
        internal static ResourceManager LoadResourceManager(string AssemblyName, string BaseName)
        {
            if (string.IsNullOrEmpty(AssemblyName)) throw new ArgumentNullException(nameof(AssemblyName));
            if (string.IsNullOrEmpty(BaseName)) throw new ArgumentNullException(nameof(BaseName));

            var assembly = AppDomain.CurrentDomain.Load(AssemblyName);

            lock (__ResourceManagers)
            {
                if (__ResourceManagers.Find(x => Equals(x.Assembly, assembly) && string.Equals(x.BaseName, BaseName, StringComparison.CurrentCultureIgnoreCase)) is { } result) 
                    return result.Manager;
                result = new ResourceManagerData
                {
                    Assembly = assembly,
                    BaseName = BaseName, 
                    Manager = new ResourceManager(BaseName, assembly)
                };
                __ResourceManagers.Add(result);
                return result.Manager;
            }
        }

        /// <summary>Метод отброса некорректных сборок</summary>
        /// <param name="assembly">Проверяемая сборка</param>
        /// <returns>Истина, если сборка не является системной и не принадлежит Microsoft</returns>
        private static bool FilterAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            // В имени сборки не должно быть упомянаний Microsoft, Interop, Blend
            if (name is { Length: > 0 } && (name.Contains("Microsoft") || name.Contains("Interop") || name.Equals("Blend"))) 
                return false;
            var attrib = (AssemblyCompanyAttribute)assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).FirstOrDefault();
            // У сборки может либо не быть атрибутов, либо атрибут компании, создавшей сборку, не должен иметь значение Microsoft
            return attrib is null || !attrib.Company.Contains("Microsoft");
        }

        /// <summary>Пытаемся найти нужный <see cref="ResourceManager"/> для использования по умолчанию</summary>
        /// <returns><see cref="ResourceManager"/> или <c>null</c> если <see cref="ResourceManager"/> не был найден</returns>
        private static ResourceManager GetDefaultResourceManager()
        {
            // Сборка, в которой мы будем искать ресурсы
            Assembly assembly = null;

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Если мы вдруг оказались в режиме дизайнера - в Visual Studio
                // Пытаемся найти главную сборку приложения
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Тут будет сборка с ресурсами - библиотека (*.dll)
                Assembly library_assembly = null;

                foreach (var asm in assemblies.Where(FilterAssembly)
                    .Select(assembly_info =>
                    {
                        try // Пытаемся получить тип, содержащийся в сборке, который заканчивается на .Properties.Resources
                        {
                            return new
                            {
                                Assembly = assembly_info,
                                ResourceType = assembly_info.GetTypes().FirstOrDefault(t => t.FullName?.EndsWith(".Properties.Resources") ?? false)
                            };
                        }
                        catch (ReflectionTypeLoadException) // Если у нас что-то пошло не так, то это плохая сборка. Мы с ней не играем
                        {
                            return null;
                        }
                    })
                    .Where(assembly_info => assembly_info?.ResourceType != null) // Отбрасываем все сборки, в которых нет типа с ресурсами
                    .Select(assembly_info => assembly_info.Assembly))
                {
                    // Сборка содержит ресурсы "по умолчанию"

                    if (asm.EntryPoint != null)
                    {
                        // Проверка на то, что сборка содержит WPF-приложение (т.к. MyApplication.App класс
                        // наследуется от System.Windows.Application)

                        var app_type = asm.GetTypes().FirstOrDefault(t => t.FullName?.EndsWith(".App") ?? false);
                        if (app_type is null || !typeof(Application).IsAssignableFrom(app_type)) continue;
                        // Сборка корректная
                        assembly = asm;

                        break;
                    }

                    library_assembly ??= asm;
                }

                // Проект должен быть проектом-библиотекой, так что используем первую сборку, что содержит ресурсы "по умолчанию"
                // и не принадлежит Microsoft
                assembly ??= library_assembly;
            }
            else
            {
                // Сборка текущего WPF application
                assembly = Application.Current != null && Application.Current.GetType() != typeof(Application)
                    ? Application.Current.GetType().Assembly
                    : Assembly.GetEntryAssembly();
            }

            if (assembly is null) return null;
            try
            {
                // Неверно определялось имя класса ресурсов для сборок с составным именем
                var resources_type = assembly.GetTypes().FirstOrDefault(t => t.FullName?.EndsWith(".Resources") ?? false);
                return resources_type?.FullName is null
                    ? null
                    : new ResourceManager(resources_type.FullName, assembly);
            }
            catch (MissingManifestResourceException)
            {
                // Ресурсы не могут быть найдены в манифесте сборки
            }

            return null;
        }

        #endregion

        #region Данные для менеджера ресурсов

        /// <summary>Данные менеджера ресурсов</summary>
        private class ResourceManagerData
        {
            /// <summary>Тип</summary>
            public Type Type { get; set; }

            /// <summary>Сборка</summary>
            public Assembly Assembly { get; set; }

            /// <summary>Базовое имя</summary>
            public string BaseName { get; set; }

            /// <summary>Менеджер ресурсов</summary>
            public ResourceManager Manager { get; set; }
        }

        #endregion

        #region Методы

        #region Свойства

        public static CultureInfo CurrentCulture => Thread.CurrentThread.CurrentUICulture;

        /// <summary>Возвращает объект, который может быть использован для локализации определённого свойства, не являющегося свойством-зависимости</summary>
        /// <param name="obj">Объект, обладающий локализуемым свойством</param>
        /// <param name="property">Описание свойства для локализации</param>
        /// <returns>Локализованное свойство</returns>
        public static LocalizedProperty Property(this DependencyObject obj, DependencyProperty property) =>
            new LocalizedDependencyProperty(obj, property);

        /// <summary>Возвращает объект, который может быть использован для локализации определённого свойства, не являющегося свойством-зависимости</summary>
        /// <param name="obj">Объект, обладающий локализуемым свойством</param>
        /// <param name="property">Описание свойства для локализации</param>
        /// <returns>Локализованное свойство</returns>
        public static LocalizedProperty Property(this DependencyObject obj, PropertyInfo property) => new LocalizedNonDependencyProperty(obj, property);

        /// <summary>Возвращает объект, который может быть использован для локализации определённого свойства, не являющегося свойством-зависимости</summary>
        /// <param name="obj">Объект, обладающий локализуемым свойством</param>
        /// <param name="propertyName">Имя свойства для локализации</param>
        /// <returns>Локализованное свойство</returns>
        public static LocalizedProperty Property(this DependencyObject obj, string propertyName)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            var property = obj.GetType().GetProperty(propertyName);

            if (property is null)
                throw new ArgumentOutOfRangeException(nameof(propertyName), null, $"Property not found in type '{obj.GetType()}'.");

            return new LocalizedNonDependencyProperty(obj, property);
        }

        #endregion

        #region Значения

        /// <summary>Присвоение значение ресурса для определённого локализуемого свойства</summary>
        /// <param name="property">Локализуемое свойство</param>
        /// <param name="resourceKey">Ключ ресурса</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="resourceKey"/> is null или пуст</exception>
        public static void SetResourceValue(this LocalizedProperty property, string resourceKey) =>
            AddLocalizedValue(new ResourceLocalizedValue(property, resourceKey));

        /// <summary>Присвоение форматированного значения для определённого свойства</summary>
        /// <param name="property">Локализуемое свойство</param>
        /// <param name="formatString">Строка форматирования</param>
        /// <param name="args">Аргументы</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="formatString"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
        public static void SetFormattedValue(this LocalizedProperty property, string formatString, params object[] args) =>
            AddLocalizedValue(new FormattedLocalizedValue(property, formatString, args));

        /// <summary>Присвоение форматированного значения для определённого свойства. Строка форматирования содержится в ресурсах.</summary>
        /// <param name="property">Локализуемое свойство</param>
        /// <param name="resourceKey">Ключ ресурса</param>
        /// <param name="args">Аргументы</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="resourceKey"/> is null или пусто</exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
        public static void SetResourceFormattedValue(this LocalizedProperty property, string resourceKey, params object[] args) =>
            AddLocalizedValue(new ResourceFormattedLocalizedValue(property, resourceKey, args));

        /// <summary>Присвоение значения, осуществляемое путём обратного вызова метода локализации</summary>
        /// <param name="property">Локализуемое свойство</param>
        /// <param name="method">Метод локализации</param>
        /// <param name="parameter">Параметр, передаваемый в метод локализации</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is null.</exception>
        /// <remarks>Этот метод будет вызван один раз для установки начального значения свойства, а также каждый раз при смене культуры</remarks>
        public static void SetCallbackValue(this LocalizedProperty property, LocalizationCallback method, object parameter) =>
            AddLocalizedValue(new MethodLocalizedValue(property, method, parameter));

        /// <summary>Прекратить работать со свойством. Не очищает значения свойства.</summary>
        /// <param name="property">Свойство</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        public static void Clear(this LocalizedProperty property) => RemoveLocalizedValue(property);

        #endregion

        public static void ChangeCulture(CultureInfo NewCulture)
        {
            var thread = Thread.CurrentThread;
            if (Equals(NewCulture, thread.CurrentUICulture)) return;

            OnCultureChanging(ref NewCulture);
            if (Equals(NewCulture, thread.CurrentUICulture)) return;
            thread.CurrentCulture = NewCulture;
            thread.CurrentUICulture = NewCulture;
            CultureInfo.DefaultThreadCurrentCulture = NewCulture;
            CultureInfo.DefaultThreadCurrentUICulture = NewCulture;
            UpdateValues();
            OnCultureChanged(NewCulture);
        }

        public static void ChangeCulture(string CultureName) => ChangeCulture(CultureInfo.GetCultureInfo(CultureName));
        #endregion
    }
}
