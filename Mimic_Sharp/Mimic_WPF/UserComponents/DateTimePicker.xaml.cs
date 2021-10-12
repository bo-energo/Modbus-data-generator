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

namespace Mimic_WPF
{
    /// <summary>
    /// Логика взаимодействия для DateTimePicker.xaml
    /// </summary>
    public partial class DateTimePicker : UserControl
    {
        private int hours;
        private int minutes;
        private int seconds;

        public int Hours { get => hours; set { hours = value; h_box.Text = value.ToString(); } }
        public int Minutes { get => minutes; set { minutes = value; m_box.Text = value.ToString(); } }
        public int Seconds { get => seconds; set { seconds = value; s_box.Text = value.ToString(); } }

        // #################################################################################

        public DateTimePicker()
        {
            InitializeComponent();
        }


        private void Change_Click(object sender, RoutedEventArgs e)
        {
            var s = sender as Button;
            if (s.Tag == null)
                return;

            int tag = int.Parse(s.Tag as string);
            int buf;

            switch (tag)
            {
                case 1:
                    buf = Hours + 1;
                    Hours = Math.Min(9999, buf);
                    break;
                case 2:
                    buf = Hours - 1;
                    Hours = Math.Max(0, buf);
                    break;
                case 3:
                    buf = Minutes + 1;
                    Minutes = Math.Min(9999, buf);
                    break;
                case 4:
                    buf = Minutes - 1;
                    Minutes = Math.Max(0, buf);
                    break;
                case 5:
                    buf = Seconds + 1;
                    Seconds = Math.Min(9999, buf);
                    break;
                case 6:
                    buf = Seconds - 1;
                    Seconds = Math.Max(0, buf);
                    break;
            }
            
        }

        private void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = sender as TextBox;
            if (s.Tag == null)
                return;

            int tag = int.Parse(s.Tag as string);
            int buf = 0;
            bool f = int.TryParse(s.Text, out buf);

            buf = Math.Min(9999, Math.Max(0, buf));

            switch (tag)
            {
                case 1:
                    Hours = f ? buf : Hours;
                    break;
                case 2:
                    Minutes = f ? buf : Minutes;
                    break;
                case 3:
                    Seconds = f ? buf : Seconds;
                    break;
            }
        }

    }
}
