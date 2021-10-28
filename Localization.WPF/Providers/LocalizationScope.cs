using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Windows;

namespace Localization.WPF.Providers
{
    /// <summary>Область локализации</summary>
    public static class LocalizationScope
    {
        #region Attached properties

        /// <summary>Известные культуры</summary>
        private static readonly DependencyPropertyKey DefinedCulturesPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "DefinedCultures", 
                typeof(IEnumerable<CultureInfo>), 
                typeof(LocalizationScope), 
                new PropertyMetadata(GetCultures()));

        /// <summary>Известные культуры</summary>
        public static readonly DependencyProperty DefinedCulturesProperty = DefinedCulturesPropertyKey.DependencyProperty;


        //public static IEnumerable<CultureInfo> DefinedCultures
        //{
        //    get { }
        //    private set { }
        //}

        /// <summary>Установка значения свойства известных культур</summary>
        /// <param name="obj">ОБъект, устанавливающий значение свойства</param>
        /// <param name="value">Значение свойства</param>
        private static void SetDefinedCultures(DependencyObject obj, IEnumerable<CultureInfo> value) => obj.SetValue(DefinedCulturesPropertyKey, value);

        /// <summary>ОПределение перечня известных культур</summary>
        /// <param name="obj">Объект, для которого проводится определение</param>
        /// <returns>Перечень известных культур</returns>
        public static IEnumerable<CultureInfo> GetDefinedCultures(DependencyObject obj)
        {
            if(obj is null) throw new ArgumentNullException(nameof(obj));
            var cultures = (IEnumerable<CultureInfo>)obj.GetValue(DefinedCulturesProperty);
            if(cultures != null) return cultures;
            SetDefinedCultures(obj, cultures = GetCultures());
            return cultures;
        }

        /// <summary>Определение всех культур</summary>
        /// <returns></returns>
        private static IEnumerable<CultureInfo> GetCultures() => CultureInfo.GetCultures(CultureTypes.AllCultures);

        #region Culture

        /// <summary><see cref="CultureInfo"/> соответствующее форматируемому значению</summary>
        /// <remarks>
        /// ВНИМАНИЕ! Установка этого свойства не приведёт к автоматическому обновлению локализованных значений.
        /// Для этого требуется вызвать метод <see cref="LocalizationManager.UpdateValues"/>.
        /// </remarks>
        public static readonly DependencyProperty CultureProperty =
            DependencyProperty.RegisterAttached(
                "Culture", typeof(CultureInfo), 
                typeof(LocalizationScope),
                new FrameworkPropertyMetadata(null, 
                    FrameworkPropertyMetadataOptions.Inherits));

        public static CultureInfo GetCulture(DependencyObject obj) => (CultureInfo)obj.GetValue(CultureProperty);

        public static void SetCulture(DependencyObject obj, CultureInfo value) => obj.SetValue(CultureProperty, value);

        #endregion

        #region UICulture

        /// <summary>Информация о культуре <see cref="CultureInfo"/>, используемая для определения ресурсов из <see cref="ResourceManager"/></summary>
        /// <remarks>
        /// ВНИМАНИЕ! Установка этого свойства не приведёт к автоматическому обновлению локализованных значений.
        /// Для этого требуется вызвать метод <see cref="LocalizationManager.UpdateValues"/>.
        /// </remarks>
        public static readonly DependencyProperty UICultureProperty =
            DependencyProperty.RegisterAttached(
                "UICulture", 
                typeof(CultureInfo), 
                typeof(LocalizationScope),
                new FrameworkPropertyMetadata(null, 
                    FrameworkPropertyMetadataOptions.Inherits));

        public static CultureInfo GetUICulture(DependencyObject obj) => (CultureInfo)obj.GetValue(UICultureProperty);

        public static void SetUICulture(DependencyObject obj, CultureInfo value) => obj.SetValue(UICultureProperty, value);

        #endregion

        #region Менеджер ресурсов

        /// <summary>Менеджер ресурсов, используемый для получения локализованных значений</summary>
        /// <remarks>
        /// ВНИМАНИЕ! Установка этого свойства не приведёт к автоматическому обновлению локализованных значений.
        /// Для этого требуется вызвать метод <see cref="LocalizationManager.UpdateValues"/>.
        /// </remarks>
        public static readonly DependencyProperty ResourceManagerProperty =
            DependencyProperty.RegisterAttached(
                "ResourceManager", 
                typeof(ResourceManager), 
                typeof(LocalizationScope),
                new FrameworkPropertyMetadata(null, 
                    FrameworkPropertyMetadataOptions.Inherits));

        public static ResourceManager GetResourceManager(DependencyObject obj) => (ResourceManager)obj.GetValue(ResourceManagerProperty);

        public static void SetResourceManager(DependencyObject obj, ResourceManager value) => obj.SetValue(ResourceManagerProperty, value);

        #endregion

        #endregion
    }
}
