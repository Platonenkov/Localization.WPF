using Localization.WPF.TestWPF.ViewModels;
using MathCore.Hosting.WPF;

namespace Localization.WPF.TestWPF
{
    public class ServiceLocator : ServiceLocatorHosted
    {
        public MainWindowViewModel MainModel => GetRequiredService<MainWindowViewModel>();
    }
}
