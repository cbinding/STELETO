STELETO - A streamlined version of the original STELLAR.Console application for text data conversion
Converts input data to any textual output format via a custom (user-defined) textual template.
NOTE: STELETO v1.0 used the ANTLR StringTemplate engine (same as STELLAR). This was replaced
in STELETO v2.0 with the (more versatile) DotLiquid template engine. This entails slight differences 
in the template syntax (see [http://dotliquidmarkup.org/](http://dotliquidmarkup.org/) for examples) 

Some source code included here may be derived from work already in the public domain. Where this is the case it will be indicated in comments in the source code. This work is licensed under the Creative Commons Attribution 3.0 Unported License (CC-BY). To view a copy of this license, visit [http://creativecommons.org/licenses/by/3.0/](http://creativecommons.org/licenses/by/3.0/) 

Command line options may be present in any order, and may be expressed in a number of ways: 
```
-name:"value" --name:"value" /name:"value"  -name="value" --name="value" /name="value"
```

The available command line options are:
```
-i|input [required] - the name of the input data file (including path)
-o|output [required] - the name of the output data file (including path)
-t|template [required] - the name of the DotLiquid template file (including path)
-d|delimiter [optional] - the delimiter character used in the input data file if applicable (default is tab delimited)
-p|param [optional] - named parameters, passed through to the template file as 'options.name' (usage -p:name:value e.g. -p:age:"42")
-h|header [optional] - presence indicates the first line of the input data file is a header row containing field names
-w|wait [optional] - by default the console would close after processing, this pauses so you can review the output
-?|help - show all available command line options
```

STELETO will convert the input data file data according to the named template and write the result to the output file
The default input data format is delimited text (the default delimiter TAB can be overridden using the -d option e.g. -d:, )
If the input filename extension is *.json then the input data will be assumed to be JSON format
The template file uses the Liquid syntax as described in [http://dotliquidmarkup.org/](http://dotliquidmarkup.org/)
Input data is passed in to the template as an object called 'data'; for delimited text formats this will be an array;
for JSON it may be an object or an array. Any additional named parameters are passed in as an object called 'options'. 
A simple example:

Input data (myinputdata.csv):
```text
description, price, quantity
"item 1", 1.50, 24
"item 2", 2.75, 18
"item 3", 3.67, 6
```

OR input data (myinputdata.json):
```json
[
	{
		"description":"item 1", 
		"price": 1.50, 
		"quantity": 24
	},
	{
		"description":"item 2", 
		"price": 2.75, 
		"quantity": 18
	},
	{
		"description":"item 3", 
		"price": 3.67, 
		"quantity": 6
	}
]
```

Template (mytemplate.liquid):
```html
<html>
<body>
{%- comment -%}Simple example template to create a HTML list of data items{%- endcomment -%}
<h1>{{ options.title }}</h1>
<ul>
{%- for row in data -%}
<li>{{ row.description }}: {{ row.quantity }} @ {{ row.price }}</li>
{%- endfor -%}
</ul>
</body>
</html>
```

Note: the JSON input does not have to be a simple array of objects as in this example, 
it may contain nested objects and arrays which can be referenced within the template, e.g. 
```
<h1>Prices</h1>
<ul>
{%- for item in data.myobject.myproperty.items -%}
<li>{{ item.description }}{{ item.price.net }}</li>
</ul>
```

Command to run:
```
STELETO -i:"c:\path\to\myinputdata.csv" -o:"c:\path\to\myoutputfile.html" -t:"c:\path\to\mytemplate.liquid" -h -d:, -p:title:"My Title"
```
Output file (myoutputfile.html):
```html
<html>
<body>
<h1>My Title</h1>
<ul>
<li>item 1: 24 @ 1.50</li>
<li>item 2: 18 @ 2.75</li>
<li>item 3: 6 @ 3.67</li>
</ul>
</body>
</html>
```