<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:sch="http://www.ascc.net/xml/schematron" xmlns:iso="http://purl.oclc.org/dsdl/schematron"
  xmlns:svrl="http://purl.oclc.org/dsdl/svrl" version="1.0">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <title/>
      </head>
      <body>
        <xsl:apply-templates/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="svrl:text">
    <h2 align="center">
      <xsl:apply-templates/>
    </h2>
  </xsl:template>
  <xsl:template match="svrl:text[2]">
    <h3 align="left">
      <font color="magenta">
        <xsl:value-of select="substring-after(.,
'/')"/>
      </font>
    </h3>
  </xsl:template>

  <xsl:template match="svrl:failed-assert">
    <xsl:choose>
      <xsl:when test="@flag = 'fatal'">
        <p>
          <font color="red"><font color="green"><xsl:value-of select="@line"/>-<xsl:value-of
                select="@column"/>: </font>ERROR: <xsl:value-of select="svrl:text"/></font>
        </p>
      </xsl:when>
      <xsl:when test="@flag = 'warning'">
        <p>
          <font color="blue"><font color="green"><xsl:value-of select="@line"/>-<xsl:value-of
                select="@column"/>: </font>Warning: <xsl:value-of select="svrl:text"/></font>
        </p>
      </xsl:when>
      <xsl:otherwise>
        <p>
          <font color="red"><font color="green"><xsl:value-of select="@line"/>-<xsl:value-of
            select="@column"/>: </font>ERROR: <xsl:value-of select="svrl:text"/></font>
        </p>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
