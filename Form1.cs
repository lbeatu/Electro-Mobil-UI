using Syncfusion.Windows.Forms.Gauge;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.IO.Ports;

namespace cartest2
{


    public partial class Form1: Form
    {
        
        public static Queue RXQ = new Queue();
        private int RXBytes = 0;
        private int TXBytes = 0;
        public Thread QProcessorThread;
        public Thread SerialReaderThread;
        public Boolean Connected = false;
        long maksm = 30, mimm = 0;
        public  int[] batteryLevels = new int[24];
        public  string[] batteryTemps = new string[24];
        public Graphics g;
        public int Speed = 0;
        public double TotalVoltage = 0;
        public int MaxTemp = 0;
        public double CurrentCurrent = 0;
        //public int ChronoTime = 0;
       // public int LapTime = 0;
        //public int ChronoStatus = 0;
        //public int CurrentLap = 1;
       // public int[] Laptimes = new int[31];
        public string[] BatteryPercent = new string[24];              
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2; 
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();


        public Form1()
        {
            InitializeComponent();
            UserInitialization();
            QProcessorThread = new Thread(new ThreadStart(RXProcessor));
            SerialReaderThread = new Thread(new ThreadStart(SerialReader));
            SerialReaderThread.Start();
            QProcessorThread.Start();
            Debug.WriteLine("-------------------------THREAD-RXProcessor-SerialReader Başlatıldı.--------------------------------------------");
            Debug.WriteLine("-------------------------TURKMEKATRONİK.--------------------------------------------");

            //TODO Radial Guage
            RadialGauge radialGauge1 = new RadialGauge();
            this.radialGauge1.MinorDifference = 5;
            this.Controls.Add(radialGauge1);
            this.radialGauge1.VisualStyle = Syncfusion.Windows.Forms.Gauge.ThemeStyle.Black;
            this.radialGauge1.MajorDifference = 10F;
            this.radialGauge1.MaximumValue = 150F;
            this.radialGauge1.MinimumValue = 0F;
            this.radialGauge1.MinorDifference = 30F;
            this.radialGauge1.LabelPlacement = Syncfusion.Windows.Forms.Gauge.LabelPlacement.Outside;
            this.radialGauge1.TextOrientation = Syncfusion.Windows.Forms.Gauge.TextOrientation.SlideOver;
            this.radialGauge1.TickPlacement = Syncfusion.Windows.Forms.Gauge.TickPlacement.OutSide;
            this.radialGauge1.MajorTickMarkColor = System.Drawing.Color.LightYellow;
            this.radialGauge1.MinorTickMarkColor = System.Drawing.Color.Red;
            this.radialGauge1.GaugeArcColor = ColorTranslator.FromHtml("#00a0d1");
            this.radialGauge1.GaugeLableColor = ColorTranslator.FromHtml("#00a0d1");
            this.radialGauge1.InterLinesColor = System.Drawing.Color.Red;
            this.radialGauge1.MinorTickMarkHeight = 55;
            this.radialGauge1.MajorTickMarkHeight = 6;
            this.radialGauge1.MinorInnerLinesHeight = 60;
            this.radialGauge1.BackColor = Color.Transparent;
            CustomRenderer custom1 = new CustomRenderer(this.linearGauge1);
            linearGauge1.Renderer = custom1;
            labelError.Text += "Araç Bağlantısı Bekleniyor-TM";


        }
        class CustomRenderer : ILinearGaugeRenderer
        {
            /// </summary>
            /// Gets the Linear gauge
            /// </summary>
            private LinearGauge m_LinearGauge;
            /// <summary>
            /// Gets/Sets the Tick Distance of the Linear gauge.
            /// </summary>
            private float majorTicksDistance;
            /// <summary>
            /// Calculates the Minor Ticks Pixels.
            /// </summary>
            private float m_minorTicksPixels;
            /// <summary>
            /// Start point of the frame
            /// </summary>
            private int start;
            /// <summary>
            /// Counts the Major ticks count for the given range.
            /// </summary>
            private int majorTicksCount;

            /// <summary>
            ///  Gets the Radial gauge
            /// </summary>
            internal LinearGauge LinearGauge
            {
                get
                {
                    return m_LinearGauge;
                }
            }

