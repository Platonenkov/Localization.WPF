using System.Globalization;

namespace Localization.WPF.Commands
{
    /// <summary>Метод, возвращающий локализованное значение с учётом культуры интерфейса пользователя и культуры форматирования</summary>
    /// <param name="culture">Культура форматирования свойства, значение которого локализуется</param>
    /// <param name="UICulture">Культура интерфейса пользователя</param>
    /// <returns>Локализованное значение</returns>
    public delegate object LocalizationCallback(CultureInfo culture, CultureInfo UICulture, object parameter);
}
