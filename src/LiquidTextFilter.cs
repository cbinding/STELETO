using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STELETO
{
    public static class LiquidTextFilter
    {        
        // escape single quotes for use in Turtle (*.ttl) RDF file
        public static string escape_single_quotes(string input)
        {
            return input.Replace("'", "\'");
        }   

        // escape double quotes for use in Turtle (*.ttl) RDF file
        public static string escape_double_quotes(string input)
        {
            return input.Replace("\"", "\\\"");            
        }
             
    }
}

