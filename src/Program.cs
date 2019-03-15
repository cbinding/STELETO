/*
================================================================================
Creator : Ceri Binding, University of South Wales ceri.binding@southwales.ac.uk
Project	: STELETO
Classes	: STELETO.Program
Summary	: Main entry point for STELETO application
License : http://creativecommons.org/licenses/by/3.0/ 
================================================================================
History :

12/01/2011  CFB Created classes
07/01/2019  CFB Simplified: FileHelpers library replaced by CsvTextFieldParser class; 
                StringTemplate engine replaced by DotLiquid for template flexibility
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
             // display application name and version
            System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName();
            String appName = assemblyName.Name;
            Version appVersion = assemblyName.Version; 
            String appNameAndVersion = String.Format("\n{0} v{1}.{2}", appName, appVersion.Major, appVersion.Minor);
            Console.WriteLine(appNameAndVersion);
            Console.Title = appNameAndVersion;     
           
            // variables to hold values of input parameters 
            bool showHelp = false;
            String inputFileName = "";      // input file name including path
            String outputFileName = "";     // output file name including path            
            String templateFileName = "";   // template file name including path    
            Char delimiter = '\t';          // default tab delimited input unless present
            Boolean hasHeader = false;       // input data file has header row with column names
            Boolean waitAfter = false;      // pause the console after processing (otherwise just exit)
            IDictionary<string, string> extras = new Dictionary<string, string>();  // extra parameters, to pass through to template

            // capture input parameters where passed in
            var p = new Mono.Options.OptionSet() {
                { "i|input=", "name of input data {FILE}", v => { if (v != null) inputFileName = v.Trim(); }},
                { "o|output=", "name of output {FILE}", v => { if (v != null) outputFileName = v.Trim(); }},
                { "t|template=", "name of template {FILE}", v => { if (v != null) templateFileName = v.Trim(); }},
                { "d|delimiter=", "input file delimiter (default=tab) {CHAR}", v => { if (v != null) delimiter = v[0]; }},
                { "p|param={NAME}:{VALUE}", "named parameters to pass to template as {NAME}:{VALUE}", (m, v) => { extras.Add (m, v); }},
                { "h|header", "first row is a header containing field names", v => hasHeader = (v != null) },
                { "w|wait", "wait after processing (by default console will close)", v => waitAfter = (v != null) },
                { "?|help",  "show help for all options", v => showHelp = (v != null) },
            };            
            
            try
            {
                p.Parse(args);
            }
            catch (Mono.Options.OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Type {0} --help' for more information", appName);
                if (waitAfter)
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                }
                return;
            }

            if (showHelp)
            {
                p.WriteOptionDescriptions(Console.Out);
                if (waitAfter)
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                }
                return;
            }

            // validate parameters prior to processing          
            if(inputFileName =="")
            {
                Console.WriteLine("Input file name required{1}Type {0} --help' for more information", appName, Environment.NewLine);
                if (waitAfter)
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                }
                return;
            }
            if (templateFileName == "")
            {
                Console.WriteLine("Template file name required{1}Type {0} --help' for more information", appName, Environment.NewLine);
                if (waitAfter)
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                }
                return;
            }

            //If output file name not passed in, generate it from delimited file name
            if (outputFileName == String.Empty)
                outputFileName = inputFileName + ".txt";

            try
            {
                DateTime started = DateTime.Now;

                Console.WriteLine("Convert '{0}' using template '{1}'", inputFileName, templateFileName);
                int rowCount = Delimited2Liquid(
                    inputFileName: inputFileName,
                    templateFileName: templateFileName,
                    outputFileName: outputFileName,
                    options: extras,
                    hasHeader: hasHeader,
                    delimiter: delimiter);
                TimeSpan elapsed = DateTime.Now.Subtract(started);
                Console.WriteLine("{0} rows converted [time taken: {1:00}:{2:00}:{3:00}.{4:000}]",
                    rowCount, (int)elapsed.TotalHours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                if (waitAfter)
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                }
            }       
                          
        }

        public static int Delimited2Liquid(string inputFileName = "", string templateFileName = "", string outputFileName = "", IDictionary<string, string> options = null, bool hasHeader = true, char delimiter = '\t')
        {
            //Tidy up input parameters
            inputFileName = inputFileName.Trim();
            templateFileName = templateFileName.Trim();
            outputFileName = outputFileName.Trim();

            // Fail if inputFileName not passed in
            if (inputFileName == String.Empty)
                throw new ArgumentException("Input file name required", "inputFileName");

            // Fail if templateFileName not passed in
            if (templateFileName == String.Empty)
                throw new ArgumentException("Template file name required", "templateFileName");        
            
            // Get full path to the input file, if not already passed in
            String path = System.IO.Path.GetDirectoryName(inputFileName);
            if (path == "") 
                inputFileName = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), inputFileName);

            // Get full path to the template file, if not already passed in
            path = System.IO.Path.GetDirectoryName(templateFileName);
            if (path == String.Empty) 
                templateFileName = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), templateFileName);

            // Get full path to the output file, if not already passed in
            path = System.IO.Path.GetDirectoryName(outputFileName);
            if (path == String.Empty) 
                outputFileName = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), outputFileName);
            
            // read tabular delimited input text from input text file
            String inputText = System.IO.File.ReadAllText(inputFileName);

            // parse tabular delimited input text into fields
            IEnumerable<IDictionary<string, string>> parsedData = ParseDelimitedText(inputText, hasHeader, delimiter);

            // read and parse dotLiquid template from template file           
            String templateText = System.IO.File.ReadAllText(templateFileName);
            DotLiquid.Template template = DotLiquid.Template.Parse(templateText);

            // some useful extra information to be passed into the template
            options.Add("STELETO-inputFileName", System.IO.Path.GetFileName(inputFileName));
            options.Add("STELETO-outputFileName", System.IO.Path.GetFileName(outputFileName));
            options.Add("STELETO-templateFileName", System.IO.Path.GetFileName(templateFileName));
            options.Add("STELETO-timestamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));

            // dictionary structure to hold all the stuff to be passed into the template
            IDictionary<string, object> templateData = new System.Collections.Generic.Dictionary<string, object>();
            templateData.Add("data", parsedData);
            templateData.Add("options", options);
              
            try
            {

                // Specialised string functions for templates (see LiquidTextFilter class)
                DotLiquid.Template.RegisterFilter(typeof(LiquidTextFilter)); 

                // render the data according to the template and the options passed in            
                String outputText = template.Render(DotLiquid.Hash.FromDictionary(templateData));

                //Write the rendered results to the output text file
                System.IO.File.WriteAllText(outputFileName, outputText);
            }
            catch (Exception ex)
            {
                // hardly worth catching and throwing again??
                throw new Exception("Error during conversion: " + ex.Message, ex.InnerException);
            }

            // return count of delimited records processed
            return parsedData.Count(); 
        }
        
        private static IEnumerable<IDictionary<string, string>> ParseDelimitedText(string inputText, bool hasHeader = true, char delimiter = '\t')
        {    
	        using (var csvReader = new System.IO.StringReader(inputText))
	        using (var parser = new DelimitedTextFieldParser(csvReader))
	        {
                parser.SetDelimiter(delimiter);                
                //parser.Delimiters = new[] { "|" };
                //parser.SetQuoteCharacter('\"');
                //parser.SetQuoteEscapeCharacter('\\');
                parser.HasFieldsEnclosedInQuotes = true;
                parser.TrimWhiteSpace = true;
                               
                if (!parser.HasNextLine())
		        {
			        yield break;
		        }
		        string[] headerFields;
		        try
		        {
			        headerFields = parser.ReadFields();
		        }
		        catch (CsvMalformedLineException ex)
		        {
			        Console.Error.WriteLine("Failed to parse header line {0}: {1}", ex.LineNumber, parser.ErrorLine);
			        yield break;
		        }
		        //while (parser.EndOfData)
                while(parser.HasNextLine())
		        {
			        string[] fields;
			        try
			        {
				        fields = parser.ReadFields();
                        // if all the fields are empty ignore this record
                        bool allFieldsEmpty = true;
                        foreach (string s in fields) {
                            if (s != String.Empty) {
                                allFieldsEmpty = false;
                                break;
                            }
                         }
                        if (allFieldsEmpty) 
                            continue;
                        
			        }
			        catch (CsvMalformedLineException ex)
			        {
				        Console.Error.WriteLine("Failed to parse line {0}: {1}", ex.LineNumber, parser.ErrorLine);
				        continue;
			        }

			        int fieldCount = Math.Min(headerFields.Length, fields.Length);
			        IDictionary<string, string> fieldDictionary = new Dictionary<string, string>(fieldCount);
			        for (var i = 0; i < fieldCount; i++)
			        {
				        string headerField = headerFields[i];
				        string field = fields[i].Trim();
                        // if the field is not empty add it
                        if (field != String.Empty)
                            fieldDictionary[headerField] = field;
			        }
                    yield return fieldDictionary;
		        }
	        }
            
        }
    }
}
