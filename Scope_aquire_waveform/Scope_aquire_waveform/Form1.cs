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
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace Scope_aquire_waveform
{
    public partial class Form1 : Form
    {
        private MessageBasedSession mbSession;
        double[] voltage;

        string timescale;
        string timeoffset;
        string voltscale;
        string voltoffset;
        string sample_rate;

        float[] ch1_data;
        float[] ch2_data;
        float[] time;
        float fltVoltscale;



        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            string strVISArsrc = txtVISAID.Text;
            try
            {
                mbSession = (MessageBasedSession)ResourceManager.GetLocalManager().Open(strVISArsrc);
            }
            catch (InvalidCastException)
            {
                MessageBox.Show("Resource selected is not an message based session");
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void btnWaveform_Click(object sender, EventArgs e)
        {

             try
             {
                 timescale = mbSession.Query(":TIM:SCAL?");
                 timeoffset = mbSession.Query(":TIM:OFFS?");
                 voltscale = mbSession.Query(":CHAN1:SCAL?");
                 voltoffset = mbSession.Query(":CHAN1:OFFS?");
                 sample_rate = mbSession.Query(":ACQ:SAMP?");
                 mbSession.Write(":WAV:POIN:MODE RAW");
             }
             catch (Exception exp)
             {
                 MessageBox.Show(exp.Message);
             }

          byte[] strRead = null;
 
             try
             {
                 mbSession.Write(":WAV:DATA? CHAN1");
                strRead = mbSession.ReadByteArray();
             }
             catch (Exception exp)
             {
                 MessageBox.Show(exp.Message);
             }
            

            int[] intRead;
       

            intRead = new int[strRead.Length];
            int length = strRead.Length;
            voltage = new double[intRead.Length-9];

            for (int j = 0; j < length; j++)
            {
                intRead[j] = (int)strRead[j]; // my nasty byte to int converter :p
            }

            // in this part, inverting the data is not neccesarry, due to the fact that [0,0] is in top corner
             for (int j = 0; j < length-11; j++)
            {
                voltage[j] = (float)strRead[j + 10];// *1.0 + 255.0; //invert the data
                //data = data * -1 + 255
            }

            ch1_data = new float[voltage.Length];

          // Double.Parse("1.234567E-06", NumberStyles.Float, CultureInfo.InvariantCulture)

            float fltVoltoffset = float.Parse(voltoffset,NumberStyles.Float,CultureInfo.InvariantCulture);
                  fltVoltscale = float.Parse(voltscale,NumberStyles.Float,CultureInfo.InvariantCulture);
  
            for (int j = 0; j < voltage.Length; j++)
            {
                ch1_data[j] = (float)(((voltage[j] - 125) / 25) * fltVoltscale);// -fltVoltoffset; //- (fltVoltoffset / fltVoltscale)*25) / (25 *fltVoltscale);
                //data = (data - 130.0 - voltoffset/voltscale*25) / 25 * voltscale
                //observe that negative vectors = positive going amplitude? 
            }

            //need to apply correction to timedata
            time = new float[ch1_data.Length];

            for (int j = 0; j < ch1_data.Length; j++)
            {
                time[j] = j;
            }

            this.panel1.Invalidate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) //this part disposes VISA when the window are closed
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

        private void panel1_Paint(object sender, PaintEventArgs e) // do the drawing of the graph.
        {
             if (ch1_data != null){

             
            Pen Pen1;
            Pen Pen3;
            Pen1 = new Pen(System.Drawing.Color.Blue, 1);
            Pen3 = new Pen(System.Drawing.Color.Red, 1);
            Graphics ClientDC = panel1.CreateGraphics();
            float xmax = time.Max();
            float ymax = (float)4.0*fltVoltscale;
            float xscale = panel1.Width;
            float yscale = panel1.Height;
            float length = ch1_data.Length;
            float halfheight =  (float)(ClientDC.VisibleClipBounds.Height / 2.0);

            for (int i = 0; i < length - 1; i++)
                {
                    ClientDC.DrawLine(Pen1, ((time[i + 1] / xmax) * xscale), (((ch1_data[i] / ymax)) *halfheight)+halfheight, ((time[i] / xmax) * xscale), (((ch1_data[i + 1] / ymax)) * halfheight)+halfheight);
                }

            for (int i = 0; i <= 9; i++)
            {
                ClientDC.DrawLine(Pen3, (i * xscale) / 10, (0 * yscale), (i * xscale) / 10, (1 * yscale));
                // 8 horisontal lines
            }

            for (int i = 0; i <= 11; i++)
            {
                ClientDC.DrawLine(Pen3, (1 * xscale), (((i * yscale) / 8)),(0 * xscale), ((i * yscale) / 8));
            }
                 
        }
    }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string FilePath = txtFilename.Text;
            string[] TotalData = new string[ch1_data.Length];
            string[] ch1data = new string[ch1_data.Length];
            for (int i = 0; i < ch1_data.Length ; i++)
            {
                ch1data[i] = (ch1_data[i]*-1.0).ToString(CultureInfo.InvariantCulture);
               TotalData[i] = ch1data[i];// +",";//+ LastNames[i] + "," + Ages[i];
            }
            File.WriteAllLines(FilePath, TotalData);
            MessageBox.Show("CSV File Created Successfully", "Success");
        
        }

    }
}
