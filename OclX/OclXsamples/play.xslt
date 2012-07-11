<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs" version="2.0">
  <xsl:output indent="yes"></xsl:output>
 
  <xsl:template match="/purchase">
    <xsl:copy>
      <xsl:apply-templates select="code"/>
      <xsl:apply-templates select="date"/>
      <xsl:apply-templates select="status"/>
      <totalPrice>        
        <xsl:value-of select="sum(item/price)"/>
      </totalPrice>
      <xsl:apply-templates select="item"/>      
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="/purchase/item">
    <item>
      <xsl:apply-templates select="code"/>      
    </item>    
  </xsl:template>  
  
  <xsl:template match="code | date | status">
    <xsl:copy-of select="."/>    
  </xsl:template>
  
  <!--
  

  <xsl:template match="/">
    <xsl:variable name="v1" as="item()*">
      <xsl:sequence select="a"/>
    </xsl:variable>
    <xsl:variable name="v2" as="item()*" select="a"/>


    <xsl:text>v1: </xsl:text>
    <xsl:apply-templates select="$v1" mode="out"/>
    <xsl:text>&#xa;</xsl:text>

    <xsl:text>v2: </xsl:text>
    <xsl:apply-templates select="$v2" mode="out"/>
  </xsl:template>


  <xsl:template match="*" mode="out">
    <xsl:value-of select="name()"/>
    <xsl:apply-templates mode="#current"/>
  </xsl:template>


  <xsl:template match="text()" mode="#all"/>-->
</xsl:stylesheet>
