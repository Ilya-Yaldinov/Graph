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

namespace Graph
{
    public partial class SetPathCostWindow : Window
    {
        public int pathCost = 1;
        private string getText;

        public SetPathCostWindow()
        {
            InitializeComponent();
        }

        private void SetPathCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            getText = SetPathCost.Text;
            int.TryParse(getText, out pathCost);
        }

        private void ConfrimAddPathCostButton_Click(object sender, RoutedEventArgs e)
        {    
        }

        private void SetBothArrowButton_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.setArrowBoth = true;
            MainWindow.setArrowEnd  = false;
        }

        private void SetOneArrowButton_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.setArrowEnd  = true;
            MainWindow.setArrowBoth = false;
        }
    }
}
