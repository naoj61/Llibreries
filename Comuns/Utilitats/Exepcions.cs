using System;
using System.Windows.Forms;

namespace Comuns
{
    public class UtilitatsException : Exception
    {
        public UtilitatsException(Utilitats.Errors error) : base("Error Utilitats")
        {
            Error = error;
        }

        public readonly Utilitats.Errors Error;
    }

    public class ValorMassaGranException : Exception
    {
        public Control Control { get; }

        public ValorMassaGranException(string missatge, Control control)
            : base(missatge)
        {
            Control = control;
        }
    }
}
