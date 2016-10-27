/*
================================================================================
Creator : Ceri Binding, University of South Wales
Project	: STELETO
Classes	: STELETO.Data.API
Summary	: Data conversion functionality
License : http://creativecommons.org/licenses/by/3.0/
================================================================================
History :

12/01/2011  CFB Created classes
21/10/2011  CFB Added functionality to pass options file data to STG templates
================================================================================
*/
using System; //test
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using FileHelpers;              // external library for CSV I/O
using Antlr4.StringTemplate;    // external library for StringTemplate

namespace STELETO
{    
    //Main functionality for STELETO app
    public static class API
    {
        // Convert CSV file to XML file        
        public static int Delimited2XML(string fileName = "", string xmlFileName = "", string delimiter = "\t", bool hasHeader = false)
        {
            //Tidy up input parameters
            fileName = fileName.Trim();
            xmlFileName = xmlFileName.Trim();

            //If XML file name not passed in, generate it from CSV file name
            if (xmlFileName == String.Empty)
                xmlFileName = fileName + ".xml";

            DataTable dt = Delimited2DT(fileName, delimiter, hasHeader);
            //dt.TableName = "STELETO"; //otherwise can't serialize the datatable
            //dt.WriteXml(xmlFileName, XmlWriteMode.WriteSchema); //not right, need STELETO format...
            return DT2XML(dt, xmlFileName);
        }
               
        /// <summary>Read a delimited data file to a DataTable</summary>
        /// <param name="fileName">The name of the delimited file to read, including absolute or relative path as appropriate</param>
        /// <param name="delimiter">Delimiter character to be used</param>
        /// <param name="hasHeader">Is there a header row containing column names? If not they will be generated automatically</param>
        /// <param name="maxRows">Only read this many rows (useful for testing with larger files)</param>
        /// <returns>The number of records processed</returns>
        /// <exception cref="System.ArgumentException">Throws an exception if delimited file name is not supplied</exception>
        /// <exception cref="System.Exception">Throws an exception if delimited file name does not exist or cannot be accessed</exception>
        public static DataTable Delimited2DT(string fileName="", string delimiter="\t", bool hasHeader=true, int maxRows=-1)
        {
            //Fail if fileName not passed in
            if (fileName == String.Empty)
            {
                throw new ArgumentException("input data file name required", "fileName");
            }
            //Set up the delimited file reader 
            DelimitedFileEngine<DelimitedRow> engine = new DelimitedFileEngine<DelimitedRow>();
            engine.Options.Delimiter = delimiter.ToString();
            engine.Options.IgnoreEmptyLines = true;
            //engine.Options.IgnoreCommentedLines = true; 

            DataTable dt = new DataTable();
            long recordCount = 0;
            DelimitedRow[] rows;

            if (!System.IO.File.Exists(fileName))
            {
                throw new Exception(string.Format("Problem finding file {0}?", fileName));
            }
            try
            {
                rows = engine.ReadFile(fileName, maxRows);
                //TODO: Need to allow reading a file that is already open elsewhere
                //System.IO.TextReader tr1 = new System.IO.StreamReader(fileName);
                //System.IO.TextReader tr2 = System.IO.File.OpenText(fileName);
                //rows = engine.ReadStream(tr,maxRows);
                //tr.Close();                
            }
            catch (System.Exception ex)
            {
                throw new Exception(string.Format("Problem reading '{0}'? {1}", fileName, ex.Message), ex);
            }

            foreach (DelimitedRow row in rows)
            {
                //Is this the first row?
                if (recordCount == 0)
                {
                    if (hasHeader)
                    {
                        // First row contains column names    
                        foreach (String s in row.fieldValues)
                        {
                            if (s != null && s.Trim().Length > 0 && !dt.Columns.Contains(s))
                                dt.Columns.Add(getValidColumnName(s), typeof(String));
                            else
                                dt.Columns.Add(getNextColumnName(dt), typeof(String));
                        }
                    }
                    else
                    {
                        // First row just contains values so create default column names for each field
                        //TODO: there's a CHANCE that first row may not have as many values as there are columns
                        //not sure how to guard against that yet...
                        foreach (String s in row.fieldValues)
                        {
                            dt.Columns.Add(getNextColumnName(dt), typeof(String));
                        }
                        dt.Rows.Add(row.fieldValues);
                    }
                }
                else //not first row
                {
                    //still a CHANCE that we have more or less field values than DataTable columns
                    //Ensure that what's being added will match up with DataTable.Columns.Count, however
                    //this may hide errors where a field value contains commas and is incorrectly parsed
                    String[] fieldValues = new String[dt.Columns.Count];

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (i < row.fieldValues.Length)
                            fieldValues[i] = row.fieldValues[i];
                    }
                    //fieldValues.Length should now match DataTable.Columns.Count

                    //Strip leading and trailing quotes if present 
                    //(CSV engine should be doing this but doesn't seem to)                       
                    for (int i = 0; i < fieldValues.Length; i++)
                    {
                        if ((fieldValues[i] != null) &&
                            (fieldValues[i].StartsWith("\"") &&
                            (fieldValues[i].EndsWith("\""))))
                            fieldValues[i] = fieldValues[i].Substring(1, fieldValues[i].Length - 2);

                        //05/09/12 - UNICODE?
                        //if (!fieldValues[i].IsNormalized())
                        //fieldValues[i] = EscapeUnicode(fieldValues[i]);

                    }

                    dt.Rows.Add(fieldValues);
                }
                recordCount++;
                if (maxRows >= 0 && recordCount >= maxRows)
                    break;
            }
            return dt;
        }

