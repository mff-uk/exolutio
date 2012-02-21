@echo off
echo This batch file executes validation using OclX. 
echo Expects 3 paths as parameters: 
echo   - path to the validated document 
echo   - path to the schematron schema
echo   - path where output is written
echo _
echo Equivalent functionality can be achived by running 
echo XProc pipeline 'oclx-schematron-pipeline.xpl' (which is recommended).
echo Transform.exe is part of Saxon distribution, if you are using a 
echo different XSLT processor, you may have to modify the commands 
echo in this batch file accordingly. 
echo _
echo _

Transform.exe -s:%2   -o:%21 -xsl:iso_dsdl_include.xsl 
Transform.exe -s:%21  -o:%22 -xsl:iso_abstract_expand.xsl 
Transform.exe -s:%22  -o:%23 -xsl:iso_svrl_for_xslt2.xsl allow-foreign=true
Transform.exe -s:%23  -o:%24 -xsl:oclx_include.xsl functional=true oclx-import-href=oclX-functional.xsl
Transform.exe -s:%1 -o:%3 -xsl:%24


