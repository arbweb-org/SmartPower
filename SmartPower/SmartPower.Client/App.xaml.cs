namespace SmartPower.Client
{
    public partial class App : Application, IDisposable
    {
        private readonly IServiceProvider _services;
        public App(IServiceProvider services)
        {
            InitializeComponent();
            _services = services;
            _services.GetRequiredService<Services.DeviceService>().Start();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "SmartPower.Client" };
            window.Destroying += (s, e) => Dispose();
            return window;
        }

        public void Dispose()
        {
            _services.GetRequiredService<Services.DeviceService>().Dispose();
        }
    }
}