            public CustomRenderer(LinearGauge linearGauge)
            {
                m_LinearGauge = linearGauge;
                majorTicksDistance = 0;
                m_minorTicksPixels = 0;
                start = 25;

            }

            public void DrawFrame(System.Drawing.Graphics Graphics)
            {
            }

            [Obsolete]
            public void DrawLines(System.Drawing.Graphics Graphics)
            {
                Pen majorTickPen = new Pen(LinearGauge.MajorTickMarkColor);
                Pen minorTickPen = new Pen(LinearGauge.MinorTickMarkColor);
                Brush brush = new SolidBrush(LinearGauge.ForeColor);
                StringFormat sf = new StringFormat();
                Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                majorTicksDistance = ((LinearGauge.MaximumValue - LinearGauge.MinimumValue) / LinearGauge.MajorDifference);
                majorTicksCount = ((int)(LinearGauge.MaximumValue - LinearGauge.MinimumValue) / (LinearGauge.MajorDifference)) + 1;
                double majorTickValue = LinearGauge.MinimumValue;
                float tickPosition = 25f;
                float temp1 = 0;
                float s = (LinearGauge.MaximumValue - LinearGauge.MinimumValue) % LinearGauge.MajorDifference;
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                float minortickValue = 0;
                float tickPositionMinor = 0;
                GraphicsPath path = new GraphicsPath();
                int minor = LinearGauge.MinorTickCount;
                m_minorTicksPixels = ((this.LinearGauge.Height - 50) / majorTicksDistance);
                int x = this.LinearGauge.Width / 2;
                temp1 = 0;
                for (int L = 1; L <= majorTicksCount; L++)
                {
                    Graphics.DrawLine(majorTickPen, x, this.LinearGauge.Height - tickPosition, x - LinearGauge.MajorTicksHeight, this.LinearGauge.Height - tickPosition);
                    Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    if (LinearGauge.ShowScaleLabel)
                        Graphics.DrawString(Math.Round(majorTickValue, 2).ToString(),
                                 LinearGauge.Font, brush, new PointF(x - LinearGauge.MajorTicksHeight - 25, this.LinearGauge.Height - tickPosition), sf);
                    if (L == majorTicksCount)
                        minor = (LinearGauge.MinorTickCount * (int)Math.Ceiling(s)) / LinearGauge.MajorDifference;
                    if (majorTickValue < LinearGauge.MaximumValue)
                    {
                        for (int S = 1; S <= minor; S++)
                        {
                            minortickValue = (m_minorTicksPixels / (LinearGauge.MinorTickCount + 1)) * S;
                            tickPositionMinor = this.LinearGauge.Height - (minortickValue + temp1 + 25);
                            Graphics.DrawLine(minorTickPen, x, (float)tickPositionMinor, x - LinearGauge.MinorTickHeight, (float)tickPositionMinor);
                        }
                        temp1 = m_minorTicksPixels * L;
                    }

                    majorTickValue += LinearGauge.MajorDifference;
                    tickPosition += m_minorTicksPixels;
                }
                Graphics.FillRectangle(new SolidBrush(LinearGauge.GaugeBaseColor), this.LinearGauge.Width / 2, start - 1, 1, (((this.majorTicksDistance)) * m_minorTicksPixels) + 2);
                if (this.LinearGauge.MinimumValue > 0)
                    Graphics.FillRectangle(new SolidBrush(LinearGauge.ValueIndicatorColor), this.LinearGauge.Width / 2 + 10, start + (majorTicksDistance * m_minorTicksPixels) - (((LinearGauge.Value / LinearGauge.MajorDifference)) * m_minorTicksPixels), 5, (((LinearGauge.Value / LinearGauge.MajorDifference)) * m_minorTicksPixels) + 2);
                else
                    Graphics.FillRectangle(new SolidBrush(LinearGauge.ValueIndicatorColor), this.LinearGauge.Width / 2 + 10, start + (majorTicksDistance * m_minorTicksPixels) - ((((Math.Abs(this.LinearGauge.MinimumValue) + LinearGauge.Value) / LinearGauge.MajorDifference)) * m_minorTicksPixels), 5, ((((Math.Abs(this.LinearGauge.MinimumValue) + LinearGauge.Value) / LinearGauge.MajorDifference)) * m_minorTicksPixels) + 2);
                brush.Dispose();
                minorTickPen.Dispose();
            }

