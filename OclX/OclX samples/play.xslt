<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    version="2.0">
    <xsl:template match="/">
        Root
        <xsl:copy-of select="." />
    </xsl:template>    
    
</xsl:stylesheet>