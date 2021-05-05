using System;
//using System.Text;
//using System.Runtime.InteropServices;

namespace VenturaSQL
{

	public class VenturaException : Exception
	{

		public VenturaException() : base("VenturaSQL Exception")
		{
		}

		public VenturaException ( string message ) : base ( message )
		{
		}

        public VenturaException(string message, Exception innerException) : base(message, innerException)
        {
        }

	}


}
