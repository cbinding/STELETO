/*
================================================================================
Creator : Ceri Binding, University of South Wales
Project	: STELETO
Classes	: STELETO.Program
Summary	: Main entry point for STELETO application
License : http://creativecommons.org/licenses/by/3.0/ 
================================================================================
History :

12/01/2011  CFB Created classes
================================================================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace STELETO
{
    class Program
    {
        static void Main(string[] args)
        {
            //Command handler
            DelimitedToStg command = new DelimitedToStg(); 
            command.Main(args);            
        }       
    }
}
