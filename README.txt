STELETO - A simpler 'lite' version of the original STELLAR application for delimited 
text data conversion. Converts tabular input data to any textual output format via a 
custom (user-defined) template. This source code has some external dependencies:
* Antlr4.StringTemplate.dll (template engine, see http://www.stringtemplate.org/)
* FileHelpers.dll (delimited file parser, see http://www.filehelpers.com/)

Some code included here may be derived from work already in the public domain. 
Where this is the case it will be indicated in comments in the source code. This work 
is licensed under the Creative Commons Attribution 3.0 Unported License (CC-BY). 
To view a copy of this license, visit http://creativecommons.org/licenses/by/3.0/ 

Example usage (STELETO -h for help):
STELETO -i:"c:\tmp\input.csv" -o:"c:\tmp\output.txt" -t:"c:\tmp\template.stg" -f -d:"," -p:name:value

The options may be present in any order, and may be expressed: 
-name --name /name -name:value -name=value -name="value"  
For further details of usage see the Wiki

STELETO will convert the input delimited data file data using the named template and 
write to the output file The StringTemplate group file has the following general format
(see http://www.stringtemplate.org/):

// comment, start of template group file "mytemplate.stg" 
// (note the template file must have the *.stg extension)
delimiters "$", "$" // choose start/end characters to indicate data items 
HEADER(options) ::= <<
STELETO calls the 'HEADER' template once at the start of processing. 
All templates are optional; if not present then nothing qill be written! 
Writes any text once only at the start of the output e.g. a report header
>>

RECORD(data, options) ::= <<
Example syntax for a multi-line template. STELETO calls the 'RECORD' template 
once per record of the input data. Any text present here is written to the output. 
Named values from the current record are in 'data' and identified using the delimiters 
as specified at the top e.g. $data.myfield$ writes the value of a particular named field.
If there is no header row in the input data file the data items will be simply named 
"field1", "field2" etc. Additional named parameters are in 'options' - e.g. calling 
STELETO -p:language:en --> $options.language$ anywhere within the template would write 
the value 'en'
>>
// example syntax of a single line template 
FOOTER(options) ::= "STELETO calls the 'FOOTER' template once at the end"
// comment, end of template group file
