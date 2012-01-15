"c:\Program Files\Saxonica\SaxonEE9.4N\bin\Transform.exe" -s:D:\Programov n¡\EVOXSVN\SchematronTest\LastSchSchema.sch  -o:D:\Programov n¡\EVOXSVN\SchematronTest\LastSchSchema1.sch -xsl:D:\Programov n¡\EvoXSVN\IsoSchematron\iso_dsdl_include.xsl 
"c:\Program Files\Saxonica\SaxonEE9.4N\bin\Transform.exe" -s:D:\Programov n¡\EVOXSVN\SchematronTest\LastSchSchema1.sch -o:D:\Programov n¡\EVOXSVN\SchematronTest\LastSchSchema2.sch -xsl:D:\Programov n¡\EvoXSVN\IsoSchematron\iso_abstract_expand.xsl 
"c:\Program Files\Saxonica\SaxonEE9.4N\bin\Transform.exe" -s:D:\Programov n¡\EVOXSVN\SchematronTest\LastSchSchema2.sch -o:D:\Programov n¡\EVOXSVN\SchematronTest\LastSchSchema3.xsl -xsl:D:\Programov n¡\EvoXSVN\IsoSchematron\iso_svrl_for_xslt2.xsl allow-foreign=true
"c:\Program Files\Saxonica\SaxonEE9.4N\bin\Transform.exe" -s:D:\Programov n¡\EVOXSVN\SchematronTest\MatchSchedule.xml  -o:D:\Programov n¡\EVOXSVN\SchematronTest\MatchSchedule.svrl -xsl:D:\Programov n¡\EvoXSVN\SchematronTest\LastSchSchema3.xsl 

REM "c:\Program Files\Saxonica\SaxonEE9.4N\bin\Transform.exe" -s:D:\Programov n¡\EVOXSVN\SchematronTest\MatchSchedule.xml  -o:D:\Programov n¡\EVOXSVN\SchematronTest\MatchSchedule.out.xml -xsl:D:\Programov n¡\EvoXSVN\SchematronTest\MatchSchedule.xsl 

