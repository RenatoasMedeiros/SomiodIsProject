﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TestApplicationNumpad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "1";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "2";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "3";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "4";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "5";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "6";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "7";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "8";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "9";
        }

        private void button0_Click(object sender, EventArgs e)
        {
            textBoxPin.Text += "0";
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxPin.Text = "";
        }

        private void buttonEnter_Click(object sender, EventArgs e)
        {
           

            string pin = textBoxPin.Text;
                            

                




        }

        private void labelValidation_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {

        }
    }
}