            public void DrawRanges(System.Drawing.Graphics Graphics)
            {
                Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                foreach (LinearRange ptrRange in this.LinearGauge.Ranges)
                {
                    int value = (int)Math.Ceiling(LinearGauge.MaximumValue - ptrRange.EndValue) / LinearGauge.MajorDifference;
                    if (ptrRange.EndValue > ptrRange.StartValue && ptrRange.EndValue <= this.LinearGauge.MaximumValue)
                    {
                        if (this.LinearGauge.MinimumValue >= 0 && ptrRange.StartValue < 0)
                        {
                            return;
                        }
                        float startValue = (float)ptrRange.StartValue;
                        float end = (float)ptrRange.EndValue;
                        if (this.LinearGauge.MinimumValue < 0)
                        {
                            startValue = this.LinearGauge.MinimumValue + Math.Abs(ptrRange.StartValue);
                        }
                        if (this.LinearGauge.MinimumValue < 0 && ptrRange.StartValue > 0)
                        {
                            startValue = Math.Abs(this.LinearGauge.MinimumValue) + Math.Abs(ptrRange.StartValue);
                        }
                        if (this.LinearGauge.MinimumValue < 0 && ptrRange.StartValue == 0)
                        {
                            startValue = Math.Abs(this.LinearGauge.MinimumValue) + Math.Abs(ptrRange.StartValue);
                            startValue = (((startValue / LinearGauge.MajorDifference)) * m_minorTicksPixels);
                        }

                        float height = (ptrRange.EndValue / LinearGauge.MajorDifference) * m_minorTicksPixels;
                        float endValueRangeHeight = 0f;
                        if (this.LinearGauge.MinimumValue < 0)
                        {
                            height = ((Math.Abs(this.LinearGauge.MinimumValue) + ptrRange.EndValue) / LinearGauge.MajorDifference) * m_minorTicksPixels;
                        }
                        endValueRangeHeight = height;
                        if (this.LinearGauge.MinimumValue < 0 && ptrRange.StartValue == 0)
                        {
                            endValueRangeHeight = (((ptrRange.EndValue - ptrRange.StartValue) / LinearGauge.MajorDifference) * m_minorTicksPixels);
                        }
                        if (ptrRange.StartValue == 0)
                            Graphics.FillRectangle(new SolidBrush(ptrRange.Color), this.LinearGauge.Width / 2 + 10, start + (majorTicksDistance * m_minorTicksPixels) - height, 8, endValueRangeHeight);
                        else if (ptrRange.StartValue > 0)
                            Graphics.FillRectangle(new SolidBrush(ptrRange.Color), this.LinearGauge.Width / 2 + 10, start + (majorTicksDistance * m_minorTicksPixels) - height, 8, (((ptrRange.EndValue - ptrRange.StartValue) / LinearGauge.MajorDifference) * m_minorTicksPixels));
                        else if (ptrRange.StartValue < 0)
                        {
                            Graphics.FillRectangle(new SolidBrush(ptrRange.Color), this.LinearGauge.Width / 2 + 10, start + (majorTicksDistance * m_minorTicksPixels) - height, 8, (((ptrRange.EndValue - ptrRange.StartValue) / LinearGauge.MajorDifference) * m_minorTicksPixels));
                        }
                    }
                }
            }

