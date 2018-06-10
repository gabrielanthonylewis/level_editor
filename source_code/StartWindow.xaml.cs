using System;
using System.ComponentModel;
using System.Windows;


namespace LevelEditor
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public bool intendedClose = false;


        public StartWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Prevent closing on start up. Will only close when inteded..
            if(this.Visibility == Visibility.Visible && intendedClose == false)
               e.Cancel = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // If unintentionally closed the the main window will be closed (to exit the program)
            if (intendedClose == false && Application.Current.MainWindow != null)
                Application.Current.MainWindow.Close();

            intendedClose = false;
        }

        private void _start_button_Click(object sender, RoutedEventArgs e)
        {
            // Show the main window for use
            Application.Current.MainWindow.Activate();
            Application.Current.MainWindow.Focusable = true;
            Application.Current.MainWindow.IsEnabled = true;

            // Assign all of the values and intialise the main window
            ((MainWindow)System.Windows.Application.Current.MainWindow).SetMapHeight((int)_MapHeight.Value);
            ((MainWindow)System.Windows.Application.Current.MainWindow).SetMapWidth((int)_MapWidth.Value);
            ((MainWindow)System.Windows.Application.Current.MainWindow).SetTileWidth((int)_TileWidth.Value);
            ((MainWindow)System.Windows.Application.Current.MainWindow).SetPath("");
            ((MainWindow)System.Windows.Application.Current.MainWindow).Initialise(".../.../images/sheetGeneral.png");

            // Hide the window
            intendedClose = true;
            this.Visibility = Visibility.Hidden;
        }
    }
}
