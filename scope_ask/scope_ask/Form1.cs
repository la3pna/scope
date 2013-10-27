using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.VisaNS;

namespace scope_ask
{
    public partial class Form1 : Form
    {
        private MessageBasedSession mbSession;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            string strVISArsrc = textBox3.Text;
            try
            {
                mbSession = (MessageBasedSession)ResourceManager.GetLocalManager().Open(strVISArsrc);
            }
            catch (InvalidCastException)
            {
                MessageBox.Show("Resource selected iss not an message based session");
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            string strWrite = textBox1.Text;
            try
            {
                mbSession.Write(strWrite);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            byte[] strRead = null;

            try
            {
                strRead = mbSession.ReadByteArray();
                textBox2.Text = strRead.ToString();
               
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }

            int[] intRead;

            intRead = new int[strRead.Length];
            int length = strRead.Length;


            for (int j = 0; j < length; j++)
            {

                intRead[j] = (int)strRead[j]; // my nasty byte to int converter

            }

            //need to apply correction to data, may be better to write an separate aqquire program based on this




        }

        private void btnQuerry_Click(object sender, EventArgs e)
        {
            string strRead = null;

            try
            {
                strRead = mbSession.Query(textBox1.Text);
                textBox2.Text = strRead;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                mbSession.Dispose();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            Application.Exit();
        }
    }
}
