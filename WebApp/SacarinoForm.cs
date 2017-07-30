using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Encuesta;

namespace WebApp
{
    public partial class SacarinoForm : Form
    {    
        private Weather myWeather;
        private Conditions conditions;
        private HelpImg myHelpImg;
        private List<Conditions> ListConditions;
        
        // Preparo las frases del test
        Preguntas ListPreguntas; // Inicializo la lista
        int NumFrases; // Guardo el número de preguntas
        int NumSurvey;

        private string baseURL = "http://www.google.com";

        // Fade in and out. ** Efecto de inicio del form **
        private double m_dblOpacityIncrement = .05;
        private const int TIMER_INTERVAL = 60;
        // Self-calibration support
        private int m_iActualTicks = 0;

        // Delegados para manejar subprocesos del Form
        delegate void SetTabPageCallback(string strTopic); // Para poder ejecutar los subprocesos de los demás tags
        delegate void SetLabelTexCallback(string text); // Para ejecutar el subprocesos del tag principal
        delegate void GeneralCallback(string sPregunta, int nIndicador, string sTipo); // Para ejecutar el subprocesos del tag principal
        // Datos publicos para heredar del DlgBehavior
        public string sAtributo="";
        public string sValor="";
        public string GmapsURL = "";
        public string SChatOutPut="";

        public SacarinoForm()
        {
            InitializeComponent();

            // Inicio las clases que afectan el estado del SacarinoForm
            //Para visualizar el tiempo
            myWeather = new Weather();
            conditions = new Conditions();
            ListConditions = new List<Conditions>();

            // Para cambiar las imagenes de ayuda que queremos mostrar
            myHelpImg = new HelpImg();
            //myDlgBehavior = new Dlgbehavior();

            //Para ejecutar la encuesta
            ListPreguntas = new Preguntas();
            NumFrases = ListPreguntas.frases.Count;
            NumSurvey = ListPreguntas.survey.Count;

            // Propiedades de SacarinoForm
            this.Opacity = .00;
            timer1.Interval = TIMER_INTERVAL;
            timer1.Start();

        }


        #region Manejo de los subprocesos
        
        public void SelectTabPage(string ChatTopic)
        {
            if (this.tabControlPestañas.InvokeRequired)
            {
                SetTabPageCallback d = new SetTabPageCallback(SelectTabPage);
                this.Invoke(d, new object[] { ChatTopic });
            }
            else
            {
                SelectedTab(ChatTopic);
            }
        }

        public void VisualizarInicial(string MsgLabel)
        {
            if (this.tabControlPestañas.InvokeRequired)
            {
                SetLabelTexCallback d = new SetLabelTexCallback(VisualizarInicial);
                this.Invoke(d, new object[] { MsgLabel });
            }
            else
            {
                TabPrincipal(MsgLabel);
            }
        }

        public void VisualizarEncuesta(string sPregunta, int nIndicador, string sTipo)
        {
            if (this.tabControlPestañas.InvokeRequired)
            {
                GeneralCallback d = new GeneralCallback(VisualizarEncuesta);
                this.Invoke(d, new object[] { sPregunta,nIndicador,sTipo });
            }
            else
            {
                Encuesta(sPregunta, nIndicador, sTipo);
            }
        }
        
        #endregion

