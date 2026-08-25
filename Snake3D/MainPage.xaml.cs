using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp.Views.Windows;
using Snake3D.ViewModels;

namespace Snake3D;

public sealed partial class MainPage : Page
{
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _renderTimer;

    public MainViewModel ViewModel => _viewModel;

    public MainPage()
    {
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        this.InitializeComponent();

        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _renderTimer.Tick += OnRenderTick;

        this.Loaded += OnPageLoaded;
        this.Unloaded += OnPageUnloaded;

        this.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnGlobalKeyDown), handledEventsToo: true);
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        _renderTimer.Start();
        this.Focus(FocusState.Programmatic);
        Rendering.GameRenderer3D.GenerateStoreScreenshots("assets/store");
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        _renderTimer.Stop();
    }

    private void OnRenderTick(object? sender, object e)
    {
        _viewModel.UpdateFrame();
        GameCanvas.Invalidate();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        _viewModel.Renderer.Render(canvas, info.Width, info.Height, _viewModel.Engine, 0.016f);
    }

    private void OnGlobalKeyDown(object sender, KeyRoutedEventArgs e)
    {
        _viewModel.HandleKeyDown(e.Key);
    }

    private void OnPageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        _viewModel.HandleKeyDown(e.Key);
        e.Handled = true;
    }
}
