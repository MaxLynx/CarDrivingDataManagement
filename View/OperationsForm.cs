using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarDrivingDataManagement.View
{
    public partial class OperationsForm : Form
    {
        DriverController Controller { get; set; }
        public OperationsForm(bool created, String filename, int recordsCountPerBlock)
        {
            if (created)
            {
                Controller = new DriverController(filename, recordsCountPerBlock);
            }
            else
            {
                Controller = new DriverController(filename);
            }
            InitializeComponent();
        }

        private void OperationsForm_Load(object sender, EventArgs e)
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            Controller.Close();
            base.Dispose(disposing);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = Controller.DescribeDriverDBStructure();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Controller.GenerateData(Int32.Parse(textBox2.Text));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            String[] data = Controller.GetDriverByID(Int32.Parse(textBox4.Text));
            textBox5.Text = data[0];
            textBox6.Text = data[1];
            textBox4.Text = data[2];
            if (data[3] != null)
            {
                dateTimePicker1.Value = DateTime.Parse(data[3]);
            }
            if (data[4] != null)
            {
                checkBox1.Checked = Boolean.Parse(data[4]);
            }
            textBox3.Text = data[5];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(Controller.AddDriver(textBox5.Text, textBox6.Text, Int32.Parse(textBox4.Text),
                dateTimePicker1.Value, checkBox1.Checked, Int32.Parse(textBox3.Text)))
            {
                MessageBox.Show("New driver added");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(Controller.UpdateDriver(textBox5.Text, textBox6.Text, Int32.Parse(textBox4.Text),
                dateTimePicker1.Value, checkBox1.Checked, Int32.Parse(textBox3.Text)))
            {
                MessageBox.Show("Information updated");
            }
        }
    }
}
