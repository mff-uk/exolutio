<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="2.0">
    
    <xsl:template match="departments">
        <xsl:variable select="department[2]" name="self" />
        X
        <xsl:value-of select="./department[. = $self]/name"/>
        
        <xsl:value-of select="for $self2 in department[2] return 
                              ./department[. = $self2]/name"/>
        X
    </xsl:template>
    
    
    
</xsl:stylesheet>