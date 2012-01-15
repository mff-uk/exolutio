<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
    xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
    exclude-result-prefixes="xd dyn"
    xmlns:dyn="http://exslt.org/dynamic"
    xmlns:saxon="http://saxon.sf.net/"
    >
    
    <xsl:output method="xml" indent="yes"/>
    
    <xsl:template match="/">
        <ROOT>
            <xsl:variable name="d" select="saxon:evaluate('//department')" /> 
            <xsl:copy-of select="$d" />
        </ROOT>
    </xsl:template>
    
   
    
</xsl:stylesheet>
    