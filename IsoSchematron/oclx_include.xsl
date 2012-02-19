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
      <xd:p>
        The stylesheet modifies the XSLT generated 
        by schematron pipeline. It Adds namespaces 
        and imports for OclX library (it is possible
        to choose between the functional and 
        dynamic evaluation version). The location
        from which the library is be imported 
        should be passed as the value of 
        oclx-import-href parameter. 
      </xd:p>
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
  <xsl:param name="oclx-import-href" select="'../OclX/oclx-functional.xsl'" as="xs:string" />
   
  <xsl:namespace-alias stylesheet-prefix="axsl" result-prefix="xsl"/>

  <xd:doc>
    <xd:desc>
      <xd:p></xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="xsl:stylesheet">
    <xsl:variable name="mode_functional" select="xs:boolean($functional)" as="xs:boolean" />
    <xsl:copy>
      <xsl:copy-of select="@*[not(. is ../@version)]" />
      <xsl:choose>
        <xsl:when test="not($mode_functional)">
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
        <xsl:when test="$mode_functional">          
          <xsl:attribute name="version" select="'3.0'" />    
        </xsl:when>
        <xsl:otherwise>
          <xsl:namespace name="saxon" select="'http://saxon.sf.net/'" />
          <xsl:attribute name="version" select="'2.0'" />
        </xsl:otherwise>
      </xsl:choose>
      
      <axsl:import href="{$oclx-import-href}" />
      
      <xsl:choose>
        <xsl:when test="$mode_functional">
          <xsl:apply-templates mode="functional"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates mode="dynamic"/>
        </xsl:otherwise>
      </xsl:choose>     
    </xsl:copy>    
  </xsl:template>

  <!-- templates for functional OclX --> 

  <xd:doc>
    <xd:desc>
      <xd:p>Define the variable <xd:i>$self</xd:i> in each rule. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="xsl:template[svrl:fired-rule]" exclude-result-prefixes="svrl" mode="functional">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <axsl:variable name="self" as="item()" select="current()" />    
      <xsl:apply-templates mode="#current" />
    </xsl:copy>
  </xsl:template>
  
  <xd:doc>
    <xd:desc>
      <xd:p>Schematron puts test expression into the attribute of test
        of 
        <xd:i>svrl:failed-assert</xd:i> literal. Since svrl:failed-assert@test is
        just an ordinary attribute, the occurences of '{' and '}' must be doubled,
        otherwise XSLT parser will interpret them as attribute value template
        delimiters.
      </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="svrl:failed-assert" mode="functional">
    <xsl:copy>
      <xsl:copy-of select="@*[not(. is ../@test)]" />
      <xsl:attribute name="test" select="replace(replace(@test, '\{', '{{'), '\}', '}}')"/>   
      <xsl:apply-templates mode="#current" />
    </xsl:copy>
  </xsl:template>

  <!-- templates for dynamic evaluation OclX -->
  
  <xd:doc>
    <xd:desc>
      <xd:p>Define the variable <xd:i>$variables</xd:i> in each rule. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="xsl:template[svrl:fired-rule]" exclude-result-prefixes="svrl" mode="dynamic">
    <xsl:copy>
      <xsl:copy-of select="@*" />      
      <axsl:variable name="self" as="item()" select="current()" />
      <xsl:apply-templates mode="#current" />
    </xsl:copy>
  </xsl:template>

  <xd:doc>
    <xd:desc>
      <xd:p>Identity transform for the rest. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="@*|node()" mode="#all">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" mode="#current"/>
    </xsl:copy>
  </xsl:template> 
  
</xsl:stylesheet>