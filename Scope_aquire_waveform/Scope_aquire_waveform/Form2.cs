using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scope_aquire_waveform
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            textBox2.Text = (Convert.ToString(Properties.Settings.Default.cal));
            textBox1.Text = Form1.strVISArsrc;
           
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "You pressed save :-p";

            Form1.cal_value = Convert.ToInt32(textBox2.Text);
            Properties.Settings.Default.cal = Convert.ToInt32(textBox2.Text);
            
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.cal_value = Convert.ToInt32(textBox2.Text);
            Properties.Settings.Default.cal = Convert.ToInt32(textBox2.Text);
            Properties.Settings.Default.Save();
           // Properties.Settings.Default.Upgrade();
          
        }
    }
}
