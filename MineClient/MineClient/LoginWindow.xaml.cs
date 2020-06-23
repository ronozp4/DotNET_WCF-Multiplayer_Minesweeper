using MineClient.ServiceReference1;
using System;
using System.Collections.Generic;
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
    // Login Window
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        //connect button
        private void CButton_Click(object sender, RoutedEventArgs e)
        {
            isClientExist();
            
        }

        //check keys
        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                isClientExist();
            }
        }

        // Register button event.
        private void RButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            this.Close();
            registerWindow.Show();
        }

        // Connection function.
        private Boolean isClientExist()
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && !string.IsNullOrEmpty(tbPassword.Password))
            {
                ClientCallback callback = new ClientCallback();
                MineServiceClient client = new MineServiceClient(new InstanceContext(callback));
                string username = tbUsername.Text.Trim();
                string password = tbPassword.Password.Trim();

                try
                {
                    if (client.ClientExist(username, password))
                    {

                        GameTypeChooseWindow gameTypeChooseWindow = new GameTypeChooseWindow(username, password);
                        gameTypeChooseWindow.Show();
                        this.Close();
                    }   
                }
                catch (FaultException<UserFaultException> ex)
                {
                    MessageBox.Show(ex.Detail.Message);

                }

            }
            return false;
        }
    }
}

