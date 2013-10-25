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

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private MessageBasedSession mbSession;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            string strVISARsrc = TXTid.Text;

            try
            {
                mbSession = (MessageBasedSession)ResourceManager.GetLocalManager().Open(strVISARsrc);
            }
            catch(InvalidCastException)
            {
                MessageBox.Show("Resource selected must be an message based session");
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message); ;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                mbSession.Dispose();
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            string strWrite = txtWrite.Text;

            try
            {
                mbSession.Write(strWrite);
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            string strRead = null;

            try
            {
                strRead = mbSession.ReadString();
                txtRead.Text = strRead; 
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
           
        }

        private void txtRead_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
