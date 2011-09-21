﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
  <xsl:output method="xml" indent="yes" />
  <!--PSMClass: "R"-->
  <xsl:template match="/root">
    <root>
      <xsl:call-template name="TOP-R-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </root>
  </xsl:template>
  <!--End of: PSMClass: "R"-->
  <xsl:template name="TOP-R-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMContentModel: Choice-->
    <xsl:call-template name="TOP-R-CH1-ELM">
      <xsl:with-param name="ci" select="$ci[name() = 'c1'] | $ci[name() = 'c2']" />
    </xsl:call-template>
    <!--ref PSMContentModel: Choice-->
    <xsl:call-template name="TOP-R-CH2-ELM">
      <xsl:with-param name="ci" select="$ci[name() = 'e3'] | $ci[name() = 'e4']" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="TOP-R-CH1-ELM">
    <xsl:param name="ci" as="item()*" />
    <xsl:choose>
      <xsl:when test="$ci[name() = 'c1']">
        <!--ref PSMClass: "c1"-->
        <xsl:apply-templates select="$ci[name() = 'c1']" />
      </xsl:when>
      <xsl:when test="$ci[name() = 'c2']">
        <!--ref PSMClass: "c2"-->
        <xsl:apply-templates select="$ci[name() = 'c2'][position() le 3]" />
        <xsl:call-template name="TOP-R-CH1-c2-IG">
          <xsl:with-param name="count" select="2 - count($ci[name() = 'c2'])" />
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="TOP-R-CH2-ELM">
    <xsl:param name="ci" as="item()*" />
    <xsl:choose>
      <xsl:when test="$ci[name() = 'e3']">
        <!--ref PSMClass: "c3"-->
        <xsl:for-each-group select="$ci[name() = 'e3']" group-starting-with="e3">
          <xsl:if test="position() le 3">
            <xsl:variable name="ci" select="current-group()" />
            <xsl:apply-templates select="$ci[name() = 'e3']" />
          </xsl:if>
        </xsl:for-each-group>
        <xsl:call-template name="TOP-R-CH2-c3-ELM-IG">
          <xsl:with-param name="count" select="2 - count(e3)" />
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$ci[name() = 'e4'] | .">
        <!--ref PSMClass: "c4"-->
        <xsl:call-template name="TOP-R-CH2-c4-ELM">
          <xsl:with-param name="ci" select="$ci[name() = 'e4']" />
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!--PSMClass: "c2"-->
  <xsl:template match="/root/c2">
    <c2r>
      <xsl:call-template name="TOP-R-CH1-c2-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-R-CH1-c2-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </c2r>
  </xsl:template>
  <!--End of: PSMClass: "c2"-->
  <xsl:template name="TOP-R-CH1-c2-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "c2.a2" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'a2']" />
  </xsl:template>
  <xsl:template name="TOP-R-CH1-c2-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "c2.e2" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'e2']" />
    <!--ref PSMAttribute: "c2.e2m" 1..5-->
    <xsl:apply-templates select="$ci[name() = 'e2m']" />
  </xsl:template>
  <xsl:template name="TOP-R-CH2-c4-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "c4.e4" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'e4']" />
    <!--ref PSMAttribute: "c4.e4n" 1..1-->
    <xsl:call-template name="TOP-R-CH2-c4-e4n-IG" />
  </xsl:template>
  <!-- Instance generators -->
  <xsl:template name="TOP-R-CH2-c4-e4n-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <e4n>e4n<xsl:value-of select="current()" /></e4n>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-R-CH1-c2-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <c2r>
        <xsl:call-template name="TOP-R-CH1-c2-ATT-IG" />
        <xsl:call-template name="TOP-R-CH1-c2-ELM-IG" />
      </c2r>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-R-CH1-c2-ATT-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <xsl:attribute name="a2">a2</xsl:attribute>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-R-CH1-c2-ELM-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <e2>e2</e2>
      <e2m>e2m</e2m>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-R-CH2-c3-ELM-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <e3>e3</e3>
    </xsl:for-each>
  </xsl:template>
  <!--No blue nodes-->
  <!--Green nodes template-->
  <xsl:template match="c1 | @a1 | e1 | @a2 | e2 | e2m | e3 | e4" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>