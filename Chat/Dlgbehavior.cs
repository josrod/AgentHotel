using System;
using System.Collections.Generic;
using System.Text;

namespace Chat
{

    public class Dlgbehavior
    {
        #region Dialog Behavior Members
        
        private string _ChatResult;
        private string _ChatEmo;
        private string _Action;
        private string _Topic;
        private string _Atributo;
        private string _Valor;
        private string _GUrl;

        public Dlgbehavior() // Constructor
        {
            _ChatResult = "";
            _Action = "";
            _Topic = "";
            _Atributo = "";
            _Valor = "";
            _GUrl = "";
            _ChatEmo = "";
        }

        /// <summary>
        /// Chat result
        /// </summary>
        public string ChatResult
        {
            get { return _ChatResult; }
            set { _ChatResult = value; }
            
        }

        /// <summary>
        /// While dialogin Emotion
        /// </summary>
        public string ChatEmo
        {
            get { return _ChatEmo; }
            set { _ChatEmo = value; }
        }

        /// <summary>
        /// Dialog goal like Verbs
        /// </summary>
        public string Action
        {
            get { return _Action; }
            set { _Action = value; }
        }

        /// <summary>
        /// Dialog topic 
        /// </summary>
        public string Topic
        {
            get { return _Topic; }
            set { _Topic = value; }
        }

        /// <summary>
        /// Dialog attribute. Type of information
        /// </summary>
        public string Atributo
        {
            get { return _Atributo; }
            set { _Atributo = value; }
        }

        /// <summary>
        /// Attribute value
        /// </summary>
        public string Valor
        {
            get { return _Valor; }
            set { _Valor = value; }
        }

        /// <summary>
        /// Si es busqueda guardo la Url de GMap generado
        /// </summary>
        public string GUrl
        {
            get { return _GUrl; }
            set { _GUrl = value; }
        }
        //public void LipiarVariables()
        //{
        //    _ChatResult="";
        //    _Action="";
        //    _Topic="";
        //    _Atributo="";
        //    _Valor="";
        //    _GUrl="";
        //    _ChatEmo = "";
        //}
        #endregion
    }
}
