using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace Localization.WPF.Providers
{
    public interface ILocalizationProvider
    {
        object Localize(string key);

        IEnumerable<CultureInfo> Cultures { get; }
    }

    //public class ResxLocalizationProvider : ILocalizationProvider
    //{
    //    private IEnumerable<CultureInfo> _Cultures;

    //    public object Localize(string key)
    //    {
    //        return Strings.ResourceManager.GetObject(key);
    //    }

    //    public IEnumerable<CultureInfo> Cultures => _Cultures ?? (_Cultures = new List<CultureInfo>
    //    {
    //        new CultureInfo("ru-RU"),
    //        new CultureInfo("en-US"),
    //    });
    //}

    public class LocalizationCultureManager
    {
        private LocalizationCultureManager() { }

        private static LocalizationCultureManager __LocalizationManager;

        public static LocalizationCultureManager Instance => __LocalizationManager ??= new LocalizationCultureManager();

        public event EventHandler CultureChanged;

        public CultureInfo CurrentCulture
        {
            get => Thread.CurrentThread.CurrentCulture;
            set
            {
                if (Equals(value, Thread.CurrentThread.CurrentUICulture))
                    return;
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                CultureInfo.DefaultThreadCurrentCulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;
                OnCultureChanged();
            }
        }

        public IEnumerable<CultureInfo> Cultures => LocalizationProvider?.Cultures ?? Enumerable.Empty<CultureInfo>();

        public ILocalizationProvider LocalizationProvider { get; set; }

        private void OnCultureChanged() => CultureChanged?.Invoke(this, EventArgs.Empty);

        public object Localize(string key) => string.IsNullOrEmpty(key) 
            ? "[NULL]" 
            : LocalizationProvider?.Localize(key) ?? $"[{key}]";
    }

    public class KeyLocalizationListener : INotifyPropertyChanged, IDisposable
    {
        public KeyLocalizationListener(string key, object[] args)
        {
            Key = key;
            Args = args;
            LocalizationCultureManager.Instance.CultureChanged += OnCultureChanged;
        }

        private string Key { get; }

        private object[] Args { get; }

        public object Value
        {
            get
            {
                var value = LocalizationCultureManager.Instance.Localize(Key);
                return value is string str_value && Args != null 
                    ? string.Format(str_value, Args) 
                    : value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Уведомляем привязку об изменении строки
        private void OnCultureChanged(object sender, EventArgs e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

        //~KeyLocalizationListener()
        //{
        //    ReleaseUnmanagedResources();
        //}

        public void Dispose()
        {
            LocalizationCultureManager.Instance.CultureChanged -= OnCultureChanged;

            //ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        //private void ReleaseUnmanagedResources()
        //{
        //}
    }

    public class BindingLocalizationListener : IDisposable
    {
        private BindingExpressionBase BindingExpression { get; set; }

        public BindingLocalizationListener() => LocalizationCultureManager.Instance.CultureChanged += OnCultureChanged;

        public void SetBinding(BindingExpressionBase expr) => BindingExpression = expr;

        private void OnCultureChanged(object sender, EventArgs e)
        {
            try
            {
                // Обновляем результат выражения привязки
                // При этом конвертер вызывается повторно уже для новой культуры
                BindingExpression?.UpdateTarget();
            }
            catch
            {
                // ignored
            }
        }

        //~BindingLocalizationListener()
        //{
        //    ReleaseUnmanagedResources();
        //}

        public void Dispose()
        {
            LocalizationCultureManager.Instance.CultureChanged -= OnCultureChanged;

            //ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        //private void ReleaseUnmanagedResources()
        //{
        //}
    }

    public class BindingLocalizationConverter : IMultiValueConverter
    {
        public object Convert(object[] v, Type t, object p, CultureInfo c)
        {
            if (v is null || v.Length < 2) return null;

            var key = System.Convert.ToString(v[1] ?? "");
            var value = LocalizationCultureManager.Instance.Localize(key);
            if (value is not string) return value;
            var args = (p as IEnumerable<object> ?? v.Skip(2)).ToArray();
            if (args.Length == 1 && args[0] is not string && args[0] is IEnumerable enumerable)
                args = enumerable.Cast<object>().ToArray();
            return args.Length > 0 ? string.Format(value.ToString()!, args) : value;
        }

        public object[] ConvertBack(object v, Type[] t, object p, CultureInfo c) => throw new NotSupportedException();
    }
}