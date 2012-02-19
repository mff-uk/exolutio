<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
  <xsl:output method="xml" indent="yes" />
  <!--PSMClass: "Purchase"-->
  <xsl:template match="/purchaseRS">
    <purchaseRS>
      <xsl:call-template name="TOP-Purchase-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-Purchase-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </purchaseRS>
  </xsl:template>
  <!--End of: PSMClass: "Purchase"-->
  <xsl:template name="TOP-Purchase-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Purchase.code" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'code']" />
    <!--ref PSMAttribute: "Purchase.version" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'version']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Purchase.created-date" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'created-date']" />
    <!--ref PSMAttribute: "Purchase.status" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'status']" />
    <!--ref PSMClass: "Customer"-->
    <xsl:apply-templates select="$ci[name() = 'makes']" />
    <!--ref PSMClass: "items"-->
    <xsl:apply-templates select="$ci[name() = 'items']" />
  </xsl:template>
  <!--No blue nodes-->
  <!--Green nodes template-->
  <xsl:template match="makes | items | item | @code | created-date | @version | status" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>