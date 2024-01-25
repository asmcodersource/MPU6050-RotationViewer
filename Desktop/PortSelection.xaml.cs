using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.Ports;

namespace SystemMPU6050
{
    /// <summary>
    /// Interaction logic for PortSelection.xaml
    /// </summary>
    public partial class PortSelection : Window
    {
        
        public PortSelection()
        {
            InitializeComponent();
            Closing += (arg1, arg2) => { System.Windows.Application.Current.Shutdown(); };
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Button selectButton = this.FindName("selectButton") as Button;
            selectButton.IsEnabled = true;
        }

        

        private void PortComboBoxOpened(object sender, EventArgs e)
        {
            ComboBox portsBox = this.FindName("portsBox") as ComboBox;
            portsBox.Items.Clear();

            // Получим список всех доступных COM-портов
            string[] portNames = SerialPort.GetPortNames();

            // Добавим порты в ComboBox
            foreach (string portName in portNames)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = portName;
                portsBox.Items.Add(comboBoxItem);
            }
        }

        private void SelectClicked(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var window = new MainWindow(portsBox.Text);
            window.Show();
        }
    }
}
