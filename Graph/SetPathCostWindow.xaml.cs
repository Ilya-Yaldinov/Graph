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
    /// <summary>
    /// Логика взаимодействия для SetPathCostWindow.xaml
    /// </summary>
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
    }
}
