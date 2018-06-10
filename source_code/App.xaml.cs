using System.Windows;

namespace LevelEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Windows to display
        public static StartWindow startWindow;
        public static GameProperties gamePropertiesWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Main Window
            this.MainWindow = new MainWindow();
            MainWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            MainWindow.Focusable = false;
            MainWindow.IsEnabled = false;
            MainWindow.Show();

            // Start Window
            startWindow = new StartWindow();
            startWindow.Activate();
            startWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            startWindow.Show();

            // General Window
            gamePropertiesWindow = new GameProperties();
            gamePropertiesWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            gamePropertiesWindow.Hide();

        }
    }
}
