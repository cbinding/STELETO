# STELETO

STELETO - A simpler 'lite' and cross-platform version of the original STELLAR.Console application (http://github.com/cbinding/stellar) for delimited text data conversions. Converts tabular input data to any textual output format via a custom (user-defined) template.  

Some code included here may be derived from work already in the public domain. Where this is the case it will be indicated in comments in the source code. This work is licensed under the Creative Commons Attribution 3.0 Unported License (CC-BY). To view a copy of this license, visit http://creativecommons.org/licenses/by/3.0/ 

Example usage (STELETO -h for help):  
```
STELETO -i:"c:\path\in.csv" -o:"c:\path\out.txt" -t:"c:\path\template.stg" -f -d:"," -p:name:value
```  
The various parameters may be present in any order, and may be expressed as:  
```
-i --i /i -i:value -i=value -i="value"
```  
[For further usage details and examples please see the Wiki http://github.com/cbinding/STELETO/wiki]

There are no commercial restrictions on using STELETO in your own work, we would be interested to know if/how you are using it, any success stories, ideas, improvements etc.  
Good luck!
