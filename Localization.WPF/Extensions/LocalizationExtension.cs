using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Localization.WPF.Providers;

namespace Localization.WPF
{
    [ContentProperty(nameof(ArgumentBindings))]
    public class LocalizationExtension : MarkupExtension
    {
        private Collection<BindingBase> _Arguments;

        public LocalizationExtension() { }

        public LocalizationExtension(string key) => Key = key;

        /// <summary>Ключ локализованной строки</summary>
        public string Key { get; set; }

        /// <summary>Привязка для ключа локализованной строки</summary>
        public Binding KeyBinding { get; set; }

        /// <summary>Аргументы форматируемой локализованный строки</summary>
        public IEnumerable<object> Arguments { get; set; }

        /// <summary>Привязки аргументов форматируемой локализованный строки</summary>
        public Collection<BindingBase> ArgumentBindings
        {
            get => _Arguments ??= new Collection<BindingBase>();
            set => _Arguments = value;
        }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider sp)
        {
            if (Key != null && KeyBinding != null)
                throw new ArgumentException($"Нельзя одновременно задать {nameof(Key)} и {nameof(KeyBinding)}");
            if (Key is null && KeyBinding is null)
                throw new ArgumentException($"Необходимо задать {nameof(Key)} или {nameof(KeyBinding)}");
            if (Arguments != null && ArgumentBindings.Count > 0)
                throw new ArgumentException($"Нельзя одновременно задать {nameof(Arguments)} и {nameof(ArgumentBindings)}");

            var target = (IProvideValueTarget)sp.GetService(typeof(IProvideValueTarget));
            if (target?.TargetObject.GetType().FullName == "System.Windows.SharedDp")
                return this;

            // Если заданы привязка ключа или список привязок аргументов,
            // то используем BindingLocalizationListener
            if (KeyBinding != null || ArgumentBindings.Count > 0)
            {
                var listener = new BindingLocalizationListener();

                // Создаем привязку для слушателя
                var listener_binding = new Binding { Source = listener };

                var key_binding = KeyBinding ?? new Binding { Source = Key };

                var multi_binding = new MultiBinding
                {
                    Converter = new BindingLocalizationConverter(),
                    ConverterParameter = Arguments,
                    Bindings = { listener_binding, key_binding }
                };

                // Добавляем все переданные привязки аргументов
                foreach (var binding in ArgumentBindings)
                    multi_binding.Bindings.Add(binding);

                var value = multi_binding.ProvideValue(sp);
                // Сохраняем выражение привязки в слушателе
                listener.SetBinding(value as BindingExpressionBase);
                return value;
            }

            // Если задан ключ, то используем KeyLocalizationListener
            if (string.IsNullOrEmpty(Key)) return null;
            {
                var listener = new KeyLocalizationListener(Key, Arguments?.ToArray());

                switch (target?.TargetObject)
                {
                    // Если локализация навешана на DependencyProperty объекта DependencyObject или на Setter
                    case DependencyObject _ when target.TargetProperty is DependencyProperty:
                    case Setter _:
                    {
                        var binding = new Binding(nameof(KeyLocalizationListener.Value))
                        {
                            Source = listener,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        return binding.ProvideValue(sp);
                    }
                    // Если локализация навешана на Binding, то возвращаем слушателя
                    case Binding target_binding when target.TargetProperty != null && target.TargetProperty.GetType().FullName == "System.Reflection.RuntimePropertyInfo" && target.TargetProperty.ToString() == "System.Object Source":
                        target_binding.Path = new PropertyPath(nameof(KeyLocalizationListener.Value));
                        target_binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        return listener;
                    default:
                        // Иначе возвращаем локализованную строку
                        return listener.Value;
                }
            }
        }
    }
}