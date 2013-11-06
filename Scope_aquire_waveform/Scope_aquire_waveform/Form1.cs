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
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Scope_aquire_waveform
{
    public partial class Form1 : Form
    {
        private MessageBasedSession mbSession;
        double[] voltage;

        string timescale;
        string timeoffset;

        string sample_rate;

        float[] ch1_data;
        float[] ch2_data;
        float[] time;
        float[] time_vector;
        float fltVoltscale_ch1;
        float fltVoltscale_ch2;
        float fltVoltoffset_ch1;
        float fltVoltoffset_ch2;
        Form2 secondForm = new Form2();
        public static int cal_value = Properties.Settings.Default.cal;
        public static string strVISArsrc;

           


        public Form1()
        {
            
            InitializeComponent();
            cal_value = Properties.Settings.Default.cal;
            
            try
            {
                string[] stSesssion_avaible = ResourceManager.GetLocalManager().FindResources("?*");
                comboBox1.Items.AddRange(stSesssion_avaible);
            }
            catch (Exception exp) { MessageBox.Show(exp.Message+"\n \n This error may be due to VISA not being installed or the instrument not found");  }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            strVISArsrc = comboBox1.Text;
            try
            {
                mbSession = (MessageBasedSession)ResourceManager.GetLocalManager().Open(strVISArsrc);
               // if (mbSession.ResourceManufacturerID == ){
             //  string a  = (string)mbSession.ResourceManufacturerID ;
               // }
               // here it should check for the appropriate Rigol or Agilent scope, given that agilent have same data set
     
                
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
            string voltscale;
            string voltoffset;
            voltscale = null;
            voltoffset = null;

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



            fltVoltoffset_ch1 = float.Parse(voltoffset, NumberStyles.Float, CultureInfo.InvariantCulture);
            fltVoltscale_ch1 = float.Parse(voltscale, NumberStyles.Float, CultureInfo.InvariantCulture);

            ch1_data = calc_voltage(strRead, fltVoltscale_ch1, fltVoltoffset_ch1);

            strRead = null;


            try
            {
                voltscale = mbSession.Query(":CHAN2:SCAL?");
                voltoffset = mbSession.Query(":CHAN2:OFFS?");
                mbSession.Write(":WAV:DATA? CHAN2");
                strRead = mbSession.ReadByteArray();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            fltVoltoffset_ch2 = float.Parse(voltoffset, NumberStyles.Float, CultureInfo.InvariantCulture);
            fltVoltscale_ch2 = float.Parse(voltscale, NumberStyles.Float, CultureInfo.InvariantCulture);

            ch2_data = calc_voltage(strRead, fltVoltscale_ch2, fltVoltoffset_ch2);


            //need to apply correction to timedata
            time = new float[ch1_data.Length];

            for (int j = 0; j < ch1_data.Length; j++)
            {
                time[j] = j;
            }

            mbSession.Write(":KEY:FORC");


           float timescale_fl = float.Parse(timescale, NumberStyles.Float, CultureInfo.InvariantCulture);
            int steps = time.Length / 12 ;
            

            time_vector = new float[time.Length];
            int i = 0;
            for (float k = -(time.Length/2); k < (time.Length/2); k++)
            {
                time_vector[i] = (k/steps)*(timescale_fl);
                i = i + 1;
            }


            this.panel1.Invalidate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) //this part disposes VISA when the window are closed
        {
            try
            {
                mbSession.Dispose();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) // do the drawing of the graph.
        {
            if (ch1_data != null)
            {


                Pen Pen1;
                Pen Pen2;
                Pen Pen3;

                Pen1 = new Pen(System.Drawing.Color.Blue, 1);
                Pen2 = new Pen(System.Drawing.Color.SeaGreen, 1);
                Pen3 = new Pen(System.Drawing.Color.Sienna, 1);
                Graphics ClientDC = panel1.CreateGraphics();
                float xmax = time.Max();
                float y1max = (float)4.0 * fltVoltscale_ch1;
                float y2max = (float)4.0 * fltVoltscale_ch2;
                float xscale = panel1.Width;
                float yscale = panel1.Height;
                float length = ch1_data.Length;
                float halfheight = (float)(ClientDC.VisibleClipBounds.Height / 2.0);

                for (int i = 0; i < length - 1; i++)
                {
                    ClientDC.DrawLine(Pen1, ((time[i + 1] / xmax) * xscale), (((ch1_data[i] / y1max)) * halfheight) + halfheight, ((time[i] / xmax) * xscale), (((ch1_data[i + 1] / y1max)) * halfheight) + halfheight);
                }

                for (int i = 0; i < length - 1; i++)
                {
                    ClientDC.DrawLine(Pen2, ((time[i + 1] / xmax) * xscale), (((ch2_data[i] / y2max)) * halfheight) + halfheight, ((time[i] / xmax) * xscale), (((ch2_data[i + 1] / y2max)) * halfheight) + halfheight);
                }
                label2.ForeColor = Color.Blue;
                label2.Text = Convert.ToString(fltVoltscale_ch1) + " V grid";
                label4.ForeColor = Color.SeaGreen;
                label4.Text = Convert.ToString(fltVoltscale_ch2) + " V grid";

                for (int i = 0; i <= 12; i++)
                {
                    ClientDC.DrawLine(Pen3, (i * xscale) / 12, (0 * yscale), (i * xscale) / 12, (1 * yscale));
                    // 8 horisontal lines
                }

                for (int i = 0; i <= 8; i++)
                {
                    ClientDC.DrawLine(Pen3, (1 * xscale), (((i * yscale) / 8)), (0 * xscale), ((i * yscale) / 8));
                }

                for (int i = 0; i <= 80; i++)
                {
                    ClientDC.DrawLine(Pen3, (float)(0.49 * xscale), (((i * yscale) / 80)), (float)(0.51 * xscale), ((i * yscale) / 80));
                    //tics y 
                }
                for (int i = 0; i <= 120; i++)
                {
                    ClientDC.DrawLine(Pen3, (float)(i * xscale) / 120, (float)((0.49 * yscale)), (float)(i * xscale) / 120, (float)(0.51 * yscale));
                    // tics x
                }

                string timeend;
                timeend = "s";
                float flttime = float.Parse(timescale, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (flttime < 1 & flttime > 0.0009)
                {
                    timeend = "ms";
                    flttime = flttime * 1000;
                }
                else if (flttime < 0.001 & flttime > 0.000009)
                {
                    timeend = "µs";
                    flttime = flttime * 1000000;
                }
                else if (flttime < 0.000001)
                {
                    timeend = "ns";
                    flttime = flttime * 1000000000;
                }


                label5.Text = Convert.ToString(flttime) + " " + timeend + " grid";

            }
        }

        private void save_csv(string FilePath)
        {
            //string FilePath = txtFilename.Text;
            string[] TotalData = new string[ch1_data.Length];
            string[] ch1data = new string[ch1_data.Length];
            string[] ch2data = new string[ch2_data.Length];
            string[] timedata = new string[time_vector.Length];
            string[] timevector = new string[time_vector.Length];
            for (int i = 0; i < ch1_data.Length; i++)
            {
                ch1data[i] = (ch1_data[i] * -1.0).ToString(CultureInfo.InvariantCulture);
                ch2data[i] = (ch2_data[i] * -1.0).ToString(CultureInfo.InvariantCulture);
                timedata[i] = (time_vector[i]).ToString(CultureInfo.InvariantCulture);
                TotalData[i] = ch1data[i] + "," + ch2data[i] + "," + timedata[i];
            }
            File.WriteAllLines(FilePath, TotalData);
            
            MessageBox.Show("CSV File Created Successfully", "Success");

        } 		//FilePath	"C:\\Users\\Thomas\\Desktop\\test.csv"	string

        public float[] calc_voltage(byte[] vector, float voltscale, float voltoffset)
        {
            int length = vector.Length;
            voltage = new double[length - 9];
            // in this part, inverting the data is not neccesarry, due to the fact that [0,0] is in top corner
            for (int j = 0; j < length - 11; j++)
            {
                voltage[j] = (float)vector[j + 10];// *1.0 + 255.0; //invert the data
                //data = data * -1 + 255
            }
            float[] data;
            data = new float[voltage.Length];

            // Double.Parse("1.234567E-06", NumberStyles.Float, CultureInfo.InvariantCulture)

            for (int j = 0; j < voltage.Length; j++)
            {
                data[j] = (float)(((voltage[j] - cal_value) / 25) * voltscale);// -voltoffset; //- (voltoffset / voltscale)*25) / (25 *voltscale);
                //data = (data - 130.0 - voltoffset/voltscale*25) / 25 * voltscale
                //observe that negative vectors = positive going amplitude? 
            }
            return data;
        } // Does the scaling and calculations on the vectors

        private void PrintScreen(string file)
        {
            /*
            Application.DoEvents();
           

            Rectangle bounds = this.Bounds;
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }
                bitmap.Save(file , ImageFormat.Jpeg);
                bitmap.Dispose();
            }
            */
            string initialDirectory = file;
            using (BackgroundWorker worker = new BackgroundWorker()) {
    Thread.Sleep(0);
    this.Refresh();
    worker.DoWork += delegate(object sender, DoWorkEventArgs e) {
      BackgroundWorker wkr = sender as BackgroundWorker;
      Rectangle bounds = new Rectangle(Location, Size);
      Thread.Sleep(300);
      Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
      using (Graphics g = Graphics.FromImage(bitmap)) {
        g.CopyFromScreen(Location, Point.Empty, bounds.Size);
      }
      e.Result = bitmap;
    };
    worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e) {
      if (e.Error != null) {
        Exception err = e.Error;
        while (err.InnerException != null) {
          err = err.InnerException;
        }
        MessageBox.Show(err.Message, "Screen Capture", 
			MessageBoxButtons.OK, MessageBoxIcon.Stop);
      } else if (e.Cancelled == true) {
      } else if (e.Result != null) {
        if (e.Result is Bitmap) {
          Bitmap bitmap = (Bitmap)e.Result;
          
              bitmap.Save(file);
             
              }
            }
          
        
      
    };
    worker.RunWorkerAsync();
  
}


        }

        private SaveFileDialog saveFileDialog1;

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            
            saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Comma separated (*.csv)|*.csv|Graph picture (*.jpg)|*.jpg";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileOk += saveFileDialog1_FileOk;
            saveFileDialog1.ShowDialog();


        }

        void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

            
            if (e.Cancel == true)
                return;

               

                var extension = Path.GetExtension(saveFileDialog1.FileName);

                switch (extension.ToLower())
                {
                    case ".jpg":

                       
                        // Hide the form so that it does not appear in the screenshot

                        try
                        {
                               
                         PrintScreen(saveFileDialog1.FileName);

                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message);
                        }
                       
                        break;
                    case ".csv":
                        //
                        // save_csv(saveFileDialog1.FileName);
                        //

                        string FilePath = saveFileDialog1.FileName;

                        
                        StreamWriter wr = new StreamWriter(FilePath);

                        // sw.WriteLine("Hello World!");


                        //string FilePath = txtFilename.Text;
                        string[] TotalData = new string[ch1_data.Length];
                        string[] ch1data = new string[ch1_data.Length];
                        string[] ch2data = new string[ch2_data.Length];
                        string[] timevector = new string[time_vector.Length];
                        for (int i = 0; i < ch1_data.Length; i++)
                        {
                            ch1data[i] = (ch1_data[i] * -1.0).ToString(CultureInfo.InvariantCulture);
                            ch2data[i] = (ch2_data[i] * -1.0).ToString(CultureInfo.InvariantCulture);
                            timevector[i] = (time_vector[i]).ToString(CultureInfo.InvariantCulture);
                            wr.WriteLine(ch1data[i] + "," + ch2data[i] + "," + timevector[i]);
                        }
                        
                        //File.WriteAllLines(FilePath, TotalData);
                        
                        wr.Close();
                        MessageBox.Show("CSV File Created Successfully", "Success");


                        break;
                }



            }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) // settings form
        {
           
            secondForm.Show();

        }


    }
}
