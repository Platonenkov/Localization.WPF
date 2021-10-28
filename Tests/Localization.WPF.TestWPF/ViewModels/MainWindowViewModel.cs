using MathCore.Hosting;
using MathCore.WPF.ViewModels;

namespace Localization.WPF.TestWPF.ViewModels
{
    [Service]
    public class MainWindowViewModel : TitledViewModel
    {
        public MainWindowViewModel() => Title = "Главное окно";
    }
}
