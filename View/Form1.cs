using CarDrivingDataManagement.View;
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

namespace CarDrivingDataManagement
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            File.WriteAllText(textBox2.Text, "");
            new OperationsForm(true, textBox2.Text, Int32.Parse(textBox3.Text)).Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new OperationsForm(false, textBox1.Text, 0).Show();
        }
    }
}
