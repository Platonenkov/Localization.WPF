namespace Localization.WPF.Commands
{
    /// <summary>Оболочка для метода получения локализованного значения - <see cref="LocalizationCallback"/></summary>
    public class LocalizationCallbackReference
    {
        /// <summary>Метод получения локализованного значения <see cref="LocalizationCallback"/></summary>
        public event LocalizationCallback Callback;

        /// <summary>Получить метод <see cref="LocalizationCallback"/></summary>
        /// <returns>Ссылка на <see cref="LocalizationCallback"/></returns>
        internal LocalizationCallback GetCallback() => Callback;
    }
}
