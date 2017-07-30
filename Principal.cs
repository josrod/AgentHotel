using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using AvatarGestos;
using BotASR;
using WebApp;
using Chat;
using Encuesta;
using System.Text.RegularExpressions;
using System.Threading;


namespace AIEBOT
{
    public class Principal 
    {
        private ASRDllImport AsrDll;
        private IPCDllImport myIPCDll;
        private ChatBot myChatBot;

        // Variables que manejan la entrada y salida del Chat
        private string rawInput="";
        private string sOutPut="";
        private string sEmoOutPut = "";

        // preparo las frases del test
        Preguntas ListFrases; // Inicializo la lista
        int NumFrases; // Guardo el número de preguntas
        int nContFrases; // contador al numero de preguntas
        bool bIniEncuesta=true;

        int NumSurvey;
        int nContSurvey; // Contador al número de preguntas survey
        Regex objNaturalPattern; // verificar si la respuesta del test esta entre [1-5]
        //int nCont;

        private SacarinoForm myFormSacarino; // gets the instance of SacarinoForm
        
        bool Mensaje1 = true;
        protected bool InicioDLG = false;

        public Principal(SacarinoForm formSacarino)
        {

            #region DefinicionesGlobalIPC
            string nombreModulo = "Sacarino_ASR";
            string dirIPCentral = "127.0.0.1";//"192.168.107.43";//"127.0.0.1"
            string tipoMensaje_ReconASR = "MSG_Asr_Recognition";
            string tipoDatos_ReconASR = "{string,string}";
            #endregion

            #region Inicialización de la Comunicacion IPC
            myIPCDll = new IPCDllImport();
            //Conexion al central
            myIPCDll.Conectar(nombreModulo, dirIPCentral);
            ////Definicion de mensajes
            myIPCDll.DefinirMensaje(tipoMensaje_ReconASR, tipoDatos_ReconASR);
            #endregion

            #region Inicialización del ASR;
            AsrDll = new ASRDllImport();
            //AsrDll.OnMsg += new MsgEventHandler(AsrDll_OnMsg);  // Eventos del LASR Pero no lo estoy utilizando
            //AsrDll.ASRConfiguration();
            Console.WriteLine(AsrDll.strStatus + "\n");
            //AsrDll.strLibrary = "AudioMM";
            //AsrDll.strFileSource = null;
            #endregion

            #region Inicialización del ChatBot;
            myChatBot = new ChatBot();
            myChatBot.ChatBotConfig();
            #endregion

            ListFrases = new Preguntas();
            NumFrases = ListFrases.frases.Count;
            nContFrases = 0;
            NumSurvey = ListFrases.survey.Count;
            nContSurvey = 0;

            objNaturalPattern = new Regex("[1-9]");

            myFormSacarino = formSacarino;
        }

        public void IniciarPrograma(string sInput)//(SacarinoForm FormSacarino, string sInput)//Quitar sInput cuando sea x voz
        {
            Console.WriteLine("\n ESPERANDO ENTRADA DE VOZ");
            //Inicio reconocimiento
            //AsrDll.OnRecognize();
            //rawInput = AsrDll.strLastResult; 
            rawInput = sInput;
                
            // Obtengo el resultado del reconocimiento y lo imprimo                
            Console.WriteLine("You: " + rawInput + "\n");

            if (rawInput.Contains("hola"))
            {
                InicioDLG = true;
            }

            if (AsrDll.nEvento == Constants.LASRX_RETCODE_OK && InicioDLG)
            {
                // Llamo al Chat para procesar la entrada del usuario
                myChatBot.processInputFromUser(rawInput, 0.44);// AsrDll.fConfidence); OR 0.44);
                
                sOutPut = myChatBot.ChatResult;
                sEmoOutPut = myChatBot.ChatEmo;

                myFormSacarino.sAtributo = myChatBot.Atributo;
                myFormSacarino.sValor = myChatBot.Valor;
                myFormSacarino.GmapsURL = myChatBot.GUrl;
                myFormSacarino.SChatOutPut = sOutPut;

                // llamo a la acción a realizar según el promt del usuario
                //if (sOutPut != "")
                //{
                    if (myChatBot.Action == "encuesta")
                    {
                        // myChatBot.Valor para verificar que las respuestas estan entre 1-5
                        Iniciar_Encuesta(myChatBot.Valor);

                    }
                        // TODO: Meter todas las acciones que genera el Chat
                    else
                    {
                        if (sEmoOutPut =="")
                        {
                            myIPCDll.EnviarTexto("ASR_OK", sOutPut);
                            Console.WriteLine("Bot: " + sOutPut + "\n");
                        }
                        else
                        {
                            myIPCDll.EnviarTexto("ASR_OK", sEmoOutPut);
                            Console.WriteLine("Bot: " + sEmoOutPut + "\n");
                        }
                        //MostrarResultado en el SacarinoForm
                        myFormSacarino.SelectTabPage(myChatBot.Topic);

                    }

                //}
                Console.WriteLine("topic: " + myChatBot.Topic + "\n");
                Console.WriteLine("accion: " + myChatBot.Action + "\n");
                Console.WriteLine("valor: " + myChatBot.Valor + "\n");
            }
            else
            {
                //myChatBot.processInputFromUser("salir", 0.44);// AsrDll.fConfidence); OR 0.44);
                //sOutPut = formSacarino.myDlgBehavior.ChatResult;
                Console.Write("You: -- \n");
                MsgInicial();
            }

        }

