<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:oclX="http://eXolutio.com/oclX/dynamic"
    exclude-result-prefixes="saxon xs oclX"
    version="3.0">
    
  <saxon:import-query 
    href="play.xq" 
    namespace="http://eXolutio.com/oclX/dynamic" 
  />
  
  <xsl:output indent="yes"/>
  <xsl:template match="/">
   <xsl:value-of select="oclX:inc(13)"/>
     
  </xsl:template>
<!--  <xsl:template match="/">
     <xsl:copy-of select="/aa/b/oclX:foo(.)"/>
  </xsl:template>    
    
  <xsl:function name="oclX:foo" as="item()*" >
    <xsl:param name="a" as="item()*" />
    <x1>1</x1>    
    <xsl:copy-of select="$a"/>
    <x2>2</x2>
  </xsl:function>  -->
    
    <!--
  <xsl:stream href="employees.xml">
    <xsl:iterate select="*/employee">
      <xsl:param name="highest-earners" 
        as="map(xs:string, element(employee))" 
        select="map:new()"/>
      <xsl:variable name="this" select="copy-of(.)" as="element(employee)"/> 
      <xsl:next-iteration>
        <xsl:with-param name="highest-earners"
          select="let $existing := $highest-earners($this/department)
          return if ($existing/salary gt $this/salary)
          then $highest-earners
          else map:new($highest-earners, map:entry($this/department, $this))"/>
      </xsl:next-iteration>
      <xsl:on-completion>
        <xsl:for-each select="map:keys($highest-earners)">
          <department name="{.}">
            <xsl:copy-of select="$highest-earners(.)"/>
          </department>
        </xsl:for-each>
      </xsl:on-completion>
    </xsl:iterate>
  </xsl:stream>
  -->
</xsl:stylesheet>