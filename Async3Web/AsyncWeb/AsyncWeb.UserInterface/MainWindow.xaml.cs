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

        // Good way with async
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            using HttpClient client = new HttpClient();            
            HttpResponseMessage response = await client.GetAsync("http://localhost:5000");
            string result = await response.Content.ReadAsStringAsync();
            MessageBox.Show(result);
        }
    }
}
