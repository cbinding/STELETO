/*
================================================================================
Creator : Ceri Binding, University of South Wales
Project	: STELETO
Classes	: STELETO.DelimitedToStg
Summary	: Main console command functionality
License : Creative Commons Attribution http://creativecommons.org/licenses/by/3.0/ 
================================================================================
History :

25/10/2016  CFB Adapted from STELLAR code base, but using Mono.Options
================================================================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STELETO
{   
    public class DelimitedToStg
    {
        protected System.IO.TextReader In = null;
        protected System.IO.TextWriter Out = null;
        protected System.IO.TextWriter Error = null;

        public DelimitedToStg()
        {
            //by default, read from/write to standard streams
            this.In = System.Console.In;
            this.Out = System.Console.Out;
            this.Error = System.Console.Error;
        }

        public void Main(string[] args)
        {
            //application name and version display
            String appName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            //String appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Version appVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            this.Out.WriteLine("\n{0} v{1}.{2}", appName, appVersion.Major, appVersion.Minor);
                
            DateTime started = DateTime.Now;
            
            // variables to hold values of input parameters 
            bool showHelp = false;
            String csvFileName = "";    // delimited data input file name with path
            String stgFileName = "";    // StringTemplate template group file name with path    
            String outFileName = "";    // output file name including path
            String delimiter = "\t";    // default is tab delimited unless present
            String subdelimiter = ";";    // default is ; unless present
            Boolean hasFieldNames = false;  // input data file has header row of column names?
            IDictionary<string, string> extras = new Dictionary<string, string>();  // extra parameters, to pass through to template

            var p = new Mono.Options.OptionSet() {
                { "i|input=", "name of input data {FILE}", v => { if (v != null) csvFileName = v.Trim(); }},
                { "o|output=", "name of output {FILE}", v => { if (v != null) outFileName = v.Trim(); }},
                { "t|template=", "name of template {FILE}", v => { if (v != null) stgFileName = v.Trim(); }},
                { "d|delimiter=", "input file delimiter (default=tab) {STRING}", v => { if (v != null) delimiter = v; }},
                //{ "s|subdelimiter=", "input file sub-delimiter (default=';') {STRING}", v => { if (v != null) subdelimiter = v; }},
                { "p|param={NAME}:{VALUE}", "named parameters to pass to template as {NAME}:{VALUE}", (m, v) => { extras.Add (m, v); }},
                { "f|fields", "first input row contains field names", v => hasFieldNames = v != null },
                { "h|?|help",  "show this message and exit", v => showHelp = v != null },
            };
            
            try
            {
                p.Parse(args);
            }
            catch (Mono.Options.OptionException e)
            {
                this.Out.WriteLine(e.Message);
                this.Out.WriteLine("Type {0} --help' for more information", appName);
                return;
            }

            if (showHelp)
            {
                p.WriteOptionDescriptions(this.Out);
                return;
            }

            // validate parameters prior to processing          
            if(csvFileName =="")
            {
                this.Out.WriteLine("input file name required{1}Type {0} --help' for more information", appName, Environment.NewLine);
                return;
            }
            if (stgFileName == "")
            {
                this.Out.WriteLine("template file name required{1}Type {0} --help' for more information", appName, Environment.NewLine);
                return;
            }
            
            try 
            {
                this.Out.WriteLine("Convert '{0}' with template '{1}'", csvFileName, stgFileName);
                int rowCount = API.Delimited2STG(
                    dataFileName: csvFileName, 
                    stgFileName: stgFileName, 
                    outFileName: outFileName, 
                    options: extras, 
                    hasHeader: hasFieldNames, 
                    delimiter: delimiter);
                TimeSpan elapsed = DateTime.Now.Subtract(started);
                this.Out.WriteLine("{0} rows converted [time taken: {1:00}:{2:00}:{3:00}.{4:000}]", 
                    rowCount, (int)elapsed.TotalHours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
            }
            catch (Exception ex)
            {
                this.Error.WriteLine("Error: {0}", ex.Message);
            }                   
        }        
    }
}