using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using StarPrinterWebServiceAppNamespace;

namespace StarPrinterWebServiceApp
{
    public partial class MainWindow : Form
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private StarPrinterWebApiService _starPrinterWebService;
        private Printer _printer;
        private Timer timer;

        public MainWindow()
        {
            InitializeComponent();
        }

       
        private void Form1_Load(object sender, EventArgs e)
        {
            logger.Info("Starting application");
            notifyIcon1.BalloonTipText = "Printerio programa";
            notifyIcon1.BalloonTipTitle = "Printerio programa";
            _printer = new Printer();
            Templates templates = new Templates();

            _starPrinterWebService = new StarPrinterWebApiService(templates, _printer);
            _starPrinterWebService.Start("http://localhost:5000/");

            timer = new Timer();
            timer.Interval = 5000;
            timer.Tick += Timer_Tick;
            timer.Start();

            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerAsync();


        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            _printer.findPrinterIfNotAvailable();
            string printerInfo = _printer.available() ? _printer.getInfo() : "nerastas";

            Invoke(new Action(() =>
            {
                label2.Text = printerInfo;
            }));                
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _starPrinterWebService.Stop();
        }

     }
}
