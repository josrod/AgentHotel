using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;

namespace WebApp
{
    public partial class SplashScreen : Form
    {
        Process myprocCentral = new Process();
        string strPath;

        // Fade in and out.
        private double m_dblOpacityIncrement = .05;
        private const int TIMER_INTERVAL = 50;

        // Self-calibration support
        private int m_iActualTicks = 0;

        public SplashScreen()
        {
            InitializeComponent();

            strPath = "K:/PROGRAMAS/";

            // TODO : Falta chequear si hay un central.exe lanzado para no lanzarlo otra vez
            //myprocCentral.EnableRaisingEvents=false;
            //myprocCentral = Process.Start(strPath +"central.exe");

            this.Opacity = .00;
            timer1.Interval = TIMER_INTERVAL;
            timer1.Start();


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //valor 80 +- 5 seg *** Close the splashscreen 
            if (m_iActualTicks > 100)
            {
                SplashMsg2();
                //m_iActualTicks = 0;
            }
            else
                SplashMsg1();
                //this.Close();

                m_iActualTicks++;
                if (this.Opacity < 1)
                    this.Opacity += m_dblOpacityIncrement;
        }

        public string SplashMsg1()
        {
            this.labelSaludoIni.Text = "Hola, soy Pedro y estoy aqui para darte la información que necesites!";
            return labelSaludoIni.Text;
        
        }

        public string SplashMsg2()
        {
            this.labelSaludoIni.Text = "Para empezar a interactuar conmigo solo debes decir !hola!";
            return labelSaludoIni.Text;
        }

    }
}