            public void DrawPointer(System.Drawing.Graphics Graphics)
            {
                Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                GraphicsPath path = new GraphicsPath();
                int a = 0;
                if (this.LinearGauge.MinimumValue < 0)
                    a = (int)Math.Ceiling((((Math.Abs(this.LinearGauge.MinimumValue) + LinearGauge.Value) / (float)LinearGauge.MajorDifference) * m_minorTicksPixels));
                else
                    a = (int)Math.Ceiling(((LinearGauge.Value / (float)LinearGauge.MajorDifference) * m_minorTicksPixels));
                int y = (this.LinearGauge.Height / 2 + 5 + LinearGauge.MajorTicksHeight) - LinearGauge.MajorTicksHeight;
                a = 10 + (int)Math.Ceiling((majorTicksDistance * m_minorTicksPixels)) - a;
                Rectangle rect = new Rectangle(new Point(this.LinearGauge.Width / 2 + 28, a), new Size(32, 32));
                SizeF sf = Graphics.MeasureString(this.LinearGauge.Value.ToString(), this.LinearGauge.GaugelabelFont);
                PointF point = new PointF(rect.X + rect.Width / 2 - sf.Width / 2, rect.Y + rect.Height / 2 - sf.Height / 2);
                Graphics.FillEllipse(new SolidBrush(LinearGauge.NeedleColor), rect);
                Graphics.DrawEllipse(new Pen(ColorTranslator.FromHtml("#00a0d1")), rect);
                Graphics.DrawLine(new Pen(ColorTranslator.FromHtml("#00a0d1")), rect.X, rect.Y + rect.Height / 2, rect.X - 18, rect.Y + rect.Height / 2);
                Graphics.DrawString(Math.Round(LinearGauge.Value, 2).ToString(), this.LinearGauge.GaugelabelFont, new SolidBrush(ColorTranslator.FromHtml("#ffffff")), point);
            }

            [Obsolete]
            public void UpdateRenderer(System.Windows.Forms.PaintEventArgs PaintEventArgs)
            {
                DrawLines(PaintEventArgs.Graphics);
                DrawRanges(PaintEventArgs.Graphics);
                DrawPointer(PaintEventArgs.Graphics);
            }
        }
        private void UserInitialization()
        {
            string[] comPorts = SerialPort.GetPortNames();
            int[] comBauds = { 7200, 9600, 19200, 56000, 57600, 115200, 128000 };
            foreach (string port in comPorts)
            {
                comboBoxAdv1.Items.Add(port);
            }
            try
            {
                comboBoxAdv2.DataSource = comBauds;
                comboBoxAdv1.SelectedIndex = 0;
                comboBoxAdv2.SelectedIndex = 2;
            }
            catch (Exception)
            {

            }

        }
        private void UpdateStatus()
        {
            lblRXSize.Text =this.RXBytes.ToString() + " byte  ";
            lblTXSize.Text =  this.TXBytes.ToString() + " byte  ";
            lblQueueLength.Text = RXQ.Count.ToString() + " ";
        }
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            String dataStr = serialPort1.ReadLine();
            this.RXBytes += dataStr.Length;
            RXQ.Enqueue(dataStr);
            this.UpdateStatus();
        }

