using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarDrivingDataManagement.View
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            File.WriteAllText(textBox6.Text, "");
            File.WriteAllText(textBox5.Text, "");
            File.WriteAllText(textBox4.Text, "");
            new CarOperationsForm(true,
                textBox6.Text, Int32.Parse(textBox7.Text),
                textBox5.Text, Int32.Parse(textBox8.Text),
                textBox4.Text, Int32.Parse(textBox9.Text)
                ).Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new CarOperationsForm(false,
                textBox1.Text, 0,
                textBox2.Text, 0,
                textBox3.Text, 0
                ).Show();
        }
    }
}