        public void MsgInicial()
        {
            string MsgIPC;
            if (Mensaje1)
            {
                MsgIPC = Constantes.MsgIni_1;
                myIPCDll.EnviarTexto("ASR_OK", Emotions.iMirar + MsgIPC + Emotions.fMirar);
                Console.WriteLine("Bot: " + MsgIPC + "\n");

                myFormSacarino.VisualizarInicial(MsgIPC);
                Mensaje1 = false;         
            }
            else
            {
                MsgIPC = Constantes.MsgIni_2;
                myIPCDll.EnviarTexto("ASR_OK", Emotions.iBocaContento + MsgIPC + Emotions.fBocaContento);
                myIPCDll.EnviarTexto("ASR_OK", Emotions.iParpadeo + Emotions.fParpadeo);
                Console.WriteLine("Bot: " + MsgIPC + "\n");

                myFormSacarino.VisualizarInicial(MsgIPC);
                Mensaje1 = true;
            }

        }

        #region ENCUESTA
        private void Iniciar_Encuesta(string sValor)
        {
            string sTipo = "";
            if (nContFrases == 0 && bIniEncuesta)
            {
                myFormSacarino.SChatOutPut = "Diga Si, si desea hacer la encuesta o No si quiere salir";
                myIPCDll.EnviarTexto("ASR_OK", sOutPut);
                Console.WriteLine("Bot: " + sOutPut + "\n");
                myFormSacarino.SelectTabPage(myChatBot.Topic);
                bIniEncuesta = false;
            }
            
           else
           {
               if (sValor == "fin")
               {
                   sTipo = "END";
                   sOutPut = Constantes.MsgEnd_Encuesta;
                   ReIniciarValores();
                   myFormSacarino.VisualizarEncuesta(sOutPut, nContFrases, sTipo);
               }
               else if (myChatBot.Atributo == "inicio")
               {
                   myIPCDll.EnviarTexto("ASR_OK", sOutPut);
                   Console.WriteLine("Bot: " + sOutPut + "\n");
                   myFormSacarino.VisualizarEncuesta(sOutPut, nContFrases, sTipo);
                   Thread.Sleep(4000);
                   Iniciar_Seq_Encuesta(nContFrases);
               }
               else
               {
                   if (nContFrases == 0)
                       return;

                   if (nContFrases < NumFrases)
                   {
                       Iniciar_Seq_Encuesta(nContFrases);
                   }
                   else
                   {
                       if (nContSurvey < NumSurvey) 
                       {
                           iniciar_Seq_Survey(nContSurvey);
                       }
                       else
                       {
                           if (nContSurvey >= NumSurvey)
                           {
                               sTipo = "END";
                               sOutPut = Constantes.MsgEnd_Encuesta;
                               ReIniciarValores();
                               myFormSacarino.VisualizarEncuesta(sOutPut, nContFrases, sTipo);
                           }
                       }
                   }                       
               }

           }
        }

        private void Iniciar_Seq_Encuesta(int inumPregunta)
        {
            string sTipo = "";
            if (objNaturalPattern.IsMatch(myChatBot.Valor))
            {
                sTipo = "ENCUESTA";
                sOutPut = ListFrases.frases[inumPregunta];
                nContFrases++;
                myFormSacarino.VisualizarEncuesta(sOutPut, nContFrases, sTipo);
            }

        }

        private void iniciar_Seq_Survey(int nSurvey) 
        {
            string sTipo = "SURVEY";
            if (nContSurvey == 0)
            {

                sOutPut = Constantes.MsgIni_Encuesta;
                myIPCDll.EnviarTexto("ASR_OK", sOutPut);
                Console.WriteLine("Bot: " + sOutPut + "\n");
            }

            if (myChatBot.Valor == "data" || objNaturalPattern.IsMatch(myChatBot.Valor))
            {
                sOutPut = ListFrases.survey[nContSurvey];
                nContSurvey++;
                myIPCDll.EnviarTexto("ASR_OK", sOutPut);
                Console.WriteLine("Bot: " + sOutPut + "\n");
                myFormSacarino.VisualizarEncuesta(sOutPut, nContSurvey, sTipo);

            }

        }
        #endregion

        private void ReIniciarValores()
        {
            nContFrases = 0;
            nContSurvey = 0;
            myChatBot.ReiniciarValores();
            InicioDLG = false;
            bIniEncuesta = true;
        }

    }
}
