<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
  <!-- Template generated by eXolutio on 24.7.2011 8:37 
       from D:\Programování\XCase\ExolutioExportTest\MultiplicityAC.eXo. -->
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
    <xsl:call-template name="TOP-Person-SEQemails" />
  </xsl:template>
  <!--PSMContentModel: Sequence-->
  <xsl:template name="TOP-Person-SEQemails">
    <emails>
      <xsl:call-template name="TOP-Person-SEQemails-ELM" />
    </emails>
  </xsl:template>
  <!--End of: PSMContentModel: Sequence-->
  <xsl:template name="TOP-Person-SEQemails-ELM">
    <xsl:apply-templates select="email" />
  </xsl:template>
  <!--No blue nodes-->
  <!--Green nodes template-->
  <xsl:template match="email" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>