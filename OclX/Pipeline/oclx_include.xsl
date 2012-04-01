<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
  xmlns:svrl="http://purl.oclc.org/dsdl/svrl"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
  xmlns:axsl="http://www.w3.org/1999/XSL/TransformAlias" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:oclX="http://eXolutio.com/oclX"
  
  exclude-result-prefixes="xd svrl oclX"
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
   
  
  <xd:doc>
    <xd:desc>
      <xd:p>If set to true, the XPath assert tests are wrapped using 
        <xd:i>xsl:try</xd:i> instruction, which prevents the XSLT 
      compiler from stopping on dynamic errors. I.e. when a dynamic error
      is encountered, the assert fails and the validation proceeds. </xd:p>
      <xd:p>        
        Note: this settings is only relevant for functional OclX.  
      </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:param name="protect-invalid" select="'false'" as="xs:string" /> 
   
  <xsl:namespace-alias stylesheet-prefix="axsl" result-prefix="xsl"/>

  <xd:doc>
    <xd:desc>
      <xd:p>The template refines the top-level stylesheet element.
      It adds references to OclX, changes the version of the stylesheet 
      to 3.0 when functional OclX is being used and adds namespace for
      Saxon extension when dymaic OclX is being used. 
      </xd:p>
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
      <xsl:namespace name="svrl" select="'http://purl.oclc.org/dsdl/svrl'" />
      <xsl:namespace name="err" select="'http://www.w3.org/2005/xqt-errors'" />
      
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
      <xd:p>Schematron puts test expression into the attribute of test
        of 
        <xd:i>svrl:failed-assert</xd:i> literal. Since svrl:failed-assert@test is
        just an ordinary attribute, the occurences of '{' and '}' must be doubled,
        otherwise XSLT parser will interpret them as attribute value template
        delimiters.
      </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="svrl:failed-assert/attribute::test" mode="functional">
    <xsl:attribute name="test" select="replace(replace(., '\{', '{{'), '\}', '}}')"/>
  </xsl:template>

  <xd:doc>
    <xd:desc>
      <xd:p>When <xd:i>protect-invalid</xd:i> parameter is set to 'true', 
      <xd:i>xsl:try</xd:i> is added around every 
      assert test to prevent it from stopping the validation when 
      a dynamic error is encountered. 
      </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="xsl:choose[preceding-sibling::svrl:fired-rule]" mode="functional">
    <xsl:choose>
      <xsl:when test="$protect-invalid eq 'true'">
        <axsl:try>
          <axsl:choose>
            <xsl:apply-templates select="./(@* | node())" mode="#current" />    
          </axsl:choose>
          <axsl:catch>
            <svrl:failed-assert>
              <xsl:apply-templates select=".//svrl:failed-assert/(node() | @*)" mode="#current" />
              <svrl:text>Assert failed due to expression resulting in 'invalid'.</svrl:text>
              <svrl:text>  - dynamic error occured during evaluation</svrl:text>
              <svrl:text><axsl:sequence select="'  - code: ' || $err:code"/></svrl:text>
              <svrl:text><axsl:sequence select="'  - description: ' || $err:description"/></svrl:text>
              <svrl:text><axsl:sequence select="'  - value: ' || $err:value"/></svrl:text>
            </svrl:failed-assert>            
          </axsl:catch>
        </axsl:try>        
      </xsl:when>
      <xsl:otherwise>          
        <axsl:choose>
          <xsl:apply-templates select="./(@* | node())" mode="#current" />    
        </axsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- templates for dynamic evaluation OclX -->
  
  <xd:doc>
    <xd:desc>
      <xd:p>Define the variable <xd:i>$variables</xd:i> in each rule. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="xsl:template[svrl:fired-rule]" mode="dynamic">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <xsl:choose>
        <xsl:when test="xsl:variable[@select eq 'current()']">          
          <axsl:variable name="variables" as="item()*" select="oclX:vars(., '{xsl:variable[@select eq 'current()']/@name}')" />
        </xsl:when>
        <xsl:otherwise>
          <axsl:variable name="variables" as="item()*" select="oclX:vars(.)" />          
        </xsl:otherwise>
      </xsl:choose> 
      <xsl:apply-templates mode="#current" />
    </xsl:copy>
  </xsl:template>

  <xd:doc>
    <xd:desc>
      <xd:p>Identity transform for the rest. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:template match="@*|node()" mode="#all">
    <xsl:if test="not(local-name() eq 'context-variable')">
      <xsl:copy>      
        <xsl:apply-templates select="(@*|node())" mode="#current"/>      
      </xsl:copy>
    </xsl:if>
  </xsl:template> 
  
  
  
</xsl:stylesheet>