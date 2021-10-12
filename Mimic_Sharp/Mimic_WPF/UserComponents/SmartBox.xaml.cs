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

namespace Mimic_WPF.UserComponents
{
    /// <summary>
    /// Логика взаимодействия для SmartBox.xaml
    /// </summary>
    public partial class SmartBox : UserControl
    {
        private int first = 0;
        private int second = 0;

        public int First 
        { 
            get => first;
            set
            {
                first = value;
                first_box.Text = value.ToString();
                fix_box.Text = value.ToString();
            } 
        }
        public int Second
        {
            get => second;
            set
            {
                second = value;
                second_box.Text = value.ToString();
            }
        }

        public bool RandMode
        {
            get => (bool)randFlag.IsChecked;
            set { randFlag.IsChecked = value; }
        }

    // #################################################################################

    public SmartBox()
        {
            InitializeComponent();
        }

        private void Rand_Checked(object sender, RoutedEventArgs e)
        {
            first_box.Visibility = Visibility.Visible;
            second_box.Visibility = Visibility.Visible;
            fix_box.Visibility = Visibility.Hidden;
        }

        private void Rand_Unchecked(object sender, RoutedEventArgs e)
        {
            first_box.Visibility = Visibility.Hidden;
            second_box.Visibility = Visibility.Hidden;
            fix_box.Visibility = Visibility.Visible;
        }

        private void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = sender as TextBox;
            if (s.Tag == null)
                return;

            int tag = int.Parse(s.Tag as string);
            int buf = 0;
            bool f = int.TryParse(s.Text, out buf);

            switch (tag)
            {
                case 1:
                    First = f ? buf : First;
                    break;
                case 2:
                    Second = f ? buf : Second;
                    break;
            }
        }


    }
}
