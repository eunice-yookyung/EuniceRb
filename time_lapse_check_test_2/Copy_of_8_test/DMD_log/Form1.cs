using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Diagnostics; // for using Trace

namespace DLPGUI
{
    public partial class DMD_Control_GUI : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();
        public DMD_Control_GUI()

        {
            InitializeComponent();
        }

        private DLP dlp;
        private System.Timers.Timer aTimer;
        private string last;


        private SampleFileSystemWatcher.DelayedFileSystemWatcher tracking_watcher2 = new SampleFileSystemWatcher.DelayedFileSystemWatcher();

        private Bitmap LoadRAW(string file)
        {
            Bitmap Bmp = new Bitmap(1024, 768);
            using (System.IO.BinaryReader binReader = new System.IO.BinaryReader(System.IO.File.Open(file, System.IO.FileMode.Open)))
            {
                byte nextbyte;
                for (int i = 0; i < 128 * 768; i++)
                {
                    nextbyte = binReader.ReadByte();
                    for (int j = 0; j < 8; j++)
                    {
                        if ((nextbyte & (1 << j)) != 0)
                            Bmp.SetPixel((i % 128) * 8 + j, (int)(i / 128), Color.White);
                        else
                            Bmp.SetPixel((i % 128) * 8 + j, (int)(i / 128), Color.Black);
                    }
                }
            }

            return Bmp;
        }