        /// <summary>
        /// Función para cambiar la visualización (SelectedTab) según la solicitud del usuario
        /// A partir de las propiedades de myDlgBehavior
        /// </summary>
        void SelectedTab(string ChatTopic)
        {
            //string sAcction = ChatAcction;
            string strTipoTopic = ChatTopic;

            //GmapsURL = myDlgBehavior.GUrl;

            if (strTipoTopic == "HOTELINFO")
            {
                this.toolStripLabelTopic.Text = "HOTEL";
                this.tabControlPestañas.SelectedTab = this.tabPageHotel;
                this.toolStripButtonTopic.Image = AIEBOT.Properties.Resources.icono_Hotel;
                string img = myHelpImg.SelectImg(sAtributo, sValor);
                pictureBoxHotel.Image = Image.FromFile(img);
                img = myHelpImg.SelectMsg();
                pictureBoxMsg.Image = Image.FromFile(img);
            }
            else if (strTipoTopic == "OWNINFO")
            {
                this.toolStripLabelTopic.Text = "MI INFORMACIÖN";
                //this.toolStripButtonTopic.Image = AIEBOT.Properties.Resources.icono_tiempo;
                this.tabControlPestañas.SelectedTab = this.tabPageOwnInfo;
                this.labelOwnInfo.Text = SChatOutPut;
            }
            else if (strTipoTopic == "RESTAURANTES")
            {
                this.toolStripLabelTopic.Text = "RESTAURANTES";
                this.tabControlPestañas.SelectedTab = this.tabPageWebApp;
                if (sAtributo == "tipo")
                {
                    this.labelRestaurantes.Visible = false;
                    this.pictureBoxRest.Visible = false;
                    webBrowserMap.Visible = true;
                    webBrowserMap.Size= new Size(1080, 705);
                    webBrowserMap.Location = new Point(-378, -120);
                    webBrowserMap.Navigate(GmapsURL);                    
                }
                else if (sAtributo == "nombre")
                {
                    this.labelRestaurantes.Visible = false;
                    this.pictureBoxRest.Visible = false;
                    webBrowserMap.Visible = true;
                    webBrowserMap.Size = new Size(550, 555);
                    webBrowserMap.Location = new Point(0, 0);
                    webBrowserMap.Navigate(GmapsURL);
                }
                else
                {
                    webBrowserMap.Visible = false;
                    this.labelRestaurantes.Visible = true;
                    this.pictureBoxRest.Visible = true;
                    this.labelRestaurantes.Text = SChatOutPut;

                }
                
            }
            else if (strTipoTopic == "CLIMA")
            {
                this.toolStripLabelTopic.Text = "EL TIEMPO";
                this.toolStripButtonTopic.Image = AIEBOT.Properties.Resources.icono_tiempo;
                this.tabControlPestañas.SelectedTab = this.tabPageWeather;
                getWeather("Valladolid");
            }
            else if (strTipoTopic == "AYUDA")
            {
                this.toolStripLabelTopic.Text = "AYUDA";
                this.toolStripButtonTopic.Image = AIEBOT.Properties.Resources.icono_help;
                this.tabControlPestañas.SelectedTab = this.tabPageHelp;

            }
            else if (strTipoTopic == "SUBJETIVA")
            {
                this.toolStripLabelTopic.Text = "ENCUESTA";
                this.toolStripButtonTopic.Image = AIEBOT.Properties.Resources.icono_busqueda;
                this.tabControlPestañas.SelectedTab = this.tabPageTest;
                this.labelPregunta.Text = SChatOutPut;
            }
            else
            {
                this.toolStripLabelTopic.Text = "PRINCIPAL";
                this.toolStripButtonTopic.Image = AIEBOT.Properties.Resources.icono_Hotel;
                this.tabControlPestañas.SelectedTab = this.tabPageIniDlg;
                this.labelInicial.Text = SChatOutPut;
            }

        }

        #region SPLASH SCREEN
        /// <summary>
        /// Función para generar el Splash screen de la aplicación 
        /// Mientras el usuario no diga hola para iniciar el dialogo
        /// </summary>
        void TabPrincipal(string Msg)
        {
            this.labelPrincipal.Text = Msg;
            this.toolStripLabelTopic.Text = "";
            this.toolStripButtonTopic.Image = null;
            this.tabControlPestañas.SelectedTab = this.tabPagePrincipal;
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            /* Esto es solo para que al iniciar el Form tenga efecto Fade*/
            if (m_iActualTicks > 50)
            {
                timer1.Stop();
            }

            m_iActualTicks++;
            if (this.Opacity < 1)
            {
                this.Opacity += m_dblOpacityIncrement;
            }
        }
        #endregion

