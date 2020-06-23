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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WcfMineServer;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MineHost
{
  //Main Host
    public partial class MainWindow : Window
    {
        ServiceHost host = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (host != null)
            {
                host.Close();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try { 
            host = new ServiceHost(typeof(WcfMineServer.MineService));
            host.Description.Behaviors.Add(
                new ServiceMetadataBehavior { HttpGetEnabled = true });
            host.Open();
            label1.Content = "Host connected";
            }
            catch (Exception)
            {
                MessageBox.Show("Could not access server");
                this.Close();
            }
        }
    }
}