        private void updateImage(string file)
        {
            Image image = LoadRAW(file);

            Bitmap rotatedBmp = new Bitmap(1024, 768);

            Graphics g = Graphics.FromImage(rotatedBmp);
            g.TranslateTransform(-512, -384);
            g.RotateTransform(-45, System.Drawing.Drawing2D.MatrixOrder.Append);
            g.TranslateTransform(pictureBox1.Width / 2, pictureBox1.Height / 2, System.Drawing.Drawing2D.MatrixOrder.Append);
            g.DrawImage(image, new PointF(0, 0));
            
            // scale
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 500, 584 - 500), new Point(312 + 500, 584 + 500));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312-3, 584+3), new Point(312+3, 584-3));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 3+70, 584 + 3+70), new Point(312 + 3+70, 584 - 3+70));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 3+140, 584 + 3+140), new Point(312 + 3+140, 584 - 3+140));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 3 + 210, 584 + 3 + 210), new Point(312 + 3 + 210, 584 - 3 + 210));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 3 + 280, 584 + 3 + 280), new Point(312 + 3 + 280, 584 - 3 + 280));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 3 - 70, 584 + 3 - 70), new Point(312 + 3 - 70, 584 - 3 - 70));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 3 - 140, 584 + 3 - 140), new Point(312 + 3 - 140, 584 - 3 - 140));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 3 - 210, 584 + 3 - 210), new Point(312 + 3 - 210, 584 - 3 - 210));
            g.DrawLine(new Pen(Color.Red, 3), new Point(312 - 3 - 280, 584 + 3 - 280), new Point(312 + 3 - 280, 584 - 3 - 280));

            pictureBox1.Image = rotatedBmp;
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(new System.IO.FileStream("latest.log", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite));
        }

        //Initialize everything in form at startup
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void Form1_Load(object sender, EventArgs e)
        {
            //
            if (System.IO.File.Exists("console_log.txt")) 
                System.IO.File.Delete("console_log.txt");
            Trace.Listeners.Clear();
            TextWriterTraceListener twtl = new TextWriterTraceListener("console_log.txt");
            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;
            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            ctl.TraceOutputOptions = TraceOptions.DateTime;

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
            //Trace.WriteLine("The first line to be in the logfile and on the console.");


            dlp = new DLP();
            if (dlp.Status)
            {
                lStatus.Text = "DMD online";
                lStatus.ForeColor = Color.Green;
            }
            else
            {
                lStatus.Text = "DMD offline";
                lStatus.ForeColor = Color.Red;
            }

            updateImage("patterns\\black.raw");
            dlp.ShowList(new String[1] { "black" }, 0);

            // load gui settings
            if (System.IO.File.Exists("gui_settings.bin"))
            {
                using (System.IO.BinaryReader b = new System.IO.BinaryReader(System.IO.File.Open("gui_settings.bin", System.IO.FileMode.Open)))
                {
                    radioButton_lineChannel.Checked = b.ReadBoolean();
                    radioButton_fullChannel.Checked = !radioButton_lineChannel.Checked;
                    numericAlpha_pattern.Value = b.ReadDecimal();
                    numericFocus.Value = b.ReadDecimal();
                    numericScaledGradX_bare.Value = b.ReadDecimal();
                    numericScaledGradY_bare.Value = b.ReadDecimal();
                    numericShiftx_scaled.Value = b.ReadDecimal();
                    numericShifty_scaled.Value = b.ReadDecimal();
                    numericCov.Value = b.ReadDecimal();
                    ModeSelection.Value = b.ReadDecimal();
                    
                }
            }

            //function to update CurrentRunning text upon button click
            foreach (var button in Controls.OfType<Button>()) 
                if (!(button.Name==button_log.Name))
                    button.Click += updata_CurrentRunning;
            
            //
            initGlobalConstants();

        }
 
        private void initGlobalConstants()
        {
            // calibrated values
            numericAlpha_cal.Value = -39;//WAS -39
            numericScaledGradX_Cal.Value = (decimal)7;
            numericScaledGradY_Cal.Value = (decimal)-29;
            numericScaledGradPeriodCal.Value = (decimal)0.94; //this is actually important
            //numericFocus.Value = 0;//-20;//-35
            numericApert_global.Value = 700;//500
            
            //global params
            // Do not touch this - phillip
            dlp.GratingKVector = 0.4 * Math.PI;
            dlp.GratingAngle = Math.PI / 18;

            dlp.SetAlphaCal(Math.PI * ((double)numericAlpha_cal.Value) / 180);
            dlp.SetScaledGradPeriodCal((double)numericScaledGradPeriodCal.Value);
            dlp.SetScaledGradCal((double)numericScaledGradX_Cal.Value, (double)numericScaledGradY_Cal.Value);
            
            //set focus

            //set global aperture: 0 to disable
            dlp.GlobalAperture = (int)numericApert_global.Value;

            // pattern params
            dlp.SetAlphaPattern(0.0);
            dlp.SetApertureShift(0, 0);

            dlp.SetImageShift(0,0);
            dlp.TotalCoverage = 768;

            //dlp.GradX = scale((double)numericScaledGradX.Value);
            //dlp.GradY = scale((double)numericScaledGradY.Value);
            //dlp.SetApertureShift((int)numericShiftx.Value, (int)numericShifty.Value);
            if (radioButton_fullChannel.Checked)
            {
                dlp.Channel = 1;
            }
            else
                dlp.Channel = 0;
            
            //dlp.PatternAngle = Math.PI * (double)numericAlpha_pattern.Value / 180;
            //dlp.TotalCoverage = (int)numericCov.Value;

            // you can touch this:
            dlp.CircularAperture = true;
            dlp.PatternMethod = DLP.Method.Random;//Alex fix for Deterministic patterns
                                                  // in general want "Random". Could actually
                                                  // make a hybrid where you insert seed value for "Random"

            // is this guy fixed?
            dlp.NumberPatchesPerRow = 31;
            dlp.PatchDiameter = 23;
            dlp.TriggerEdge = 1;        // set to non-zero for rising edge, set to 0 for falling edge
            
            // set piezo voltages to 5! Keeps piezos flat at startup
            dlp.Voltage(5, 3);
            dlp.Voltage(5, 2);
            dlp.Voltage(5, 1);
            dlp.Voltage(5, 4);


        }

        // scale "micrometer" number into a phase information
        // the formula assumes the DMD is in the backfocal plane of the objective and ignores the optics up to that point, leading to a scaling error

        // doesn't seem to be used anywhere?
        /*
        private double scale(double value)
        {
            double lambda = 0.755;
            //L is length in mircon (I believe)
            double L = 500000;
            // M is magnification?
            double M = 1.0 / 70.0;
            //10.8 is the size of the mirrors in um
            return 2 * Math.PI * 10.8 * value / (lambda * L * M);
        }
        */
        // changes the angle property of the dlp object when the corresponding gui element is changed
        private void numericAlphaPattern_ValueChanged(object sender, EventArgs e)
        {
            //dlp.PatternAngle = Math.PI*Convert.ToDouble(numericAlpha_pattern.Value)/180;
        }

        // changes the tilt property of the dlp object when the corresponding gui element is changed
        private void numericScaledGradX_ValueChanged(object sender, EventArgs e)
        {
            //dlp.GradX = scale((double)numericScaledGradX.Value);
        }

        // changes the tilt property of the dlp object when the corresponding gui element is changed
        private void numericScaledGradY_ValueChanged(object sender, EventArgs e)
        {
            //dlp.GradY = scale((double)numericScaledGradY.Value);
        }

        // starts the calibration
        private void button_phase_cal_Click(object sender, EventArgs e)
        {
            //reminders
            MessageBox.Show("Trigger connected?");
            MessageBox.Show("Calibration will overwrite (l)phase0!");
            button_phase_cal.Text = "calibrating";
            
            //disable button
            button_phase_cal.Enabled = false;
            dlp.SetAlphaCal(Math.PI * ((double)numericAlpha_cal.Value) / 180);
            


            //to include defocus in phase cal:
            //precompensate (l)phase0 with defocus and use mode 1 in makeMapping to include defocus
            dlp.FlattenPhaseMaps(); //make (l)phase.bin flat
            MessageBox.Show("Maps flattened");
            //dlp.Flat2D(0.35);  
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);  //defocus (l)phase.bin

            MessageBox.Show("Maps defocused");
            dlp.PrecompPhaseMaps();//save (l)phase as (l)phase0, which now includes defocus for calibration
            MessageBox.Show("phase0 saved");

            //set parameters to get correct image plane shift
            dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);
            //set size of movable aperture :MNR 5/3/2016
            dlp.TotalCoverage = (int)numericCov.Value;


            // select mode for calibration: intermediate circular/line beam or line calibration on atoms
            int re = (comboBox_calMode.SelectedIndex < 2) ? dlp.PhaseCalibrate((int)num_averages.Value, comboBox_calMode.SelectedIndex) : dlp.LineCalibrate(7, 0.6);
            MessageBox.Show(re.ToString());
            // The new phase0 does not contain the pre-compensated defocus
            // Add defocus to the map
            
            dlp.ResetMaps();// set (l)phase equal to phase0
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);//defocus (l)phase
            dlp.PrecompPhaseMaps(); // save (l) phase as phase0, now including the defocus
            MessageBox.Show("Added defocus to phase0");

            // re-enable gui
            
            button_phase_cal.Enabled = true;
            button_phase_cal.Text = "phase cal";
        }

        private void button_amp_cal_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Trigger connected?");
            MessageBox.Show("There is no defocus applied here. Everything should be included in phase0 already");
            button_amp_cal.Text = "calibrating";
            // disable gui
            
            button_amp_cal.Enabled = false;

            dlp.SetAlphaCal(Math.PI * ((double)numericAlpha_cal.Value) / 180);
            //temporarily add defocus to (l)phase0 for calibration
            dlp.ResetMaps();
            //dlp.StoreTempMaps();
            // dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);//defocus (l)phase.bin
            //dlp.PrecompPhaseMaps();//save (l)phase as (l)phase0, which now includes defocus for calibration

            //set parameters to get correct image plane shift
            dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            
            dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);
            
            //set size of movable aperture: MNR 05/03/2016
            dlp.TotalCoverage = (int)numericCov.Value;

            // select mode for calibration: intermediate circular/line beam or line calibration on atoms
            int re = (comboBox_calMode.SelectedIndex < 2) ? dlp.AmpCalibrate((int)num_averages.Value, comboBox_calMode.SelectedIndex) : dlp.LineCalibrate(7, 0.6);

            //dlp.RestoreTempMaps();//restore the original (l)phase0, not including defocus
            MessageBox.Show(re.ToString());
            // re-enable gui
            button_amp_cal.Enabled = true;
            button_amp_cal.Text = "amp_cal";
        }
        // gui closing: save settings
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            dlp.Cancel();

            // save settings
            using (System.IO.BinaryWriter b = new System.IO.BinaryWriter(System.IO.File.Open("gui_settings.bin", System.IO.FileMode.Create)))
            {
                b.Write(radioButton_lineChannel.Checked);
                b.Write(numericAlpha_pattern.Value);
                b.Write(numericFocus.Value);
                b.Write(numericScaledGradX_bare.Value);
                b.Write(numericScaledGradY_bare.Value);
                b.Write(numericShiftx_scaled.Value);
                b.Write(numericShifty_scaled.Value);
                b.Write(numericCov.Value);
                b.Write(ModeSelection.Value);
                
            }
        }

        // create and display alignment grating
        private void button_alignment_Click(object sender, EventArgs e)
        {
            // update pattern params
            dlp.SetAlphaCal(Math.PI * ((double)numericAlpha_cal.Value) / 180);
            dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);
            dlp.TotalCoverage = (int)numericCov.Value;

            // remove any profiles from phase and amplitude maps
            dlp.ResetMaps();

            // apply defocus
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.ShowGrating((int)ModeSelection.Value);

            // show pattern on gui
            updateImage("patterns\\grating\\000.raw");

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
            foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();

            System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\alignment.raw", true);

            string[] list = Directory.GetFiles("patterns\\current\\").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            dlp.ShowList(list, 1.0);

         }

        // changes the aperture property of the dlp object when the corresponding gui element is changed
        // mnr: I don't think this should actually happen.... or at least maybe not.
        // I feel like when I clik buttons this thing should grab the variable
        /*
        private void numericCov_changed(object sender, EventArgs e)
        {
            dlp.TotalCoverage = (int)numericCov.Value;
        }
         */

        private void atomCalAlongLineButton_Click(object sender, EventArgs e)
        {

            // update pattern params
            dlp.SetAlphaCal(Math.PI * ((double)numericAlpha_cal.Value) / 180);
            dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);
            
            //Philipp P
            dlp.ResetMaps();
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, 0, 0);
            dlp.Flat2D(0.6);            // sets the average amplitude level to 60% maximum (everything that can not be brought to this level will just be maximized)
            //dlp.AdditionalAberrations();

            int no_images = 20; //round cal
            //int no_images = 10;//elliptical transv cal

            ////////// 27 is patch size, was 27+8 for round in the past, but we want smaller patches in Fourier plane
            //dlp.MakeLinePatterns(2, -1, 21, -600, no_images, -1);//smaller patches for elliptical beam
            //dlp.MakeLinePatterns(3, -1, 22, -600, no_images, -1);//elliptical 11/21/14
            dlp.MakeLinePatterns((int)ModeSelection.Value, -1, 27, -600, no_images, -1);//or size 27, or 31, was 22
            //dlp.MakeLinePatterns((int)ModeSelection.Value, -1, 27 + 8 * dlp.Channel, -600, no_images, -1);//or size 27, or 31, was 22
            //first argument: 0 no correction, 1 phase correction, 2 phase and ampitude, 3 additional aberrations. Patchsize 29 for calibration.
            //use the radio button in the GUI to select round or line illumination. makelinepatterns will use the according phase and amplitude map.(see dlp.Channel)

            string str = "";
            if (ModeSelection.Value == 0)
                str = "mapping\\";
            else if (ModeSelection.Value == 1)
                str = "mapping\\ph\\";
            else if (ModeSelection.Value > 1)
                str = "mapping\\ph,amp\\";

            string[] list = new string[no_images+1];
            list[0] = "gauss"; // "grating\\000";
            for (int i = 0; i < no_images; i++)
                list[i+1] = str + (i).ToString("D3");
            dlp.ShowListHack(list, 1.0);

        }

        // changes the illumination channel property of the dlp object when the corresponding gui element is changed
        // changes when checked. Not necessarily pulled when patterns are creates. This is probably the right thing.
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_fullChannel.Checked)
            {
                dlp.Channel = 1;
            }
            else
                dlp.Channel = 0;
        }

        //maps out amp profile in image plane by shifting around
        /*
        private void button10_Click(object sender, EventArgs e)
        {
            //display alignment patch at beginging to refresh dx dy
            //is necessary...
            button_alignment.PerformClick();

            //dlp.DX = 0;
            //dlp.DY = 0;

            double stepSize = 0.7;
            ushort NumberPatterns_1D = 10;
            int NumberPatterns_2D = NumberPatterns_1D * NumberPatterns_1D;

            dlp.ResetMaps(); //alex

            double xValue = (double)numericScaledGradX.Value;
            double yValue = (double)numericScaledGradY.Value;

            Trace.WriteLine("Start making patterns");

            for (int j = 0; j < NumberPatterns_1D; j++)
                for (int i = 0; i < NumberPatterns_1D; i++)
                {
                    int index = i + j * NumberPatterns_1D;

                    //    //dlp.GradX = scale(-1.2 + i * 0.2 * Math.Cos(-dlp.PatternAngle)); //for translating the line transverse to the long direction
                    //    //dlp.GradY = scale(1.2-i * 0.2 * Math.Sin(-dlp.PatternAngle));

                    numericScaledGradX.Value = (decimal)(xValue + (i- NumberPatterns_1D/2) * stepSize);
                    numericScaledGradY.Value = (decimal)(yValue + (j - NumberPatterns_1D / 2) * stepSize);
                    //dlp.GradX = scale((xValue + (i- NumberPatterns_1D/2) * stepSize));
                    //dlp.GradY = scale((yValue + (j - NumberPatterns_1D / 2) * stepSize));

                    //************************* here defines the pattern ************************
                    // dlp.PatternAngle -=  Math.PI / 180.0;
                    //dlp.MakeProfile("h1", (int)numericUpDown5.Value, "f", (int)numericUpDown6.Value, (int)numericUpDown9.Value, (int)numericUpDown10.Value, 100);
                    dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
                    dlp.ShowGrating(1);
                    //***************************************************************************

                    System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\measureProfile\\" + index.ToString("D3") + ".raw", true);
                    Trace.WriteLine(index);
                }

            Trace.WriteLine("Done");
           
            //int error = dlp.MeasureProfile(1, NumberPatterns_2D);
            
            //put the values back
            numericScaledGradX.Value = (decimal)xValue;
            numericScaledGradY.Value = (decimal)yValue;
            //dlp.GradX = scale((double)numericScaledGradX.Value);
            //dlp.GradY = scale((double)numericScaledGradY.Value);
            //display alignment patch at end
            button_alignment.PerformClick();

            MessageBox.Show("Profile Measurement Done");
            
        }
         */

        // changes the Fourier space shifting property of the dlp object when the corresponding gui element is changed
        //admmittedly this seems unnecessary
        /*
        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
        }
         * */

        // write settings into a log file
        private void button_log_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            //MessageBox.Show(folderBrowserDialog1.SelectedPath+"\\DMD_log");

            if (System.IO.Directory.Exists(folderBrowserDialog1.SelectedPath + "\\DMD_log") == true)
                MessageBox.Show("'DMD_log' already existed, logging aborted. Delete folder first.");
            else
            {
                System.IO.Directory.CreateDirectory(folderBrowserDialog1.SelectedPath + "\\DMD_log");
                System.IO.Directory.CreateDirectory(folderBrowserDialog1.SelectedPath + "\\DMD_log\\current");
                
                System.IO.File.Copy(System.IO.Directory.GetParent(".").Parent.FullName + "\\Form1.cs", folderBrowserDialog1.SelectedPath + "\\DMD_log\\Form1.cs");
                System.IO.File.Copy("console_log.txt", folderBrowserDialog1.SelectedPath + "\\DMD_log\\console_log.txt");
                //System.IO.File.Copy("arb_sequence.txt", folderBrowserDialog1.SelectedPath + "\\DMD_log\\arb_sequence.txt");
                System.IO.File.Copy("arb_sequence.txt", folderBrowserDialog1.SelectedPath + "\\DMD_log\\" + DateTime.Now.ToString("yyyyMMdd") + "arb_sequence.txt");

                // copy the current patterns
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
                foreach (System.IO.FileInfo file in di.GetFiles()) System.IO.File.Copy(file.FullName, folderBrowserDialog1.SelectedPath + "\\DMD_log\\current\\" + file.Name, true);

                using (var bmp = new Bitmap(this.Width, this.Height))
                {
                    this.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    //bmp.Save(@"c:\temp\screenshot.png");
                    bmp.Save(folderBrowserDialog1.SelectedPath + "\\DMD_log\\DMD_param.png");
                }

                //System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\measureProfile\\" + index.ToString("D3") + ".raw", true);
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(folderBrowserDialog1.SelectedPath + "\\DMD_log\\DMD_params.txt", false))
                {
                    file.WriteLine("GENERAL");
                    file.WriteLine("illumination mode " + (radioButton_lineChannel.Checked ? "line" : "full"));
                    file.WriteLine("pattern angle " + (double)numericAlpha_pattern.Value);
                    file.WriteLine("defocus " + (double)numericFocus.Value);
                    file.WriteLine("image plane displacement x " + (double)numericScaledGradX.Value);
                    file.WriteLine("image plane displacement y " + (double)numericScaledGradY.Value);
                    file.WriteLine("aperture " + (double)numericCov.Value);
                    file.WriteLine("Fourier plane displacement x " + (double)numericShiftx_scaled.Value);
                    file.WriteLine("Fourier plane displacement y " + (double)numericShifty_scaled.Value);
                    file.WriteLine("arb map file: " + textBox_arb_map.Text);
                    file.WriteLine("");
                    file.WriteLine("Currently " + label_CurrentRunning.Text);

                }
            }
        }

        private void button_arb_map_Click(object sender, EventArgs e)
        {

            // update pattern params
            dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);

            dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);
            dlp.TotalCoverage = (int)numericCov.Value;

            generate_arb_map(textBox_arb_map.Text.Trim());

            // clear the folder 'current', copy the current file over, and display in GUI
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
            foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();

            System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\" + textBox_arb_map.Text.Trim() + "X" + numericScaledGradX_bare.Value.ToString().Replace(".", "p") + "Y" + numericScaledGradY_bare.Value.ToString().Replace(".", "p") + ".raw", true);

            string[] list = Directory.GetFiles("patterns\\current\\").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            dlp.ShowList(list, 1.0);
            updateImage("patterns\\current\\" + textBox_arb_map.Text.Trim() + "X" + numericScaledGradX_bare.Value.ToString().Replace(".", "p") + "Y" + numericScaledGradY_bare.Value.ToString().Replace(".", "p") + ".raw");

        }

        private void generate_arb_map(string template_name)
        {
            // 8/14/15 AL update: update of pattern parameters now is outside the function some old buttons might missbehave
            // necessary
            dlp.ResetMaps();


            // 05/03/16 MNR update: in principle I'm not sure this numericCov should be hard coded. But free to be bigger if necessary.
            //numericCov.Value = 600;
            numericShiftx_scaled.Value = 0;
            numericShifty_scaled.Value = 0;



            // only the first argument matters
            // this "f" means "flat top" but doesn't actually do anything
            dlp.MakeProfile("t" + template_name, 0, "f", 0, 0);

            //// apply defocus
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            //// create and upload pattern
            dlp.ShowGrating((int)ModeSelection.Value);

            // force garbage collection
            GC.Collect();

        }

        // in principle this could be avoided if we just grab this vaue every time we click a button
        private void numericApert_global_ValueChanged(object sender, EventArgs e)
        {
            dlp.GlobalAperture = (int)numericApert_global.Value;
            numericApert_global.Value = dlp.GlobalAperture;

        }

        private void updata_CurrentRunning(object sender, EventArgs eventArgs)
        {
            label_CurrentRunning.Text = "Running: " + ((Button)sender).Name.ToString();

        }

        private void buttonDisplayRaw_Click(object sender, EventArgs e)
        {

                //openFileDialog1.InitialDirectory = "patterns\\";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    // clear the folder 'current', copy the current file over, and display in GUI
                    //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
                    //foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();

                    try{
                        dlp.ShowSpecificFile(openFileDialog1.FileName);
                        updateImage(openFileDialog1.FileName);
                    }
                    catch (Exception ex){
                        MessageBox.Show("Loading RAW pattern failed.");
                    }
                }

        }

    /*
        private void line_shift_cal_Click(object sender, EventArgs e)
        {
            //Runs the line shift for both a horizontal and vertical line
            //Use listLen points for each axis, plus one alignment patch in the beginning
            //Total number of patterns is 2*listLen+1
            Trace.WriteLine("Running pattern Shift Calibration...");
            Trace.WriteLine(" ");

            int listLen = 10;

            // clear the folder 'current', copy the current file over, and display in GUI
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
            foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();

            //string[] list = new string[2*listLen+1];
            string[] list = new string[listLen + 1];
            Trace.WriteLine("first");
            button_alignment.PerformClick();
            System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\first.raw", true);
            list[0] = "first";

            for (int i = 0; i < (listLen); i++)
            {
                list[i + 1] = i.ToString("D3");

                numericScaledGradX.Value = (decimal)(19);
                numericScaledGradY.Value = (decimal)(19.2 + i*0.1);
                //textBox_arb_map.Text = "hor_wall_smoothbox"; //"hgaussline_vert0p9x12_shift0_new";

                Trace.WriteLine(i.ToString("D3"));

                generate_arb_map("hor_wall_smoothbox");

                System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\" + i.ToString("D3") + ".raw", true);
            }

            //for (int i = listLen; i < (2 * listLen); i++)
            //{
            //    list[i + 1] = i.ToString("D3");

            //    numericScaledGradX.Value = (decimal)(5);
            //    numericScaledGradY.Value = (decimal)(5.8 + (i - listLen) * 0.15);
            //    textBox_arb_map.Text = "hor_hgaussline_0p9x14_sigwidth0p32"; //"hgaussline_hor0p9x12_new";

            //    Trace.WriteLine(i.ToString("D3"));

            //    generate_arb_map();

            //    System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\" + i.ToString("D3") + ".raw", true);
            //}z

            MessageBox.Show("Ready? Check Significant value!");
            dlp.ShowList(list, 1.0);
        }
     * */


        private void TrackingOffButton_CheckedChanged(object sender, EventArgs e)
        {
            if (TrackingOffButton.Checked)
            {
                dlp.TrackingOn = false;
                StopTracking();
            }
            else
            {
                dlp.TrackingOn = true;
                StartTracking();
            }
            
        }

        private void StartTracking()
        {
            string log_pathname = "C:\\Users\\Rubidium4\\Documents\\DMD_tracking"; //"W:\\Data\\DMD_tracking";
            string log_filename = "dmd_tracking_current_combine.txt";  
            string log_file = log_pathname + "\\" + log_filename;

            //erase history if there is one (really just initialize in this case)
            dlp.tracking_shift_x.RemoveRange(0, dlp.tracking_shift_x.Count());
            dlp.tracking_shift_y.RemoveRange(0, dlp.tracking_shift_y.Count());
            dlp.tracking_shift_x_hor.RemoveRange(0, dlp.tracking_shift_x_hor.Count());
            dlp.tracking_shift_y_hor.RemoveRange(0, dlp.tracking_shift_y_hor.Count());
            //long lastWriteTime_ms = 0; // File.GetLastWriteTime(log_file).Ticks / TimeSpan.TicksPerMillisecond;
            label_lastTrackTime.Text = DateTime.Now.ToString(); // File.GetLastWriteTime(log_file).ToString(); DateTime.Now.ToString();
            tracking_watcher2.Path = log_pathname;
            tracking_watcher2.NotifyFilter = NotifyFilters.LastWrite;
            tracking_watcher2.Filter = log_filename;

            // Add event handlers.
            //tracking_watcher2.Changed += new FileSystemEventHandler((sender2, e2) => OnTrackingFileChanged(sender2, e2, log_file));
            tracking_watcher2.Changed += new FileSystemEventHandler((sender2, e2) => OnTrackingFileChanged(sender2, e2, log_file));
            // Begin watching.
            tracking_watcher2.EnableRaisingEvents = true;
            //Initialize the piezo error correction at zero
            piezo_err_x.Value = 0;
            piezo_err_y.Value = 0;
            piezo_err_x_hor.Value = 0;
            piezo_err_y_hor.Value = 0;

            //Initialize set point history at zero
            int i = 0;
            for (i = 0; i < 3; i++)
            {
                dlp.piezo_hist_x[i] = 0;
                dlp.piezo_hist_y[i] = 0;
                dlp.piezo_hist_x_hor[i] = 0;
                dlp.piezo_hist_y_hor[i] = 0;
            }
            //Console.WriteLine("Piezo x err hist (" + dlp.piezo_hist_x[0].ToString() + ", " + dlp.piezo_hist_x[1].ToString() + ", " + dlp.piezo_hist_x[2].ToString() + ") - " + File.GetLastWriteTime(log_file).ToString());
            //Console.WriteLine("Piezo y err hist (" + dlp.piezo_hist_y[0].ToString() + ", " + dlp.piezo_hist_y[1].ToString() + ", " + dlp.piezo_hist_y[2].ToString() + ") - " + File.GetLastWriteTime(log_file).ToString());
    
        }

        private void StopTracking()
        {
            // Stop watching.
            tracking_watcher2.EnableRaisingEvents = false;
            //Set the piezo error correction to zero
            piezo_err_x.Value = 0;
            piezo_err_y.Value = 0;
            piezo_err_x_hor.Value = 0;
            piezo_err_y_hor.Value = 0;

            //Reset piezo history at zero
            int i = 0;
            for (i = 0; i < 3; i++)
            {
                dlp.piezo_hist_x[i] = 0;
                dlp.piezo_hist_y[i] = 0;
                dlp.piezo_hist_x_hor[i] = 0;
                dlp.piezo_hist_y_hor[i] = 0;
            }

            Piezo_shift.PerformClick();
        }

        private void OnTrackingFileChanged(object source, FileSystemEventArgs e, string log_file)
        {
            try
            {
                // extra hack to avoid duplicates from FileSystemWatcher
                //tracking_watcher.EnableRaisingEvents = false;
                tracking_watcher2.EnableRaisingEvents = false;
                DateTime timeNow = DateTime.Now; // File.GetLastWriteTime(log_file); // DateTime.Now;
                DateTime convertedTime;
                convertedTime = Convert.ToDateTime(label_lastTrackTime.Text);
                if (((timeNow.Ticks - convertedTime.Ticks) / TimeSpan.TicksPerMillisecond) > 5000)
                {

                    //// a little time so that matlab can't be writing
                    System.Threading.Thread.Sleep(50);

                    string[] tracking_shifts = System.IO.File.ReadAllText(log_file).Trim().Split(',');
                    double tracking_shift_x = Convert.ToDouble(tracking_shifts[0].Trim());
                    double tracking_shift_y = Convert.ToDouble(tracking_shifts[1].Trim());
                    double tracking_shift_x_hor = Convert.ToDouble(tracking_shifts[2].Trim());
                    double tracking_shift_y_hor = Convert.ToDouble(tracking_shifts[3].Trim());

                    Console.WriteLine("New shifts for DMD tracking" + File.GetLastWriteTime(log_file).ToString() + ":" + "\n" +
                                      "vertical :" + "(" + tracking_shift_x.ToString() + ", " + tracking_shift_y.ToString() + ")" + "\n" +
                                      "horzontal :" + "(" + tracking_shift_x_hor.ToString() + ", " + tracking_shift_y_hor.ToString() + ")" + "\n");



                    //Maintain a history of the most recent set points
                    int i = 0;
                    for (i = 0; i < 2; i++)
                    {
                        dlp.piezo_hist_x[i] = dlp.piezo_hist_x[i + 1];
                        dlp.piezo_hist_y[i] = dlp.piezo_hist_y[i + 1];
                        dlp.piezo_hist_x_hor[i] = dlp.piezo_hist_x_hor[i + 1];
                        dlp.piezo_hist_y_hor[i] = dlp.piezo_hist_y_hor[i + 1];
                    }

                    dlp.piezo_hist_x[2] = (double)piezo_shift_x.Value - tracking_shift_x - Math.Round((double)piezo_shift_x.Value - tracking_shift_x);
                    dlp.piezo_hist_y[2] = (double)piezo_shift_y.Value - tracking_shift_y - Math.Round((double)piezo_shift_y.Value - tracking_shift_y);
                    dlp.piezo_hist_x_hor[2] = (double)piezo_shift_x_hor.Value - tracking_shift_x_hor - Math.Round((double)piezo_shift_x_hor.Value - tracking_shift_x_hor);
                    dlp.piezo_hist_y_hor[2] = (double)piezo_shift_y_hor.Value - tracking_shift_y_hor - Math.Round((double)piezo_shift_y_hor.Value - tracking_shift_y_hor);

                    double piezo_avg_x = 0;
                    double piezo_avg_y = 0;
                    double piezo_avg_x_hor = 0;
                    double piezo_avg_y_hor = 0;

                    int p = 0;
                    for (p = 0; p < 3; p++)
                    {
                        piezo_avg_x = piezo_avg_x + dlp.piezo_hist_x[p];
                        piezo_avg_y = piezo_avg_y + dlp.piezo_hist_y[p];
                        piezo_avg_x_hor = piezo_avg_x_hor + dlp.piezo_hist_x_hor[p];
                        piezo_avg_y_hor = piezo_avg_y_hor + dlp.piezo_hist_y_hor[p];
                    }

                    piezo_avg_x = piezo_avg_x / dlp.piezo_hist_x.Length;
                    piezo_avg_y = piezo_avg_y / dlp.piezo_hist_y.Length;
                    piezo_avg_x_hor = piezo_avg_x_hor / dlp.piezo_hist_x_hor.Length;
                    piezo_avg_y_hor = piezo_avg_y_hor / dlp.piezo_hist_y_hor.Length;

                    try
                    {

                        this.Invoke((MethodInvoker)delegate // BeginInvoke seems to give double events??
                        {
                            label_lastTrackTime.Text = timeNow.ToString();

                            //Console.WriteLine("Piezo x err hist (" + dlp.piezo_hist_x[0].ToString() + ", " + dlp.piezo_hist_x[1].ToString() + ", "  + dlp.piezo_hist_x[2].ToString() + ") - " + File.GetLastWriteTime(log_file).ToString());
                            //Console.WriteLine("Piezo y err hist (" + dlp.piezo_hist_y[0].ToString() + ", " + dlp.piezo_hist_y[1].ToString() + ", " + dlp.piezo_hist_y[2].ToString() + ") - " + File.GetLastWriteTime(log_file).ToString());
                            Console.WriteLine("Avg piezo error x,y " + File.GetLastWriteTime(log_file).ToString() + ":" + "\n" +
                                                "vertical :" + "(" + piezo_avg_x.ToString() + ", " + piezo_avg_y.ToString() + ")" + "\n" +
                                                "horzontal :" + "(" + piezo_avg_x_hor.ToString() + ", " + piezo_avg_y_hor.ToString() + ")");
                            //The feedback only takes care of corrections < 1 site. 
                            //Matlab tracking should return the current raw shift between dot and lattice, not averaged.

                            double p_gain = 0.5;// 0.5 ////See mathematica notebook in Z:/Calculations/DMD_related to play with these numbers
                            double i_gain = 0.5;
                            piezo_err_x.Value = piezo_err_x.Value + (decimal) i_gain * (decimal)piezo_avg_x + (decimal)p_gain * (decimal)dlp.piezo_hist_x[2];
                            piezo_err_y.Value = piezo_err_y.Value + (decimal) i_gain * (decimal)piezo_avg_y + (decimal)p_gain * (decimal)dlp.piezo_hist_y[2];
                            piezo_err_x_hor.Value = piezo_err_x_hor.Value + (decimal)i_gain * (decimal)piezo_avg_x_hor + (decimal)p_gain * (decimal)dlp.piezo_hist_x_hor[2];
                            piezo_err_y_hor.Value = piezo_err_y_hor.Value + (decimal)i_gain * (decimal)piezo_avg_y_hor + (decimal)p_gain * (decimal)dlp.piezo_hist_y_hor[2];

                            Piezo_shift_avg();

                        });
                    }
                    catch
                    {
                    }

                };

            }
            
            finally
            {
                // extra hack to avoid duplicates from FileSystemWatcher
                //tracking_watcher.EnableRaisingEvents = true;
                tracking_watcher2.EnableRaisingEvents = true;
            }
        }

        ////old tracking routine 
        private void numericTrackingX_ValueChanged(object sender, EventArgs e) { updateScaledGradShifts(); }
        private void numericTrackingY_ValueChanged(object sender, EventArgs e) { updateScaledGradShifts(); }
        private void numericScaledGradX_bare_ValueChanged(object sender, EventArgs e) { updateScaledGradShifts(); }
        private void numericScaledGradY_bare_ValueChanged(object sender, EventArgs e) { updateScaledGradShifts(); }

        private void updateScaledGradShifts()
        {
            numericScaledGradX.Value = numericScaledGradX_bare.Value + numericTrackingX.Value;
            numericScaledGradY.Value = numericScaledGradY_bare.Value + numericTrackingY.Value;
        }

        private void button_runTracked_Click(object sender, EventArgs e)
        {
           

            // clear the folder 'current', copy the current file over, and display in GUI
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
            foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();

            string[] arbmap_names = System.IO.File.ReadAllLines(@"arb_sequence.txt");
            int num = arbmap_names.Length;

            decimal tempx = numericScaledGradX_bare.Value;
            decimal tempy = numericScaledGradY_bare.Value;

            for (int j = 0; j < num; j++)
            {
                int fileExtPos = arbmap_names[j].LastIndexOf(".");
                dlp.Channel = Convert.ToInt32(arbmap_names[j].Substring(fileExtPos+1, 1));             

                dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);//PMP 08/27/2015 now need to set IP position outside "generate_arb_map"

                generate_arb_map(arbmap_names[j].Substring(0, fileExtPos));
                System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\" + j.ToString("D3") + arbmap_names[j].Substring(0,fileExtPos - 1) + "X" + numericScaledGradX_bare.Value.ToString().Replace(".", "p") + "Y" + numericScaledGradY_bare.Value.ToString().Replace(".", "p") + ".raw", true);
               //list[j] = "arb_seq" + j.ToString("D3");
            }

            numericScaledGradX_bare.Value = (decimal)(tempx);
            numericScaledGradY_bare.Value = (decimal)(tempy);

            System.IO.File.Copy("patterns\\tracking_vert.raw", "patterns\\current\\tracking_vert.raw", true);
                                   
            string[] list = Directory.GetFiles("patterns\\current\\").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            dlp.ShowList(list, 1.0);

            // do stuff end here
        }

        private void buttonDisplayPattern_Click(object sender, EventArgs e)
        {
            if (comboBox_atomCalMode.SelectedIndex == 0)
            {
                dlp.ShowSpecificFile("patterns\\mapping\\" + ((int)numeric_patternNumber.Value).ToString("D3") + ".raw");
                updateImage("patterns\\mapping\\" + ((int)numeric_patternNumber.Value).ToString("D3") + ".raw");
            }

            else if (comboBox_atomCalMode.SelectedIndex == 1)
            {
                dlp.ShowSpecificFile("patterns\\mapping\\ph\\" + ((int)numeric_patternNumber.Value).ToString("D3") + ".raw");
                updateImage("patterns\\mapping\\ph\\" + ((int)numeric_patternNumber.Value).ToString("D3") + ".raw");
            }

            else if (comboBox_atomCalMode.SelectedIndex == 2)
            {
                dlp.ShowSpecificFile("patterns\\mapping\\ph,amp\\" + ((int)numeric_patternNumber.Value).ToString("D3") + ".raw");
                updateImage("patterns\\mapping\\ph,amp\\" + ((int)numeric_patternNumber.Value).ToString("D3") + ".raw");
            }

            else
            {
                MessageBox.Show("you are asking for too much! check parameters");
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            dlp.Scan((int)ModeSelection.Value, (int)Xres.Value, (int)Yres.Value, (double)scan_length.Value, (int)nrep.Value, "test.bin");
        }

        private void Pattern_change_Click(object sender, EventArgs e)
        {
            // update pattern params
            dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);
            dlp.TotalCoverage = (int)numericCov.Value;

            generate_arb_map(textBox_arb_map.Text.Trim());

            //// clear the folder 'current', copy the current file over, and display in GUI
            //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
            //foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();

            string[] listint = Directory.GetFiles("patterns\\current\\").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            System.IO.File.Delete("patterns\\current\\" + listint[(int)pattern_for_change.Value - 1] + ".raw");
            System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\" + ((int)pattern_for_change.Value - 1).ToString("D3") + textBox_arb_map.Text.Trim() + "X" + numericScaledGradX_bare.Value.ToString().Replace(".", "p") + "Y" + numericScaledGradY_bare.Value.ToString().Replace(".", "p") + ".raw", true);

            string[] list = Directory.GetFiles("patterns\\current\\").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            dlp.ShowList(list, 1.0);

        }
        //talks to the NI Box to change the piezo voltages
        private void Output_Voltage_Ch1_Click(object sender, EventArgs e)
        {
            dlp.Voltage((double)Ch1_Voltage.Value, 1);
            //see s to help for some reason when sweeping large movement
            System.Threading.Thread.Sleep(1000);
            dlp.Voltage((double)Ch1_Voltage.Value, 1);
        }

        //talks to the NI Box to change the piezo voltages
        private void Output_Voltage_Ch2_Click(object sender, EventArgs e)
        {
           dlp.Voltage((double)Ch2_Voltage.Value, 2);
            //see s to help for some reason when sweeping large movement
            System.Threading.Thread.Sleep(1000);
            dlp.Voltage((double)Ch2_Voltage.Value, 2);
        }
        //talks to the NI Box to change the piezo voltages
        private void Output_Voltage_Ch3_Click(object sender, EventArgs e)
        {
            dlp.Voltage((double)Ch3_Voltage.Value, 3);
            //seems to help for some reason when sweeping large movement
            System.Threading.Thread.Sleep(1000);
            dlp.Voltage((double)Ch3_Voltage.Value, 3);

        }

        //talks to the NI Box to change the piezo voltages
        private void Output_Voltage_Ch4_Click(object sender, EventArgs e)
        {
            dlp.Voltage((double)Ch4_Voltage.Value, 4);
            //see s to help for some reason when sweeping large movement
            System.Threading.Thread.Sleep(1000);
            dlp.Voltage((double)Ch4_Voltage.Value, 4);
        }


        private void Piezo_shift_Click(object sender, EventArgs e)
        {
            Piezo_shift_avg();
        }

        // Thiss no longer seems like it does averaging. But does look at the  offsets and calls piezo_convert
        public void Piezo_shift_avg()
        {
            // apply the piezo voltages. If tracking is on, V_piezo = V_setpoint + V_error (i.e. feed forward and feed back)
            double[] Vs = new double[4];

            Vs = Piezo_convert((double)piezo_shift_x.Value + (double)piezo_err_x.Value, (double)piezo_shift_y.Value + (double)piezo_err_y.Value,
                               (double)piezo_shift_x_hor.Value + (double)piezo_err_x_hor.Value, (double)piezo_shift_y_hor.Value + (double)piezo_err_y_hor.Value);

            if ((0 < Vs[0]) && (Vs[0] < 10) && (0 < Vs[1]) && (Vs[1] < 10))
            {
                dlp.Voltage(Vs[0], 2);
                dlp.Voltage(Vs[1], 3);
                dlp.Voltage(Vs[2], 1);
                dlp.Voltage(Vs[3], 4);
                //again, some magic with sending the voltage twice sometimes corrects a small offset
                System.Threading.Thread.Sleep(1000);
                dlp.Voltage(Vs[0], 2);
                dlp.Voltage(Vs[1], 3);
                dlp.Voltage(Vs[2], 1);
                dlp.Voltage(Vs[3], 4);
                Console.WriteLine("Channel 1 voltage (Vx): " + Vs[0].ToString());
                Console.WriteLine("Channel 2 voltage (Vy): " + Vs[1].ToString());
                Console.WriteLine("Channel 3 voltage (Vx): " + Vs[2].ToString());
                Console.WriteLine("Channel 4 voltage (Vy): " + Vs[3].ToString());
                Console.WriteLine(" ");
            }
            else
            {
                Console.WriteLine("Channel 1 voltage (Vx): " + Vs[0].ToString());
                Console.WriteLine("Channel 2 voltage (Vy): " + Vs[1].ToString());
                Console.WriteLine("Channel 3 voltage (Vx): " + Vs[2].ToString());
                Console.WriteLine("Channel 4 voltage (Vy): " + Vs[3].ToString());
                MessageBox.Show("One of the voltages exeeds range");
            }
        
        }

        // this is where the piezo calibrations are put in
        private double[] Piezo_convert(double x, double y, double x_hor, double y_hor)
        {
            
            // convert sites to piezo voltages
            double[] Vs = new double[4];
            // these values came from andor calibration

            double Vx = -0.00023 * Math.Pow(x, 4) -0.00195 * Math.Pow(x, 3) +0.027 * Math.Pow(x, 2) -0.735 * x;
            double Vy = -0.0000636 * Math.Pow(y, 4) -0.00058 * Math.Pow(y, 3) +0.00115 * Math.Pow(y, 2) -0.5275 * y;
            double Vx_hor = -0.0063 * Math.Pow(x_hor, 4) -0.0131 * Math.Pow(x_hor, 3) +0.1275 * Math.Pow(x_hor, 2) -1.575 * x_hor;
            double Vy_hor = -0.00146 * Math.Pow(y_hor, 4) +0.00664 * Math.Pow(y_hor, 3) +0.0645 * Math.Pow(y_hor, 2) +1.12 * y_hor;
            
            Vs[0] = Vx + 5.0;
            Vs[1] = Vy + 5.0;
            Vs[2] = Vx_hor + 5.0;
            Vs[3] = Vy_hor + 5.0;
           
            return Vs;
            
            
        }

        private void make_gauss_button_Click(object sender, EventArgs e)
        {
            //
            dlp.ResetMaps();

            // update pattern params
            dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);
            dlp.TotalCoverage = (int)numericCov.Value;

            // create phase and amplitude map
            //dlp.MakeProfile("h1", (int)numericUpDown5.Value, "f", (int)numericUpDown6.Value, (int)numericSigWidth.Value);

            //-100 sig width makes the sig width twice the aperture size? idk why this is the case or is necessary. seems a little funny
             dlp.MakeProfile("g", (int)numericGaussWidth.Value, "g", (int)numericGaussWidth.Value, -100);
            //dlp.MakeProfile("g", (int)numericGaussWidth.Value, "g", (int)numericGaussWidth.Value, 100);

            //dlp.MakeProfile("tflat", (int)numericUpDown5.Value, "f", (int)numericUpDown6.Value, (int)numericUpDown9.Value, (int)numericUpDown10.Value, 800);
            // apply defocus
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            // create and upload pattern
            dlp.ShowGrating((int)ModeSelection.Value);

            // show pattern on gui
            updateImage("patterns\\grating\\000.raw");
        }

        private void upload_folder_Click(object sender, EventArgs e)
        {
            string[] list = Directory.GetFiles("patterns\\current\\").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            dlp.ShowList(list, 1.0);
        }

        private void atomLineCalTrackButton_Click(object sender, EventArgs e)
        {
            // clear the folder 'current', copy the current file over, and display in GUI
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
            foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();

            // update pattern params
            dlp.SetAlphaCal(Math.PI * ((double)numericAlpha_cal.Value) / 180);
            dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
            dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.SetImageShift((double)numericScaledGradX.Value, (double)numericScaledGradY.Value);

            //Philipp P
            dlp.ResetMaps();
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, 0, 0);
            dlp.Flat2D(0.6);            // sets the average amplitude level to 60% maximum (everything that can not be brought to this level will just be maximized)
            //dlp.AdditionalAberrations();

            int no_images = 20; //round cal
            //int no_images = 10;//elliptical transv cal

            //dlp.MakeLinePatterns(2, -1, 21, -600, no_images, -1);//smaller patches for elliptical beam
            //dlp.MakeLinePatterns(3, -1, 22, -600, no_images, -1);//elliptical 11/21/14
            //dlp.MakeLinePatterns((int)ModeSelection.Value, -1, 27, -600, no_images, -1);//or size 27, or 31, was 22
            dlp.MakeLinePatterns((int)ModeSelection.Value, -1, 27 + 8 * dlp.Channel, -600, no_images, -1);//or size 27, or 31, was 22 2025/05/26 consistent with old DMD1 
            //first argument: 0 no correction, 1 phase correction, 2 phase and ampitude, 3 additional aberrations. Patchsize 29 for calibration.
            //use the radio button in the GUI to select round or line illumination. makelinepatterns will use the according phase and amplitude map.(see dlp.Channel)

            // add hole
            dlp.ResetMaps();
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.ShowGrating((int)ModeSelection.Value);

            for (int j = 0; j < no_images; j++)
            {
                System.IO.File.Copy("patterns\\mapping\\ph,amp\\" + (j).ToString("D3") + ".raw", "patterns\\current\\" + j.ToString("D3") + "fringe" + ".raw", true);
                System.IO.File.Copy("patterns\\tracking_vert.raw", "patterns\\current\\" + j.ToString("D3") + "tracking_vert" + ".raw", true);
            }

            System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\0000gauss.raw", true);
            System.IO.File.Copy("patterns\\tracking_vert.raw", "patterns\\current\\0000tracking_vert.raw", true);


            //upload current
            string[] list = Directory.GetFiles("patterns\\current\\").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            dlp.ShowList(list, 1.0);

        }

        private void arb_map_DMD_shift_Click(object sender, EventArgs e)
        {
            // clear the folder 'current', copy the current file over, and display in GUI
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo("patterns\\current");
            foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();

            // add hole
            dlp.ResetMaps();
            dlp.Defocus((double)numericFocus.Value, (double)numericFocus.Value, (int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
            dlp.ShowGrating((int)ModeSelection.Value);

            System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\000gauss.raw", true);
            System.IO.File.Copy("patterns\\tracking_round.raw", "patterns\\current\\000tracking.raw", true);

            for (int j = 0; j < (int)arb_map_shift_number.Value; j++)
            {
                // update pattern params
                dlp.SetAlphaPattern((double)numericAlpha_pattern.Value / 180 * Math.PI);
                dlp.SetApertureShift((int)numericShiftx_scaled.Value, (int)numericShifty_scaled.Value);
                dlp.SetImageShift((double)numericScaledGradX.Value + j * (double)arb_map_shift_value.Value, (double)numericScaledGradY.Value);
                dlp.TotalCoverage = (int)numericCov.Value;

                generate_arb_map(textBox_arb_map.Text.Trim());

                System.IO.File.Copy("patterns\\grating\\000.raw", "patterns\\current\\" + ((j+1)*10).ToString("D3") + textBox_arb_map.Text.Trim() + "X" + ((double)numericScaledGradX.Value + j * (double)arb_map_shift_value.Value).ToString().Replace(".", "p") + "Y" + numericScaledGradY_bare.Value.ToString().Replace(".", "p") + ".raw", true);
                System.IO.File.Copy("patterns\\tracking_round.raw", "patterns\\current\\" + ((j+1)*10 + 1).ToString("D3") + "round_dmd_tracking" + ".raw", true);
            }

            string[] list = Directory.GetFiles("patterns\\current\\").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            dlp.ShowList(list, 1.0);
        }
      
    }
}
