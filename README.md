# STELETO

STELETO - A simpler 'lite' version of the original STELLAR application for delimited text data conversion. Converts tabular input data to any textual output format via a custom (user-defined) template. This source code has some external dependencies:

* Antlr4.StringTemplate.dll (template engine, see http://www.stringtemplate.org/)
* FileHelpers.dll (delimited file parser, see http://www.filehelpers.com/)

Some code included here may be derived from work already in the public domain. Where this is the case it will be indicated in comments in the source code. This work is licensed under the Creative Commons Attribution 3.0 Unported License (CC-BY). To view a copy of this license, visit http://creativecommons.org/licenses/by/3.0/ 

Example usage (STELETO -h for help):  
```STELETO -i:"c:\path\in.csv" -o:"c:\path\out.txt" -t:"c:\path\template.stg" -f -d:"," -p:name:value```  
The options may be present in any order, and may be expressed:  
```-name --name /name -name:value -name=value -name="value"```  
[For further details of usage please see the Wiki http://github.com/cbinding/STELETO/wiki]
