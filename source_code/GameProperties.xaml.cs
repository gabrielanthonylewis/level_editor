using System.ComponentModel;
using System.Windows;

namespace LevelEditor
{
    /// <summary>
    /// Interaction logic for GeneralProperties.xaml
    /// </summary>
    public partial class GameProperties : Window
    {
        public GameProperties()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Prevent from closing if the window is visible
            if (this.Visibility == Visibility.Visible)
                e.Cancel = true;
        }

        // Update the displayed values with the new ones provided
        public void RefreshInfo(LevelEditor.GeneralGameProperties prop)
        {
            _Lives.Value = prop.lives;
            _Health.Value = prop.health;
            _DamageSpikes.Value = prop.damageSpikes;
            _DamageSawBots.Value = prop.damageSawBot;
            _DamageTurretBots.Value = prop.damageTurretBot;
            _Timer.Value = prop.timer;
        }

        
        private void Okay_Click(object sender, RoutedEventArgs e)
        {
            // Activate the main window for use
            Application.Current.MainWindow.Activate();
            Application.Current.MainWindow.Focusable = true;
            Application.Current.MainWindow.IsEnabled = true;

            // Set the actual data store on the main class for later use (saving)
            LevelEditor.GeneralGameProperties prop = new GeneralGameProperties();
            prop.lives = (int)_Lives.Value;
            prop.health = (int)_Health.Value;
            prop.damageSpikes = (int)_DamageSpikes.Value;
            prop.damageSawBot = (int)_DamageSawBots.Value;
            prop.damageTurretBot = (int)_DamageTurretBots.Value;
            prop.timer = (int)_Timer.Value;

            ((MainWindow)System.Windows.Application.Current.MainWindow).SetGeneralGameProperties(prop);

            // Close general window (as done with it)
            this.Visibility = Visibility.Hidden;
        }
    }
}
