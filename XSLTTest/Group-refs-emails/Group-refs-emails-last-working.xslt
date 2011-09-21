<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
  <xsl:output method="xml" indent="yes" />
  <!--PSMClass: "Person"-->
  <xsl:template match="/Person">
    <Person>
      <xsl:call-template name="TOP-Person-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </Person>
  </xsl:template>
  <!--End of: PSMClass: "Person"-->
  <xsl:template name="TOP-Person-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "wrapper"-->
    <xsl:call-template name="TOP-Person-wrapper" />
  </xsl:template>
  <!--PSMClass: "wrapper"-->
  <xsl:template name="TOP-Person-wrapper">
    <emails>
      <xsl:call-template name="TOP-Person-wrapper-ELM" />
    </emails>
  </xsl:template>
  <!--End of: PSMClass: "wrapper"-->
  <xsl:template name="TOP-Person-wrapper-ELM">
    <!--ref PSMClass: "Person"-->
    <xsl:apply-templates select="email" />
  </xsl:template>
  <!--No blue nodes-->
  <!--Green nodes template-->
  <xsl:template match="email" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>