        #region WEATHER
        /// <summary>
        /// Función para visualizar el tiempo del día y el pronóstico
        /// A partir del archivo xml generado por google en la clase Weather.cs
        /// </summary>
        void getWeather(string City)
        {
            labelCity.Text = "El tiempo en " + City;
            conditions = myWeather.GetCurrentConditions(City);

            if (conditions != null)
            {
                labelCon.Text = "Condiciones: " + conditions.Condition;
                labelTEmp.Text = (conditions.TempC + "ºC");
                labelHum.Text = conditions.Humidity;
                labelWind.Text = conditions.Wind;

                #region FORECAST

                ListConditions = myWeather.GetForecast();

                // Today Forecast
                conditions = ListConditions[0];
                pictureBoxDay1.Load(baseURL + conditions.IconDay);
                labelDia1.Text = conditions.DayOfWeek;
                labelTemp1.Text = conditions.TempHigh + " ºC | " + conditions.TempLow + " ºC";

                // Tomorrow Forecast
                conditions = ListConditions[1];
                pictureBoxDay2.Load(baseURL + conditions.IconDay);
                labelDia2.Text = conditions.DayOfWeek;
                labelTEmp2.Text = conditions.TempHigh + " ºC | " + conditions.TempLow + " ºC";

                // Day 3 Forecast
                conditions = ListConditions[2];
                pictureBoxDay3.Load(baseURL + conditions.IconDay);
                labelDia3.Text = conditions.DayOfWeek;
                labelTemp3.Text = conditions.TempHigh + " ºC | " + conditions.TempLow + " ºC";

                // Day 4 Forecast
                conditions = ListConditions[3];
                pictureBoxDay4.Load(baseURL + conditions.IconDay);
                labelDia4.Text = conditions.DayOfWeek;
                labelTemp4.Text = conditions.TempHigh + " ºC | " + conditions.TempLow + " ºC";
                #endregion
            }
            else
            {
                this.labelWeather.Text = "En estos momentos no puedo visualizar la información";
                //MessageBox.Show("There was an error processing the request.");
                //MessageBox.Show("Please, make sure you are using the correct location or try again later.");
            }
        }
        #endregion

        #region ENCUESTA

        private void Encuesta(string sPregunta, int nIndicador, string sTipo)
        {
            if (sTipo == "ENCUESTA")
            {
                this.labelPregunta.Text = sPregunta;
                this.labelIndicador.Text = "Pregunta " + nIndicador + " de " + NumFrases;
                //guardarBD(sAtributo, sValor);
            }
            else if (sTipo == "SURVEY")
            {
                this.labelEncInfo.Text = "A continuación debes llenar los siguiente datos demográficos, sobre tu Sexo, Edad, y experiencia en sistemas como este y al final si lo deseas puedes hacer algunos comentarios.";
                labelPregunta.Visible = false;
                groupBoxDemoData.Visible = true;
                this.labelPregunta.Text = sPregunta;
                this.labelIndicador.Text = "Pregunta " + nIndicador + " de " + NumSurvey;
                //guardarBD(sAtributo, sValor);

            }
            else if (sTipo == "END")
            {
                EndEncuesta();
            }
            else
            {
                this.labelPregunta.Text = sPregunta;
            }
        }
        private void EndEncuesta()
        {
            //Mensaje de salida y inicializo los componentes de la encuesta
            this.labelPregunta.Text = "Gracias por tu colaboración nos vemos en una proxima ocación.";
            this.labelEncInfo.Text = "A continuación se formulan algunas preguntas en relación a su experiencia de interacción. Por favor, diga la respuesta que prefiera:";
            this.labelIndicador.Text = "";
            labelPregunta.Visible = true;
            groupBoxDemoData.Visible = false;
            
            // Start the BackgroundWorker.
            this.backgroundWorker1.RunWorkerAsync();

        }

        /// <summary>
        /// backgroundWorker1_DoWork para finalizar automaticamente la aplicación una vez
        /// el usuario ha hecho la Encuesta 
        /// Por lo cual llamo a el tag principal y se reinician las propiedades. 
        /// </summary> 
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Doy un tiempo de 3 seg para llevar a la ventana principal del dialogo
            // Wait 3 seconds.
            Thread.Sleep(2500);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Llamo a la aplicación para mostrar la pantalla inicial de interacíón
            VisualizarInicial("");

        }
        #endregion

        //Función para llamar a la base de datos y guardar los resultados de la encuesta
        private void guardarBD(string sAtributo, string sValor)
        {
            if (sAtributo != "")
            {
                listBoxResp.Items[0] = sAtributo;
                listBoxResp.Items[1] = sValor;
            }

        }

        private void SacarinoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ESTA COMENTADO PARA EN ESTAS PRUBAS NO GENERAR UN LOG
            //Chat.ChatBot.swWriter.WriteLine("~~~~~~~~~~~~~~~~ Conversation ~~~~~~~~~~~~~~~~");
            //Chat.ChatBot.swWriter.WriteLine("    Cierre Fecha: " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //Chat.ChatBot.swWriter.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            //Chat.ChatBot.swWriter.WriteLine("");

            //Chat.ChatBot.swWriter.Close();
        }



    }
}