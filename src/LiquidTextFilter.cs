using System;

namespace STELETO
{
    public static class LiquidTextFilter
    {
        // convert string representation of date/time to xsd:dateTime for use in RDF and XML applications
        public static string to_iso_8601(string input)
        {
            DateTime result;
            bool success = DateTime.TryParse(input, out result);
            if (success)
                return result.ToString("yyyy-MM-ddTHH:mm:ssZ");
            else
                return input;
        }

        // escape backslash for use in NTriples (*.ttl) RDF file 
        public static string escape_backslash(string input)
        {
            return input.Replace("\\", "\\\\");
        }

        // escape single quotes for use in Turtle (*.ttl) RDF file (superseded - dont really need this now we have regexreplace)
        public static string escape_single_quotes(string input)
        {
            return input.Replace("'", "\'");
        }

        // escape double quotes for use in Turtle (*.ttl) RDF file (superseded - dont really need this now we have regexreplace)
        public static string escape_double_quotes(string input)
        {
            return input.Replace("\"", "\\\"");            
        }

        // check if a string value is purely numeric or not (superseded - dont really need this now we have isregexmatch)
        public static bool isinteger(string input)
        {
            Int64 result = 0;
            return Int64.TryParse(input, out result);
        }

        // check if a string value contains ANY numbers (superseded - dont really need this now we have isregexmatch)
        public static bool containsinteger(string input)
        {
            return input.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) >= 0;            
        }

        // regular expression replacement within templates
        public static string regexreplace(string input, string pattern, string replacement) {
           return System.Text.RegularExpressions.Regex.Replace(input ?? "", pattern, replacement);
        }

        // test for a regular expression match
        public static bool isregexmatch(string input, string pattern) {
            return System.Text.RegularExpressions.Regex.IsMatch(input ?? "", pattern);
        }

        public static string htmldecode(string input) {
            return System.Web.HttpUtility.HtmlDecode(input ?? "");
        }

    }
}

