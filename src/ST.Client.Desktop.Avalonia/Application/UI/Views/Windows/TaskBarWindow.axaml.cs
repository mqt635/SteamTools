using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.Styling;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Threading.Tasks;

namespace System.Application.UI.Views.Windows
{
    public partial class TaskBarWindow : ReactiveWindow<TaskBarWindowViewModel>
    {
        private bool IsPointerOverSubMenu = false;

        public TaskBarWindow()
        {
            InitializeComponent();
            Topmost = true;

            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            //SystemDecorations = SystemDecorations.None;
            TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
            SizeToContent = Avalonia.Controls.SizeToContent.Height;
            CanResize = false;
            ShowInTaskbar = false;

            this.Opened += Window_Opened;
            this.LostFocus += Window_LostFocus;

            //var localAuthbtn = this.FindControl<Button>("LocalAuthMenu");
            //var userChangebtn = this.FindControl<Button>("UserChangeMenu");
            //if (userChangebtn != null)
            //{
            //    userChangebtn.PointerEnter += MenuButton_PointerEnter;
            //    //localbtn.PointerLeave += MenuButton_PointerLeave;
            //}
            //if (localAuthbtn != null)
            //{
            //    localAuthbtn.PointerEnter += MenuButton_PointerEnter;
            //    //localbtn.PointerLeave += MenuButton_PointerLeave;
            //}

#if DEBUG
            this.AttachDevTools();
#endif

            if (OperatingSystem2.IsWindows10AtLeast)
            {
                AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceNativeTitleBarToTheme(this);
            }
        }

        public void MenuButton_PointerLeave(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            //if (sender is Control c)
            //{
            //    //var flyout = FlyoutBase.GetAttachedFlyout(c);
            //    //flyout?.Hide();
            //    //IsPointerOverSubMenu = false;
            //}
        }

        public async void MenuButton_PointerEnter(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (sender is Control c && !IsPointerOverSubMenu)
            {
                if (c.Tag is TabItemViewModel tab && !tab.IsTaskBarSubMenu) return;

                var flyout = FlyoutBase.GetAttachedFlyout(c);
                await Task.Delay(500);
                if (flyout is not null && c.IsPointerOver && !IsPointerOverSubMenu)
                {
                    flyout.ShowAt(c);
                    IsPointerOverSubMenu = flyout.IsOpen;
                    flyout.Closed += (sender, e) => IsPointerOverSubMenu = flyout.IsOpen;
                    //c.Focus();
                }
            }
        }

        private void Window_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!this.IsPointerOver && !IsPointerOverSubMenu)
            {
                if (this.DataContext is TaskBarWindowViewModel vm)
                {
                    this.Hide();
                }
            }
        }

        private void Window_Opened(object? sender, EventArgs e)
        {
            if (this.DataContext is TaskBarWindowViewModel vm)
            {
                //if (vm.SizePosition.X > 0 && vm.SizePosition.Y > 0)
                //{
                //    this.Position = new PixelPoint(vm.SizePosition.X, vm.SizePosition.Y - (int)this.Height);
                //}

                vm.WhenAnyValue(x => x.SizePosition.X, x => x.SizePosition.Y)
                    .Subscribe(x =>
                    {
                        var screen = this.Screens.ScreenFromPoint(new PixelPoint(x.Item1, x.Item2));
                        var heightPoint = x.Item2 - (int)(this.Height * screen.PixelDensity);

                        if (heightPoint < 0)
                        {
                            this.Position = new PixelPoint(x.Item1, x.Item2);
                        }
                        else if ((x.Item1 + (int)this.Width + 30) > screen.WorkingArea.Width)
                        {
                            this.Position = new PixelPoint(x.Item1 - (int)(this.Width * screen.PixelDensity), heightPoint);
                        }
                        else
                        {
                            this.Position = new PixelPoint(x.Item1, heightPoint);
                        }
                    });
            }

            if (OperatingSystem2.IsWindows)
            {
                DI.Get<ISystemWindowApiService>().SetActiveWindow(new Models.HandleWindow { Handle = this.PlatformImpl.Handle.Handle });
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
