<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:saxon="http://saxon.sf.net/"      
  xmlns:oclXin="http://eXolutio.com/oclX/dynamic/internal"
  version="2.0"
  exclude-result-prefixes="xs saxon oclXin"
  >
  
  <xsl:function name="oclXin:appendVarToSequence" as="item()*">
    <xsl:param name="sequence" as="item()*"/>
    <xsl:param name="varName" as="xs:string"/>
    <xsl:param name="varValue" as="item()*" />
    
    <xsl:variable name="newVar" as="item()*">
      <xsl:element name="{$varName}"/>
      <!-- use placeholder ##EMPTY to represente empty sequence -->
      <xsl:choose>
        <xsl:when test="count($varValue) eq 0">
          <xsl:text>##EMPTY</xsl:text>
        </xsl:when>
        <xsl:when test="count($varValue) eq 1">
          <xsl:sequence select="$varValue" />
        </xsl:when>
        <xsl:otherwise>
          <SEQ>
            <xsl:sequence select="for $i in $varValue return oclXin:wrap($i, 'ITEM')" />
          </SEQ>
        </xsl:otherwise>
      </xsl:choose>
      
      <!--<xsl:sequence select="if (exists($varValue)) then $varValue else" />-->
    </xsl:variable>  
    <xsl:sequence select="$sequence, $newVar" />
  </xsl:function>
  
  <xsl:function name="oclXin:wrap">
    <xsl:param name="value" as="item()" />
    <xsl:param name="text" as="xs:string"/>
    <xsl:element name="{$text}">
      <xsl:sequence select="$value" />
    </xsl:element>
  </xsl:function>
    
  <!--<xsl:function name="oclXin:varToSequence" as="item()*">    
    <xsl:param name="varName" as="xs:string"/>
    <xsl:param name="varValue" as="item()*" />
    
    <xsl:variable name="newVar" as="item()*">
      <xsl:element name="{$varName}"/>
      <!-\- use placeholder ##EMPTY to represente empty sequence -\-> 
      <xsl:sequence select="if (exists($varValue)) then $varValue else '##EMPTY'" />
    </xsl:variable>  
    <xsl:sequence select="$newVar" />
  </xsl:function>-->
  
  <!-- 
    Performs variable replacement in the string. 
    substrings as $self, $x, $var1 are replaced for $p1, $p2, $p3... 
    replacements contains sequence defining substrings for replacements, 
    e.g. <self>..</self><x>..</x><var1>..</var1>
  -->
  <xsl:function name="oclXin:expressionForSaxon" as="xs:string">
    <xsl:param name="expression" as="xs:string"/>
    <xsl:param name="replacements" as="item()*" />    
    <xsl:value-of>
      <xsl:variable name="oddMembers" select="oclXin:oddMembers($replacements)" />
      <xsl:analyze-string select="$expression" 
        regex="\$([a-zA-Z][a-zA-Z0-9_]*)">       
        <xsl:matching-substring>        
          <xsl:variable name="trueMatch" select="$oddMembers[name() = regex-group(1)]" as="element()*"/>
          <xsl:sequence select="
            if (exists($trueMatch))
            then (for $index in (1 to count($oddMembers))
            return if ($oddMembers[$index] is $trueMatch) then concat('$p', $index) else ())[1]
            else .
            "/>
        </xsl:matching-substring>
        <xsl:non-matching-substring><xsl:sequence select="."/></xsl:non-matching-substring>
      </xsl:analyze-string>
    </xsl:value-of>      
  </xsl:function>  
    
  <xsl:function name="oclXin:retrieveVarFromSequence" as="item()*">
    <xsl:param name="item" /> 
    <xsl:choose>
      <xsl:when test="xs:string($item) eq '##EMPTY'">
        <xsl:sequence select="()" />
      </xsl:when>
      <xsl:when test="$item instance of element() and local-name($item) eq 'SEQ'">
        <xsl:sequence select="$item/ITEM/child::node()" />    
      </xsl:when>
      <xsl:otherwise>
        <xsl:sequence select="$item" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>   

  <xsl:function name="oclXin:oddMembers" as="item()*">
    <xsl:param name="sequence" as="item()*" />
    <xsl:sequence select="for $i in 1 to (count($sequence) div 2) return $sequence[$i * 2 - 1]" />
  </xsl:function>
  
</xsl:stylesheet>