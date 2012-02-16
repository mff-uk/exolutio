<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
  xmlns:svrl="http://purl.oclc.org/dsdl/svrl"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
  xmlns:axsl="http://www.w3.org/1999/XSL/TransformAlias" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="xd svrl"
  version="2.0">
  
  <xd:doc scope="stylesheet">
    <xd:desc>
      <xd:p><xd:b>Created on:</xd:b> Feb 16, 2012</xd:p>
      <xd:p><xd:b>Author:</xd:b> Jakub</xd:p>
      <xd:p></xd:p>
    </xd:desc>
  </xd:doc>
  
  <xsl:output indent="yes" method="xml" />
  
  <xd:doc>
    <xd:desc>
      <xd:p><xd:i>'true'</xd:i>(default) or <xd:i>'false'</xd:i>. 
        When set to 'true', namespace for OclX with higher-order
        functions will be referenced (requires XSLT 3.0 processor). 
        Otherwise, namespace for OclX with dynamic evaluation 
        will be referenced. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:param name="functional" select="'true'" as="xs:string"/>
  <xd:doc>
    <xd:desc>
      <xd:p>URI pointing to
        main OclX library stylesheet. Default is 
        'http://eXolutio.com/oclX/functional'.</xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:param name="oclx-import-href" select="$functional" as="xs:string" />
  
  
    
  <xsl:namespace-alias stylesheet-prefix="axsl" result-prefix="xsl"/>

  <xd:doc>
    <xd:desc>
      <xd:p></xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="xsl:stylesheet">
    <xsl:copy>
      <xsl:copy-of select="@*[not(. is ../@version)]" />
      <xsl:choose>
        <xsl:when test="not(xs:boolean($functional))">
          <xsl:namespace name="oclX" select="'http://eXolutio.com/oclX/dynamic'" />
          <xsl:namespace name="oclXin" select="'http://eXolutio.com/oclX/dynamic/internal'" />    
        </xsl:when>
        <xsl:otherwise>
          <xsl:namespace name="oclX" select="'http://eXolutio.com/oclX/functional'" />
          <xsl:namespace name="oclXin" select="'http://eXolutio.com/oclX/functional/internal'" />
        </xsl:otherwise>
      </xsl:choose>      
      <xsl:namespace name="oclDate" select="'http://eXolutio.com/oclX/types/date'" />
      <xsl:namespace name="oclString" select="'http://eXolutio.com/oclX/types/string'" />
      <xsl:choose>
        <xsl:when test="xs:boolean($functional)">          
          <xsl:attribute name="version" select="'3.0'" />    
        </xsl:when>
        <xsl:otherwise>
          <xsl:namespace name="saxon" select="'http://saxon.sf.net/'" />
          <xsl:attribute name="version" select="'2.0'" />
        </xsl:otherwise>
      </xsl:choose>
      
      <axsl:import href="{$oclx-import-href}" />
      
      <xsl:apply-templates />
    </xsl:copy>    
  </xsl:template>

  <xd:doc>
    <xd:desc>
      <xd:p></xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="xsl:template[svrl:fired-rule]" exclude-result-prefixes="svrl">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <axsl:variable name="variables" as="item()*" select="oclX:vars(.)" />
      <xsl:apply-templates />
    </xsl:copy>
  </xsl:template>

  <xd:doc>
    <xd:desc>
      <xd:p></xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template> 
  
</xsl:stylesheet>