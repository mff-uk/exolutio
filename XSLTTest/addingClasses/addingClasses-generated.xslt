<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
  <xsl:output method="xml" indent="yes" />
  <!--PSMClass: "Purchase"-->
  <xsl:template match="/purchase-request">
    <purchase-request>
      <xsl:call-template name="TOP-Purchase-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-Purchase-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </purchase-request>
  </xsl:template>
  <!--End of: PSMClass: "Purchase"-->
  <xsl:template name="TOP-Purchase-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Purchase.purchase-date" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'purchase-date']" />
    <!--ref PSMClass: "GroupA"-->
    <xsl:call-template name="TOP-Purchase-GroupA-ATT">
      <xsl:with-param name="ci" select="()" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="TOP-Purchase-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Purchase.id" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'id']" />
    <!--ref PSMClass: "Item"-->
    <xsl:for-each-group select="$ci[name() = 'amount']" group-starting-with="amount">
      <xsl:call-template name="TOP-Purchase-Item">
        <xsl:with-param name="ci" select="current-group()" />
      </xsl:call-template>
    </xsl:for-each-group>
    <!--ref PSMClass: "AddedWithE"-->
    <xsl:call-template name="TOP-Purchase-AddedWithE" />
    <!--ref PSMClass: "AddedWithA"-->
    <xsl:call-template name="TOP-Purchase-AddedWithA" />
    <!--ref PSMClass: "AddedEmpty"-->
    <xsl:call-template name="TOP-Purchase-AddedEmpty" />
    <!--ref PSMClass: "GroupE"-->
    <xsl:call-template name="TOP-Purchase-GroupE-ELM">
      <xsl:with-param name="ci" select="()" />
    </xsl:call-template>
  </xsl:template>
  <!--PSMClass: "Item"-->
  <xsl:template name="TOP-Purchase-Item">
    <xsl:param name="ci" as="item()*" />
    <item>
      <xsl:call-template name="TOP-Purchase-Item-ATT">
        <xsl:with-param name="ci" select="$ci" />
      </xsl:call-template>
    </item>
  </xsl:template>
  <!--End of: PSMClass: "Item"-->
  <xsl:template name="TOP-Purchase-Item-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Item.amount" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'amount']" />
    <!--ref PSMAttribute: "Item.unit-price" 1..1-->
    <xsl:call-template name="TOP-Purchase-Item-unit-price-IG" />
  </xsl:template>
  <!--PSMClass: "AddedWithE"-->
  <xsl:template name="TOP-Purchase-AddedWithE">
    <addedWithE>
      <xsl:call-template name="TOP-Purchase-AddedWithE-ELM" />
    </addedWithE>
  </xsl:template>
  <!--End of: PSMClass: "AddedWithE"-->
  <xsl:template name="TOP-Purchase-AddedWithE-ELM">
    <!--ref PSMAttribute: "AddedWithE.E" 1..1-->
    <xsl:call-template name="TOP-Purchase-AddedWithE-E-IG" />
  </xsl:template>
  <!--PSMClass: "AddedWithA"-->
  <xsl:template name="TOP-Purchase-AddedWithA">
    <addedWithA>
      <xsl:call-template name="TOP-Purchase-AddedWithA-ATT" />
    </addedWithA>
  </xsl:template>
  <!--End of: PSMClass: "AddedWithA"-->
  <xsl:template name="TOP-Purchase-AddedWithA-ATT">
    <!--ref PSMAttribute: "AddedWithA.A" 1..1-->
    <xsl:call-template name="TOP-Purchase-AddedWithA-A-IG" />
  </xsl:template>
  <!--PSMClass: "AddedEmpty"-->
  <xsl:template name="TOP-Purchase-AddedEmpty">
    <addedEmpty />
  </xsl:template>
  <!--End of: PSMClass: "AddedEmpty"-->
  <xsl:template name="TOP-Purchase-GroupA-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "GroupA.ga" 1..1-->
    <xsl:call-template name="TOP-Purchase-GroupA-ga-IG" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-GroupE-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "GroupE.ge" 1..1-->
    <xsl:call-template name="TOP-Purchase-GroupE-ge-IG" />
  </xsl:template>
  <!-- Instance generators -->
  <xsl:template name="TOP-Purchase-Item-unit-price-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <xsl:attribute name="unit-price">unit-price<xsl:value-of select="current()" /></xsl:attribute>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-AddedWithE-E-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <E>E<xsl:value-of select="current()" /></E>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-AddedWithA-A-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <xsl:attribute name="A">A<xsl:value-of select="current()" /></xsl:attribute>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-GroupA-ga-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <xsl:attribute name="ga">ga<xsl:value-of select="current()" /></xsl:attribute>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-GroupE-ge-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <ge>ge<xsl:value-of select="current()" /></ge>
    </xsl:for-each>
  </xsl:template>
  <!--Element to attribute conversion template-->
  <xsl:template match="amount" priority="0">
    <xsl:attribute name="{name()}">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>
  <!--Attribute to element conversion template-->
  <xsl:template match="@id" priority="0">
    <xsl:element name="{name()}">
      <xsl:value-of select="." />
    </xsl:element>
  </xsl:template>
  <!--No blue nodes-->
  <!--Green nodes template-->
  <xsl:template match="@purchase-date" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>