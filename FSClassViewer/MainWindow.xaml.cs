using RosterTreeViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace FSClassViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FSViewModel _fsVM;// = new FSViewModel(this);

        public MainWindow()
        {
            _fsVM = new FSViewModel(this);
            InitializeComponent();
            DataContext = _fsVM;
            trvRosters.ItemsSource = _fsVM.Years;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExpandTop(trvRosters.ItemContainerGenerator.ContainerFromItem(trvRosters.Items[0]) as TreeViewItem);

        }
        private void ExpandTop(ItemsControl items)
        {
            items.ApplyTemplate();
            TreeViewItem item = items as TreeViewItem;
            item.IsExpanded = true;
            item.UpdateLayout();
            if(item.Items.Count > 0)
            {
                item = item.ItemContainerGenerator.ContainerFromItem(items.Items[0]) as TreeViewItem;
                if (item != null)
                    item.IsExpanded = true;
            }
        }
        private void CloseApp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void IRButton_Click(object sender, RoutedEventArgs e)
        {
            _fsVM.GenerateFiles();
        }
        private void LoadClassButton_Click(object sender, RoutedEventArgs e)
        {
            _fsVM.LoadClass();
            actionsPanel.IsEnabled = true;
        }

        private void GradesButton_Click(object sender, RoutedEventArgs e)
        {
            _fsVM.RefreshClass();//.LoadGrades();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                ContainingBorder.Padding = new Thickness(5,10,10,5);
                ContainingBorder.BorderThickness = new Thickness(0D);
            }
            else
            {
                ContainingBorder.BorderThickness = new Thickness(1D);
                ContainingBorder.Padding = new Thickness(0.0);
            }
        }

        private void FinalIRButton_Click(object sender, RoutedEventArgs e)
        {
            _fsVM.MakeFinalIR();
        }

        private void RecentClassSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                _fsVM.LoadRecent((RecentClass)e.AddedItems[0]);
                actionsPanel.IsEnabled = true;
                if (sender is ComboBox box)
                {
                    box.SelectedIndex = 0;
                }
            }
        }

        private void trvRosters_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.WriteLine($"{e.OldValue} {e.NewValue}");
            if (e.NewValue != null && e.NewValue is MonthData md)
            {
                if (md.Classes.Count == 1)
                {
                    ClassData cd = md.Classes[0];
                    Debug.WriteLine($"LOADING MONTH'S ONLY CLASS {cd.ClassName}");
                    _fsVM.SelectedClass = cd;
                    foreach (var roster in cd.Rosters)
                    {
                        Debug.WriteLine(roster);
                    }
                    actionsPanel.IsEnabled = true;

                }
                else
                {
                    Debug.WriteLine($"Month has multiple classes -- need to select one to load");
                }
            }
            else if (e.NewValue != null && e.NewValue is ClassData cd)
            {
                Debug.WriteLine($"LOADING CLASS {cd.ClassName}");
                _fsVM.SelectedClass = cd;
                actionsPanel.IsEnabled = true;
            }
        }
    }
}
