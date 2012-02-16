<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
  xmlns:oclX="http://eXolutio.com/oclX/functional"
  xmlns:oclXin="http://eXolutio.com/oclX/functional/internal" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  exclude-result-prefixes="xd oclX xs oclXin"
  version="3.0">
  
  <!-- strings --> 
  
  <!-- numbers --> 
  
  <!-- boolean --> 
  <xsl:function name="oclX:oclIsUndefined" as="xs:boolean">
    <xsl:param name="item" as="item()*" />
    <xsl:sequence select="not(exists($item))" />
   
  </xsl:function>
  
  <xsl:function name="oclX:oclIsInvalid" as="xs:boolean">
    <xsl:param name="item" as="item()" />
    <xsl:sequence select="false()" />
    
  </xsl:function>
  
  
  
  
</xsl:stylesheet>