﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
  <xsl:output method="xml" indent="yes" />
  <!--PSMClass: "Purchase"-->
  <xsl:template match="/purchase">
    <purchase>
      <xsl:call-template name="TOP-Purchase-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-Purchase-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </purchase>
  </xsl:template>
  <!--End of: PSMClass: "Purchase"-->
  <xsl:template name="TOP-Purchase-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Purchase.purchase-date" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'purchase-date']" />
    <!--ref PSMClass: "Item"-->
    <xsl:for-each-group select="$ci[name() = 'subcode'] | $ci[name() = 'code'] | $ci[name() = 'weight'] | $ci[name() = 'title'] | $ci[name() = 'amount'] | $ci[name() = 'unit-price']" group-starting-with="code">
      <xsl:call-template name="TOP-Purchase-Item-ATT">
        <xsl:with-param name="ci" select="current-group()" />
      </xsl:call-template>
    </xsl:for-each-group>
  </xsl:template>
  <xsl:template name="TOP-Purchase-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "Item"-->
    <xsl:for-each-group select="$ci[name() = 'subcode'] | $ci[name() = 'code'] | $ci[name() = 'weight'] | $ci[name() = 'title'] | $ci[name() = 'amount'] | $ci[name() = 'unit-price']" group-starting-with="code">
      <xsl:call-template name="TOP-Purchase-Item-ELM">
        <xsl:with-param name="ci" select="current-group()" />
      </xsl:call-template>
    </xsl:for-each-group>
    <!--ref PSMClass: "CustomerInfo"-->
    <xsl:call-template name="TOP-Purchase-CustomerInfo-ELM">
      <xsl:with-param name="ci" select="$ci[name() = 'city'] | $ci[name() = 'zip'] | $ci[name() = 'street'] | $ci[name() = 'customer-no'] | $ci[name() = 'email']" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "ItemI"-->
    <xsl:call-template name="TOP-Purchase-Item-ItemI-ATT">
      <xsl:with-param name="ci" select="$ci[name() = 'amount'] | $ci[name() = 'unit-price']" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "Product"-->
    <xsl:call-template name="TOP-Purchase-Item-Product-ELM">
      <xsl:with-param name="ci" select="$ci[name() = 'subcode'] | $ci[name() = 'code'] | $ci[name() = 'weight'] | $ci[name() = 'title']" />
    </xsl:call-template>
    <!--ref PSMClass: "ItemI"-->
    <xsl:call-template name="TOP-Purchase-Item-ItemI-ELM">
      <xsl:with-param name="ci" select="$ci[name() = 'amount'] | $ci[name() = 'unit-price']" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="TOP-Purchase-CustomerInfo-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "Address"-->
    <xsl:call-template name="TOP-Purchase-CustomerInfo-Address-ELM">
      <xsl:with-param name="ci" select="$ci[name() = 'city'] | $ci[name() = 'zip'] | $ci[name() = 'street']" />
    </xsl:call-template>
    <!--ref PSMClass: "Customer"-->
    <xsl:apply-templates select="$ci[name() = 'customer-no']" />
    <xsl:apply-templates select="$ci[name() = 'email']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-Product-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Product.subcode" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'subcode']" />
    <!--ref PSMAttribute: "Product.code" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'code']" />
    <!--ref PSMAttribute: "Product.weight" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'weight']" />
    <!--ref PSMAttribute: "Product.title" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'title']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-ItemI-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "ItemI.amountr" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'amount']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-ItemI-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "ItemI.unit-price" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'unit-price']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-CustomerInfo-Address-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Address.city" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'city']" />
    <!--ref PSMAttribute: "Address.zip" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'zip']" />
    <!--ref PSMAttribute: "Address.street" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'street']" />
  </xsl:template>
  <!--PSMAttribute: "ItemI.amountr" 1..1-->
  <xsl:template match="/purchase/amount">
    <xsl:attribute name="amountr">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>
  <!--End of: PSMAttribute: "ItemI.amountr" 1..1-->
  <!--No blue nodes-->
  <!--Green nodes template-->
  <xsl:template match="@purchase-date | customer-no | email | city | zip | street | subcode | code | weight | title | unit-price" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>