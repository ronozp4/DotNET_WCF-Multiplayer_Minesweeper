using MineClient.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
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

namespace MineClient
{
    // Register Window class
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RegisterFunc();
        }
        private void Button_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterFunc();
            }
        }

        // Window closed event.
        private void Window_Closed(object sender, EventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            this.Close();
            loginWindow.Show();
        }

        // Registration function.
        private void RegisterFunc()
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && !string.IsNullOrEmpty(tbPassword.Text))
            {
                ClientCallback callback = new ClientCallback();
                MineServiceClient client = new MineServiceClient(new InstanceContext(callback));
                string username = tbUsername.Text.Trim();
                string password = tbPassword.Text.Trim();

                try
                {
                    client.Register(username, password);
                    this.Close();
                    MessageBox.Show("Successfully registered.");
                }
                catch (FaultException<UserFaultException> ex)
                {
                    MessageBox.Show(ex.Detail.Message);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