        // 05/09/12 cater for Unicode
        public static string EscapeUnicode(string input)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsSurrogatePair(input, i))
                {
                    builder.Append("\\U" + char.ConvertToUtf32(input, i).ToString("X8"));
                    i++;  //skip the next char     
                }
                else
                {
                    int charVal = char.ConvertToUtf32(input, i);
                    if (charVal > 127)
                    {
                        builder.Append("\\u" + charVal.ToString("X4"));
                    }
                    else
                    {
                        //an ASCII character 
                        builder.Append(input[i]);
                    }
                }
            }

            return builder.ToString();
        }
        //end 05/09/12

        /* 
         * <data>
         *      <row id="1">
         *          <col name="col1">Value1</col>
         *          <col name="col2">Value2</col>
         *      </row>
         *      <row id="2">
         *          <col name="col1">Value3</col>
         *          <col name="col2">Value4</col>
         *      </row>
         * </data>
         */
        /// <summary>Alternative to default XML writing of DataTable - STELETO specific format
        /// <param name="table">The DataTable to be used</param>
        /// <param name="xmlFileName">The name of the XML file to write, including absolute or relative path as appropriate</param>
        /// <returns>The number of records processed</returns>
        /// <exception cref="System.ArgumentException">Throws an exception if xmlFileName is not supplied</exception>
        public static int DT2XML(DataTable table, String xmlFileName)
        {
            //tidy up input parameters
            xmlFileName = xmlFileName.Trim();

            //Fail if xmlFileName not passed in
            if (xmlFileName == String.Empty)
                throw new ArgumentException("file name required", "xmlFileName");

            //System.IO.StringWriter sw = new StringWriter();
            //XmlTextWriter tw = new XmlTextWriter(xmlFileName, Encoding.UTF8);
            XmlTextWriter tw = new XmlTextWriter(xmlFileName, Encoding.Unicode);
            tw.Formatting = Formatting.Indented;
            tw.WriteStartDocument();
            tw.WriteStartElement("data");
            //tw.WriteAttributeString("source", xmlFileName);
            int rowCount = 0;
            foreach (DataRow dr in table.Rows)
            {
                rowCount++;
                tw.WriteStartElement("row");
                tw.WriteAttributeString("id", rowCount.ToString());

                for (int i = 0; i < dr.ItemArray.Length; i++)
                {
                    tw.WriteStartElement("col");
                    tw.WriteAttributeString("name", table.Columns[i].ColumnName);
                    tw.WriteString(dr[i].ToString().Trim());
                    tw.WriteEndElement(); //col
                }
                tw.WriteEndElement(); //row                
            }
            tw.WriteEndElement(); //data

            tw.Flush();
            tw.Close();
            return rowCount;
        }

        /// <summary>Write the contents of a DataTable to a delimited file</summary>
        /// <param name="table">The DataTable to be used</param>
        /// <param name="fileName">The name of the delimited file to write, including absolute or relative path as appropriate</param>
        /// <param name="delimiter">Delimiter character to be used</param>
        /// <returns>The number of records processed</returns>
        /// <exception cref="System.ArgumentException">Throws an exception if output file name is not supplied</exception>
        public static int DT2Delimited(DataTable table, String fileName, char delimiter)
        {
            //Tidy up input parameters
            fileName = fileName.Trim();

            //Fail if fileName not passed in
            if (fileName == String.Empty)
                throw new ArgumentException("file name required", "fileName");

            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false);

            //Write delimited header row of column names
            String colNames = "";
            foreach (DataColumn dc in table.Columns)
            {
                String colName = dc.ColumnName;
                //16/01/12 convert any double quotes to single quotes
                colName = colName.Replace('"', '\'');
                //enclose name in double quotes if it CONTAINS the delimiter
                if (colName.Contains(delimiter))
                    colName = "\"" + colName + "\"";

                colNames += colName + delimiter.ToString();
            }
            if (colNames.Length > 0)
            {
                //Remove last delimiter
                colNames = colNames.Remove(colNames.LastIndexOf(delimiter));
                sw.WriteLine(colNames);
            }

            //Write delimited record for each row of data
            int rowCount = 0;
            foreach (DataRow dr in table.Rows)
            {
                rowCount++;
                String rowVals = "";
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    String rowVal = dr.ItemArray[i].ToString();
                    //16/01/12 convert any double quotes to single quotes
                    rowVal = rowVal.Replace('"', '\'');
                    //enclose value in quotes if it CONTAINS the delimiter
                    if (rowVal.Contains(delimiter))
                        rowVal = "\"" + rowVal + "\"";

                    rowVals += rowVal + delimiter.ToString();
                }
                if (rowVals.Length > 0)
                {
                    //Remove last delimiter 
                    rowVals = rowVals.Remove(rowVals.LastIndexOf(delimiter));
                    sw.WriteLine(rowVals);
                }
            }
            sw.Close();
            sw.Dispose();
            return rowCount;
        }
        
        /// <summary>Convert a delimited file using a StringTemplateGroup file</summary>
        /// <param name="fileName">The name of the delimited file, including absolute or relative path as appropriate</param>
        /// <param name="stgFileName">The name of the StringTemplateGroup file, including absolute or relative path as appropriate</param>
        /// <param name="outFileName">The name of the file to write the output to</param> 
        /// <param name="delimiter">Delimiter character for the delimited file</param>       
        /// <returns>The number of records processed</returns>        
        public static int Delimited2STG(string dataFileName="", string stgFileName="", string outFileName="", IDictionary<string,string> options = null, bool hasHeader=true, string delimiter="\t")
        {
            //Tidy up input parameters
            dataFileName = dataFileName.Trim();
            stgFileName = stgFileName.Trim();
            outFileName = outFileName.Trim();
            
            //If output file name not passed in, generate it from delimited file name
            if (outFileName == String.Empty)  outFileName = dataFileName + ".txt";
            // read tabular delimited input data to datatable
            DataTable dt = Delimited2DT(dataFileName, delimiter, true);
            // convert datatable using template, write to output file 
            return DT2STG(dt, stgFileName, outFileName, options);
        }

        /** <summary>
         *  Where to report errors.  All string templates in this group
         *  use this error handler by default.
         *  </summary>
         */
        //class DefaultErrorListener : IStringTemplateErrorListener
        //{
        //    public virtual void Error(string s, Exception e)
        //    {
        //        Console.Error.WriteLine(s);
        //        if (e != null)
        //        {
        //            Console.Error.WriteLine(e.Message);
        //        }
        //    }
        //    public virtual void Warning(string s)
        //    {
        //        Console.Out.WriteLine(s);
        //    }
        //}              

        /// <summary>Convert a System.Data.DataTable using a StringTemplateGroup file</summary>
        /// <param name="table">DataTable to be converted</param>
        /// <param name="stgFileName">The name of the StringTemplateGroup file, including absolute or relative path as appropriate</param>
        /// <param name="outFileName">The name of the file to write the output to</param> 
        /// <param name="optFileName">The name of the file containing options to be passed to templates</param> 
        /// <returns>The number of records processed</returns>     
        /// <exception cref="System.ArgumentException">Throws an exception if stgFileName or outFileName are not supplied</exception>        
        public static int DT2STG(DataTable table, String stgFileName, String outFileName, IDictionary<string,string> options = null)
        {
            //tidy up input parameters
            stgFileName = stgFileName.Trim(); // StringTemplateGroup file
            outFileName = outFileName.Trim(); // Output file
            
            //Fail if stgFileName not passed in
            if (stgFileName == String.Empty)
                throw new ArgumentException("template file name required", "stgFileName");
            //Fail if outFileName not passed in
            if (outFileName == String.Empty)
                throw new ArgumentException("output file name required", "outFileName");
                        
            //Get full path to the STG file, if not already passed in
            String path = System.IO.Path.GetDirectoryName(stgFileName);
            if (path == "")
                stgFileName = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), stgFileName);

            //Revised for Antlr4...Read the template group from the file, define default delimiters
            TemplateGroupFile stg = new TemplateGroupFile(stgFileName, '$', '$');

            //Register renderer for performing Url/Xml Encoding
            stg.RegisterRenderer(typeof(String), new BasicFormatRenderer());


            //System.Collections.ArrayList records = new System.Collections.ArrayList();

            //Write the results to the output file
            int rowCount = 0;
            System.IO.StreamWriter sw = null;
            try
            {
                sw = new System.IO.StreamWriter(outFileName, false);

                // If the HEADER template is present, call it and write result to output file 
                if (stg.IsDefined("HEADER"))
                {
                    Template stHeader = stg.GetInstanceOf("HEADER");
                    stHeader.Add("options", options);
                    sw.WriteLine(stHeader.Render());
                }

                foreach (DataRow dr in table.Rows)
                {
                    IDictionary<string, object> record = new Dictionary<string, object>();
                    foreach (DataColumn dc in table.Columns)
                    {
                        String s = dr[dc].ToString();
                        // Ensure any leading and trailing double quotes are removed..
                        s = trimQuotes(s);
                        // Add cleaned value to the array (if not blank)
                        if (s != "")
                        {
                            record[dc.ColumnName.Trim()] = s;
                        }
                    }
                    // If the RECORD template is present, call it and write result to output file 
                    if (stg.IsDefined("RECORD"))
                    {
                        Template stRecord = stg.GetInstanceOf("RECORD");
                        stRecord.Add("data", record);
                        stRecord.Add("options", options);
                        sw.WriteLine(stRecord.Render());
                    }

                    //records.Add(record);
                    rowCount++;
                }

                // If the FOOTER template is present, call it and write result to output file 
                if (stg.IsDefined("FOOTER"))
                {
                    Template stFooter = stg.GetInstanceOf("FOOTER");
                    stFooter.Add("options", options);
                    sw.WriteLine(stFooter.Render());
                }
            }
            catch (Exception ex)
            {
                //worth catching this?
                throw new Exception("Error during conversion: " + ex.Message, ex.InnerException);
            }
            finally
            {
                sw.Close();
                sw.Dispose();
            }
            return rowCount;
        }

        /// <summary>used in CSV2DT to generate next column name when no column header row exists</summary>
        /// <param name="table">existing DataTable</param>
        /// <returns>Generated column name</returns>  
        private static String getNextColumnName(DataTable table)
        {
            int c = 1;
            while (true)
            {
                String h = "field" + c++;
                if (!table.Columns.Contains(h))
                    return h;
            }
        }

        /// <summary>used in CSV2DT to ensure table/column names are valid - punctuation and spaces replaced with underscores</summary>
        /// <param name="colName">Column name to be checked</param>
        /// <returns>Column name modified as necessary</returns>         
        private static String getValidColumnName(String colName)
        {
            return getValidName(colName);
        }

        private static String getValidTableName(String tblName)
        {
            return getValidName(tblName);
        }

        private static String getValidName(String name)
        {
            //trim, lowercase, replace spaces with underscores
            name = name.Trim().ToLower().Replace(' ', '_');

            //Strip leading and trailing quotes if present
            name = trimQuotes(name);

            //replace any punctuation with underscores
            name = System.Text.RegularExpressions.Regex.Replace(name, @"\p{P}", "_");
            return name;
        }

        // Strip leading and trailing quotes from a string if present
        private static String trimQuotes(String quotedString)
        {
            String s = quotedString.Trim();
            if ((s.StartsWith("\"") && s.EndsWith("\"")) || (s.StartsWith("'") && s.EndsWith("'")))
                s = s.Substring(1, s.Length - 2);
            return s;
        }

    }
}
