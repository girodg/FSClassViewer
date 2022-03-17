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

namespace FSClassViewer
{
    /// <summary>
    /// Interaction logic for FailureWindow.xaml
    /// </summary>
    public partial class FailureWindow : Window
    {
        public bool IsOk { get; private set; } = false;
        public FailureWindow()
        {
            InitializeComponent();
        }

        public void ShowView(MainWindow mainWin)
        {
            Owner = mainWin;
            ShowDialog();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            IsOk = false;
            Close();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            IsOk = true;
            Close();
        }
    }
}
