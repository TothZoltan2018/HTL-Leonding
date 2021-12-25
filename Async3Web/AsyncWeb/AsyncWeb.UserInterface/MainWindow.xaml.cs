using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace AsyncWeb.UserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Run this code while AsyncWeb_Webserver is running with Ctrl+F5
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using HttpClient client = new HttpClient();
            // Bad code, brcause .Result is blocking
            HttpResponseMessage response = client.GetAsync("http://localhost:5000").Result;
            string result = response.Content.ReadAsStringAsync().Result;
            MessageBox.Show(result);
        }
    }
}