        private void SerialReader()
        {
            while (true)
            {
                try
                {
                    if (serialPort1.IsOpen)
                    {
                        String dataStr = serialPort1.ReadLine();
                        this.RXBytes += dataStr.Length;

                        RXQ.Enqueue(dataStr);

                        this.UpdateStatus();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
        private void RXProcessor()
        {
            while (true)
            {
                try
                {
                    if (RXQ.Count > 0)
                    {
                        Debug.WriteLine("-------------------------XBee RF modülü haberleşmesi başlatıldı.-------------------------");
                        String RXData = RXQ.Dequeue().ToString().Substring(1);
                        String Command = RXData.Substring(0, 1);
                        String dataInfo = RXData.Substring(1);
                        Debug.WriteLine("RXData: " + RXData);
                        switch (Command)
                    {
                        case "T":
                            Int32.TryParse(dataInfo, out MaxTemp);
                            break;
                        case "V":
                            Double.TryParse(dataInfo, out TotalVoltage);
                            TotalVoltage /= 100;
                            break;
                        case "c":
                            Double.TryParse(dataInfo, out CurrentCurrent);
                            CurrentCurrent /= 100;
                            break;
                        case "s":
                            double s;
                            Double.TryParse(dataInfo, out s);
                            Speed = (int)Math.Ceiling(s);
                            break;
                        case "v":
                            String[] cv = dataInfo.Split('/');
                            if (cv.Length == 2 && cv[1].Length > 0)
                            {
                                int i = Int32.Parse(cv[0]);
                                double v;
                                Double.TryParse(cv[1], out v);
                                int percent = (int)(v * 100 / 4.2);
                                batteryLevels[i] = percent;
                                BatteryPercent[i] = String.Format("{0:0.00} V", v / 100);
                            }
                            break;
                        case "t":
                            String[] temp = dataInfo.Split('/');

                            if (temp.Length == 2 && temp[1].Length > 0)
                            {
                                int i;
                                Int32.TryParse(temp[0], out i);
                                this.batteryTemps[i] = temp[1].Substring(0, 4) + "°C";
                            }
                            break;
                        case "E":
                            switch (dataInfo)
                            {
                                case "S0":
                                    panel1.BackColor = Color.DarkGreen;
                                    labelError.Text += "    |-HIZ";
                                    break;
                                case "S1":
                                        panel1.BackColor = Color.Maroon;
                                        labelError.Text += "    |-HIZ Ölçülemiyor.     ";
                                    break;
                                case "B0":
                                        panel1.BackColor = Color.DarkGreen;
                                        labelError.Text += "    |-BMS";
                                    break;
                                case "B1":
                                        panel1.BackColor = Color.Maroon;
                                        labelError.Text += "    |-BMS değerleri ölçülemiyor.      ";
                                    break;
                                case "T0":
                                        panel1.BackColor = Color.DarkGreen;
                                        labelError.Text += "    |-SICAKLIK";
                                    break;
                                case "T1":
                                        panel1.BackColor = Color.Maroon;
                                        labelError.Text += "    |-SICAKLIK Ölçülemiyor.        ";
                                    break;
                                case "C0":
                                        panel1.BackColor = Color.DarkGreen;
                                        labelError.Text += "    |-AKIM";
                                    break;
                                case "C1":
                                        panel1.BackColor = Color.Maroon;
                                        labelError.Text += "    |-AKIM Ölçülemiyor.        ";
                                    break;
                                case "L0":
                                        panel1.BackColor = Color.DarkGreen;
                                        labelError.Text += "    |-AYDINLATMA      ";
                                    break;
                                case "L1":
                                        panel1.BackColor = Color.Maroon;
                                        labelError.Text += "    |-AYDINLATMA Hatası.      ";
                                    break;
                            }

                            break;
                        case "l":

                            if (dataInfo.Substring(0, 1) == "1")
                            {
                                pictureSagSig.Enabled = true;
                                pictureSolSig.Enabled = true;
                            }
                            else if (dataInfo.Substring(0, 1) == "2")
                            {

                                pictureSagSig.Enabled = true;
                                pictureSolSig.Enabled = false;
                            }
                            else if (dataInfo.Substring(0, 1) == "3")
                            {

                                pictureSagSig.Enabled = false;
                                pictureSolSig.Enabled = true;
                            }
                            else if (dataInfo.Substring(0, 1) == "0")
                            {

                                pictureSagSig.Enabled = false;
                                pictureSolSig.Enabled = false;
                            }
                            else if (dataInfo.Substring(1, 1) == "1")
                            {
                                pictureSag.Visible = false;
                                pictureSol.Visible = false;
                                pictureSagGif.Visible = true;
                                pictureSolGif.Visible = true;
                                pictureSagGif.Enabled = true;
                                pictureSolGif.Enabled = true;
                            }

                            else if (dataInfo.Substring(3, 1) == "1")
                            {
                                pictureSag.Visible = false;
                                pictureSol.Visible = false;
                                pictureSagGif.Visible = true;
                                pictureSolGif.Visible = true;
                                pictureSagGif.Enabled = true;
                                pictureSolGif.Enabled = true;
                            }

                            break;
                        default:
                            Debug.WriteLine("Undefined chracter: " + RXData);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }


        private void Panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

      
        private void BunifuFlatButton1_Click(object sender, EventArgs e)
        {
            ReadXbee();
            pictureBox2.Visible = false;
        }
        public  void ReadXbee()
        {
            try
            {                               
                labelConnect.Text = "Bağlanıyor..";                
                serialPort1.PortName = comboBoxAdv1.Items[comboBoxAdv1.SelectedIndex].ToString();
                serialPort1.BaudRate = Int32.Parse(comboBoxAdv2.Items[comboBoxAdv2.SelectedIndex].ToString());
                Debug.WriteLine("-------------------------Bağlanıyor..(" + serialPort1.PortName + ")--------------------------------------------");
                serialPort1.NewLine = "#";
                serialPort1.Open();
                if (serialPort1.IsOpen)
                {
                labelError.Text = String.Empty;
                Debug.WriteLine("-------------------------Araca bağlandı (" + serialPort1.PortName + ")-----------------------------------------");
                labelConnect.Text = "Araca bağlandı (" + serialPort1.PortName + ")";
                bunifuFlatButton1.Text = "Kontrol Sende";               
                timer1.Start();
                timer2.Start();
                timer4.Start();
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("-------------------------Bağlantı yok..(" + serialPort1.PortName + ")--------------------------------------------");
                bunifuFlatButton1.Text = "Tekrar Dene";
               
            }
        }

        private  void Timer1_Tick(object sender, EventArgs e)
        {
            linearGauge1.Value = MaxTemp;
            radialGauge1.Value= Speed;
            bunifuCustomLabelSpeed.Text = Speed.ToString();
            bunifuCustomLabelTemp.Text = MaxTemp.ToString();
            bunifuCustomLabelVolt.Text = TotalVoltage.ToString();        }
           

        private  void Timer2_Tick(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.Minimum = mimm;
            chart1.ChartAreas[0].AxisX.Maximum = maksm;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 200;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoom(mimm, maksm);
            this.chart1.Series[0].Points.AddXY((maksm), Speed);
            this.chart1.Series[1].Points.AddXY((maksm), MaxTemp);
            maksm++;
            mimm++;

            String logString = "";

            logString = "Saat: " + DateTime.Now.ToString() + " ";
            logString += "Toplam gerilim: " + TotalVoltage.ToString() + "V ";
            logString += "En yüksek Sıccaklık: " + MaxTemp.ToString() + "C ";
            logString += "Akım: " + CurrentCurrent.ToString() + "A ";
            logString += "Hız: " + Speed.ToString() + " km/h";

            for (int i = 0; i < 24; i++)
            {
                logString += i % 8 == 0 ? "\r\n" : "";
                logString += "Hücre1V: " + "0" + " Hücre1T: " +"0"+ "  ";
            }

            logString += "\r\n\r\n";

           
            this.SaveLog(logString);
        }    
      
        private void Timer3_Tick(object sender, EventArgs e)
        {
         
            if (labelError.Left > 0)
            {
                labelError.Left -= 16;
            }
            else
            {
                labelError.Left = 1644;
            }

            bunifuCircleProgressbar1.Value = batteryLevels[0];
            bunifuCircleProgressbar2.Value = batteryLevels[1];
            bunifuCircleProgressbar3.Value = batteryLevels[2];
            bunifuCircleProgressbar4.Value = batteryLevels[3];
            bunifuCircleProgressbar5.Value = batteryLevels[4];
            bunifuCircleProgressbar6.Value = batteryLevels[5];
            bunifuCircleProgressbar7.Value = batteryLevels[6];
            bunifuCircleProgressbar8.Value = batteryLevels[7];
            bunifuCircleProgressbar9.Value = batteryLevels[8];
            bunifuCircleProgressbar10.Value = batteryLevels[9];
            bunifuCircleProgressbar11.Value = batteryLevels[10];
            bunifuCircleProgressbar12.Value = batteryLevels[11];
            bunifuCircleProgressbar13.Value = batteryLevels[12];
            bunifuCircleProgressbar14.Value = batteryLevels[13];
            bunifuCircleProgressbar15.Value = batteryLevels[14];
            bunifuCircleProgressbar16.Value = batteryLevels[15];
            bunifuCircleProgressbar17.Value = batteryLevels[16];
            bunifuCircleProgressbar18.Value = batteryLevels[17];
            bunifuCircleProgressbar19.Value = batteryLevels[18];
            bunifuCircleProgressbar20.Value = batteryLevels[19];
            bunifuCircleProgressbar21.Value = batteryLevels[20];
            bunifuCircleProgressbar22.Value = batteryLevels[21];
            bunifuCircleProgressbar23.Value = batteryLevels[22];
            bunifuCircleProgressbar24.Value = batteryLevels[23];
            label1.Text = batteryTemps[0]+ "\n" + BatteryPercent[0];
            label2.Text = batteryTemps[1] + "\n" + BatteryPercent[1];
            label3.Text = batteryTemps[2] + "\n" + BatteryPercent[2];
            label4.Text = batteryTemps[3] + "\n" + BatteryPercent[3];
            label5.Text = batteryTemps[4] + "\n" + BatteryPercent[4];
            label6.Text = batteryTemps[5] + "\n" + BatteryPercent[5];
            label7.Text = batteryTemps[6] + "\n" + BatteryPercent[6];
            label8.Text = batteryTemps[7] + "\n" + BatteryPercent[7];
            label9.Text = batteryTemps[8] + "\n" + BatteryPercent[8];
            label10.Text = batteryTemps[9] + "\n" + BatteryPercent[9];
            label11.Text = batteryTemps[10] + "\n" + BatteryPercent[10];
            label12.Text = batteryTemps[11] + "\n" + BatteryPercent[11];
            label13.Text = batteryTemps[12] + "\n" + BatteryPercent[12];
            label14.Text = batteryTemps[13] + "\n" + BatteryPercent[13];
            label15.Text = batteryTemps[14] + "\n" + BatteryPercent[14];
            label16.Text = batteryTemps[15] + "\n" + BatteryPercent[15];
            label17.Text = batteryTemps[16] + "\n" + BatteryPercent[16];
            label18.Text = batteryTemps[17] + "\n" + BatteryPercent[17];
            label19.Text = batteryTemps[18] + "\n" + BatteryPercent[18];
            label20.Text = batteryTemps[19] + "\n" + BatteryPercent[19];
            label21.Text = batteryTemps[20] + "\n" + BatteryPercent[20];
            label22.Text = batteryTemps[21] + "\n" + BatteryPercent[21];
            label23.Text = batteryTemps[22] + "\n" + BatteryPercent[22];
            label24.Text = batteryTemps[23] + "\n" + BatteryPercent[23];
        }

        private void Timer4_Tick(object sender, EventArgs e)
        {
            this.clock2.CustomTime = this.clock2.CustomTime.AddSeconds(+1);
            
        }

      
        Process LogProcess;
        Boolean durum = false;
        private void BunifuFlatButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (durum == true)
                {
                    LogProcess.CloseMainWindow();
                    LogProcess.Close();
                    bunifuFlatButton2.Text = "KAYITLARI GÖSTER";
                    durum = false;
                }
                else if (durum == false)
                {
                    LogProcess = Process.Start(this.LogFilename);                    
                    bunifuFlatButton2.Text = "KAYITLARI KAPAT";
                    durum = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                bunifuFlatButton2.Text = "KAYITLARI GÖSTER";
                durum = false;
            }
            finally
            {
            }
        }
        private String LogFolder = @"C:\TM";
        private String LogFilename = @"C:\TM\TM.log";

        private void SaveLog(String s)
        {
            if (!Directory.Exists(this.LogFolder))
            {
                DirectoryInfo di = Directory.CreateDirectory(this.LogFolder);
                File.WriteAllText(this.LogFilename, "TÜRK MEKATRONİK\n\r\n\r");
            }

            File.AppendAllText(this.LogFilename, s);
        }

        private void BunifuFlatButton3_Click_1(object sender, EventArgs e)
        {
          
            
            Application.Exit();
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    Connected = false;
                    //lblConnectionStatus.Text = "Bağlantı kesildi!";
                    //lblConnectionStatus.ForeColor = Color.Red;
                    timer1.Stop();
                    timer2.Stop();
                    timer4.Stop();
                    QProcessorThread.Abort();
                    SerialReaderThread.Abort();
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
            }
        }

        private void GradientPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        
        private void PictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            timer3.Start();
            this.clock2.ShowCustomTimeClock = true;
            // To hide Dates in Digital Clock   
            this.clock2.DisplayDates = false;
            //To set the Custom time / Reset the clock   
            this.clock2.CustomTime = new DateTime();
            // To Freeze and Un Freeze the Clock   
            this.clock2.StopTimer = false;
            this.radialGauge1.BringToFront();
            this.linearGauge1.BringToFront();            
            this.pictureBox2.BringToFront();
        }

    }
        
}
    