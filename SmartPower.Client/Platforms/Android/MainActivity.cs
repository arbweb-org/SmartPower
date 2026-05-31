using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace SmartPower.Client;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetFullScreen();
    }

    protected override void OnResume()
    {
        base.OnResume();
        SetFullScreen();
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus) SetFullScreen();
    }

    private void SetFullScreen()
    {
#pragma warning disable CA1422
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            Window!.SetDecorFitsSystemWindows(false);
            Window.InsetsController?.Hide(
                WindowInsets.Type.StatusBars() |
                WindowInsets.Type.NavigationBars()
            );
            Window.InsetsController!.SystemBarsBehavior =
                (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
        }
        else
        {
            Window!.DecorView.SystemUiFlags =
                SystemUiFlags.Fullscreen |
                SystemUiFlags.HideNavigation |
                SystemUiFlags.ImmersiveSticky |
                SystemUiFlags.LayoutFullscreen |
                SystemUiFlags.LayoutHideNavigation |
                SystemUiFlags.LayoutStable;
        }
#pragma warning restore CA1422
    }
}