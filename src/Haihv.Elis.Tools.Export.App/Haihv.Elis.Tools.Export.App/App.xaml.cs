namespace Haihv.Elis.Tools.Export.App
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Lấy tỷ lệ thu phóng của màn hình:
            var scale = DeviceDisplay.MainDisplayInfo.Density;
            return  new Window(new MainPage())
            {
                Title = "Trích xuất dữ liệu Elis SQL",
                Width = 960 * scale,
                Height = 750 * scale,
                MinimumHeight = 600,
                MinimumWidth = 800,
            };
        }
    }
}
