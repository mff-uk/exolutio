<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:oclDate="http://eXolutio.com/oclX/types/date"  
  exclude-result-prefixes="xs" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  version="2.0">
  
  <xsl:function name="oclDate:after" as="xs:boolean">
    <xsl:param name="date1" />
    <xsl:param name="date2" />    
    <xsl:sequence select="xs:dateTime($date1) ge xs:dateTime($date2)"/>
  </xsl:function>
  
  <xsl:function name="oclDate:before" as="xs:boolean">
    <xsl:param name="date1" />
    <xsl:param name="date2" />    
    <xsl:sequence select="xs:dateTime($date2) ge xs:dateTime($date1)"/>
  </xsl:function>
  
  <xsl:function name="oclDate:getDate" as="xs:date">
    <xsl:param name="dateTime" as="xs:dateTime" />
    <xsl:sequence select="xs:date(format-dateTime($dateTime, '[Y]-[M,2]-[D,2]'))" />
  </xsl:function>
</xsl:stylesheet>