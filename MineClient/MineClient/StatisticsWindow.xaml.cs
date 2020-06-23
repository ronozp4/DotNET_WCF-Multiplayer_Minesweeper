using System;
using System.Collections.Generic;
using System.Data;
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

namespace MineClient
{
    // Interaction logic for StatisticsWindow.xaml
    public partial class StatisticsWindow : Window
    {

        private static string factor;
        private DataRow row;

        public StatisticsWindow(string statFactor)
        {
            InitializeComponent();
            factor = statFactor;
        }

        // Get statistics from server through delegate to main window.
        public delegate void GetStatsDelegate(string statFactor);
        public event GetStatsDelegate getStats;

        private void Search(string statFactor)
        {
            try
            {
                getStats(statFactor);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                this.Close();
            }

        }

        // Callback function, fill DataGrid, triggered by main window.
        public void Populate(List<dynamic> list)
        {
            if (factor == "Players")
            {
                dglist.ItemsSource = PlayersTable(list).DefaultView;
            }
            else if (factor == "Games")
            {
                dglist.ItemsSource = GamesTable(list).DefaultView;
            }
            else
            {
                dglist.ItemsSource = LiveGamesTable(list).DefaultView;
            }
        }

        // Window events.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = factor + " statistics";
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Search(factor);
        }


        // Datatables builders.


        //setting the players table
        private DataTable PlayersTable(List<dynamic> list)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(String));
            dt.Columns.Add("Games Played", typeof(int));
            dt.Columns.Add("Points", typeof(int));
            dt.Columns.Add("Wins", typeof(int));
            dt.Columns.Add("Losses", typeof(int));
            dt.Columns.Add("Ties", typeof(int));
            dt.Columns.Add("Win Percent", typeof(int));

            for (int i = 0; i < list.Count();)
            {
                row = dt.NewRow();
                row["Name"] = list[i++] as string;
                row["Games Played"] = list[i++];
                row["Points"] = list[i++];
                row["Wins"] = list[i++];
                row["Losses"] = list[i++];
                row["Ties"] = list[i++];
                row["Win Percent"] = list[i++];
                dt.Rows.Add(row);
            }
            return dt;
        }

        //setting the games table
        private DataTable GamesTable(List<dynamic> list)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Winner", typeof(String));
            dt.Columns.Add("Looser", typeof(String));
            dt.Columns.Add("Date", typeof(String));

            for (int i = 0; i < list.Count();)
            {
                row = dt.NewRow();
                row["Winner"] = list[i++] as string;
                row["Looser"] = list[i++] as string;
                row["Date"] = list[i++] as string;
                dt.Rows.Add(row);
            }
            return dt;
        }

        //setting the live games table
        private DataTable LiveGamesTable(List<dynamic> list)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Player 1", typeof(String));
            dt.Columns.Add("Player 2", typeof(String));
            dt.Columns.Add("Started", typeof(String));

            for (int i = 0; i < list.Count();)
            {
                row = dt.NewRow();
                row["Player 1"] = list[i++] as string;
                row["Player 2"] = list[i++] as string;
                row["Started"] = list[i++] as string;
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}

