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

            textBox2.Text = (Convert.ToString(Form1.cal_value));
            textBox1.Text = Form1.strVISArsrc;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "You pressed save :-p";
           
            
            
        }
    }
}
