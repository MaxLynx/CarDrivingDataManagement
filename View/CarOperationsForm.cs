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
    public partial class CarOperationsForm : Form
    {
        CarController CarController { get; set; }
        public CarOperationsForm(bool created, String filename1, int recordsCountPerBlock1,
            String filename2, int recordsCountPerBlock2,
            String filename3, int recordsCountPerBlock3)
        {
            if (created)
            {
                CarController = new CarController(filename1, recordsCountPerBlock1,
                    filename2, recordsCountPerBlock2,
                    filename3, recordsCountPerBlock3);
            }
            else
            {
                CarController = new CarController(filename1,
                    filename2,
                    filename3);
            }
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CarController.GenerateData(Int32.Parse(textBox2.Text));
        }

        private void CarOperationsForm_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Car IDs BTree" + CarController.DescribeCarIDDBStructure() + "@@@@@@@@@@@@@@@@@@@@@@\r\n"
                + "Car VINs BTree" + CarController.DescribeCarVINDBStructure() + "@@@@@@@@@@@@@@@@@@@@@@\r\n"
                + "Car Heap File" + CarController.DescribeCarHeapFileDBStructure() + "@@@@@@@@@@@@@@@@@@@@@@\r\n";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            String[] data = CarController.GetCarByID(textBox5.Text);
            textBox5.Text = data[0];
            textBox3.Text = data[1];
            textBox6.Text = data[2];
            textBox4.Text = data[3];
            
            if (data[4] != null)
            {
                checkBox1.Checked = Boolean.Parse(data[4]);
            }
            if (data[5] != null)
            {
                dateTimePicker1.Value = DateTime.Parse(data[5]);
            }
            if (data[6] != null)
            {
                dateTimePicker2.Value = DateTime.Parse(data[6]);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            String[] data = CarController.GetCarByVIN(textBox3.Text);
            textBox5.Text = data[0];
            textBox3.Text = data[1];
            textBox6.Text = data[2];
            textBox4.Text = data[3];

            if (data[4] != null)
            {
                checkBox1.Checked = Boolean.Parse(data[4]);
            }
            if (data[5] != null)
            {
                dateTimePicker1.Value = DateTime.Parse(data[5]);
            }
            if (data[6] != null)
            {
                dateTimePicker2.Value = DateTime.Parse(data[6]);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(CarController.AddCar(textBox5.Text, textBox3.Text, Int32.Parse(textBox6.Text), Int32.Parse(textBox4.Text),
                checkBox1.Checked, dateTimePicker1.Value, dateTimePicker2.Value))
                {
                MessageBox.Show("New car was added");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (CarController.UpdateCar(textBox5.Text, textBox3.Text, Int32.Parse(textBox6.Text), Int32.Parse(textBox4.Text),
                checkBox1.Checked, dateTimePicker1.Value, dateTimePicker2.Value))
            {
                MessageBox.Show("Car data was updated");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            CarController.Close();
            base.Dispose(disposing);
        }
    }
}
