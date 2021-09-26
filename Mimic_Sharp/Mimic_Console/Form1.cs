using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using BO_Math;

namespace Mimic_Console
{
    public partial class Form1 : Form
    {
        DataGenerator generator;
        int TS = 0;

        public Form1(DataGenerator generator)
        {
            InitializeComponent();

            this.generator = generator;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TS++;
            chart1.Series[0].Points.AddXY(TS, generator.Next());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int count = (int)numericUpDown1.Value;
            double[] values = generator.Next(count);

            for (int i = 0; i < count; i++)
            {
                TS++;
                chart1.Series[0].Points.AddXY(TS, values[i]);
            }
            
        }
    }
}
