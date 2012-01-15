<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
  exclude-result-prefixes="xd oclXin"
  xmlns:oclXin="http://eXolutio.com/oclX/dynamic/internal"
  version="2.0">
  
  <!-- test OXL syntax -->
  <!--<sch:rule context="/tournament">
    <sch:assert test="matches/day->collect(it, $it/match)->forAll(m, oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)">        
    </sch:assert>
    
  </sch:rule>
  -->
  <xsl:output indent="yes"/>
  
  <xsl:template match="text()">
    <xsl:copy />
  </xsl:template>
  
  
  <xsl:template match="comment()">
    <xsl:copy />
  </xsl:template>
  
  <xsl:function name="oclXin:processOcl" as="item()*">
    <xsl:param name="string" />
    
    <xsl:analyze-string select="$string" regex="(.*)->(.*)">
      <xsl:matching-substring>
        <m>
          <!--<xsl:sequence select="oclXin:processOcl(regex-group(1))" />-->
          <xsl:sequence select="regex-group(1)" />
          ** -> ** 
          <xsl:sequence select="regex-group(2)" />
        </m>
      </xsl:matching-substring>
      <xsl:non-matching-substring>
        <n>
          <xsl:sequence select="."></xsl:sequence>
        </n>
      </xsl:non-matching-substring>
    </xsl:analyze-string>     
  </xsl:function>  
  <xsl:template match="@test">
    <xsl:sequence select="oclXin:processOcl(.)"></xsl:sequence>
     
  </xsl:template>
  
  <xsl:template match="@*">
    <xsl:copy />
  </xsl:template>
  
  
  <xsl:template match="*">
    <xsl:copy>      
      <xsl:apply-templates select="*| @* | text() | comment()"/>
    </xsl:copy>
  </xsl:template>
  
</xsl:stylesheet>