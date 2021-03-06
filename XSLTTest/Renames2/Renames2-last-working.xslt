﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
  <xsl:output method="xml" indent="yes" />
  <!--PSMClass: "Purchase"-->
  <xsl:template match="/purchase">
    <purchaseRQ>
      <xsl:call-template name="TOP-Purchase-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-Purchase-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </purchaseRQ>
  </xsl:template>
  <!--End of: PSMClass: "Purchase"-->
  <xsl:template name="TOP-Purchase-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Purchase.date" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'purchase-date']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "CustomerInfo"-->
    <xsl:apply-templates select="$ci[name() = 'customer-info']" />
    <!--ref PSMClass: "Item"-->
    <xsl:apply-templates select="$ci[name() = 'item']" />
  </xsl:template>
  <!--PSMAttribute: "Purchase.date" 1..1-->
  <xsl:template match="/purchase/@purchase-date">
    <xsl:attribute name="date">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>
  <!--End of: PSMAttribute: "Purchase.date" 1..1-->
  <!--PSMClass: "Item"-->
  <xsl:template match="/purchase/item">
    <pur-item>
      <xsl:call-template name="TOP-Purchase-Item-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </pur-item>
  </xsl:template>
  <!--End of: PSMClass: "Item"-->
  <xsl:template name="TOP-Purchase-Item-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "Product"-->
    <xsl:apply-templates select="$ci[name() = 'product']" />
    <!--ref PSMClass: "ItemI"-->
    <xsl:apply-templates select="$ci[name() = 'amount']" />
    <xsl:apply-templates select="$ci[name() = 'unit-price']" />
  </xsl:template>
  <!--PSMClass: "Product"-->
  <xsl:template match="/purchase/item/product">
    <product>
      <xsl:call-template name="TOP-Purchase-Item-Product-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </product>
  </xsl:template>
  <!--End of: PSMClass: "Product"-->
  <xsl:template name="TOP-Purchase-Item-Product-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Product.code" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'code']" />
    <!--ref PSMAttribute: "Product.subcode" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'subcode']" />
    <!--ref PSMAttribute: "Product.title" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'title']" />
    <!--ref PSMAttribute: "Product.weight" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'weight']" />
    <!--ref PSMClass: "ArbitraryGroupNode"-->
    <xsl:for-each-group select="$ci[name() = 'groupatt'] | $ci[name() = 'child1'] | $ci[name() = 'child2']" group-starting-with="groupatt">
      <xsl:call-template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-ELM">
        <xsl:with-param name="ci" select="current-group()" />
      </xsl:call-template>
    </xsl:for-each-group>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "ArbitraryGroupNode.groupatt-N" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'groupatt']" />
    <!--ref PSMAttribute: "ArbitraryGroupNode.newAtt" 1..1-->
    <xsl:call-template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-newAtt-IG" />
    <!--ref PSMClass: "class1"-->
    <xsl:apply-templates select="$ci[name() = 'child1']" />
    <!--ref PSMClass: "class2"-->
    <xsl:apply-templates select="$ci[name() = 'child2']" />
  </xsl:template>
  <!--PSMAttribute: "ItemI.price" 1..1-->
  <xsl:template match="/purchase/item/unit-price">
    <price>
      <xsl:value-of select="." />
    </price>
  </xsl:template>
  <!--End of: PSMAttribute: "ItemI.price" 1..1-->
  <!--PSMAttribute: "ArbitraryGroupNode.groupatt-N" 1..1-->
  <xsl:template match="/purchase/item/product/groupatt">
    <groupatt-N>
      <xsl:value-of select="." />
    </groupatt-N>
  </xsl:template>
  <!--End of: PSMAttribute: "ArbitraryGroupNode.groupatt-N" 1..1-->
  <!--PSMClass: "class1"-->
  <xsl:template match="/purchase/item/product/child1">
    <child1-N>
      <xsl:call-template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class1-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </child1-N>
  </xsl:template>
  <!--End of: PSMClass: "class1"-->
  <xsl:template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class1-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "class1.a1ren" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'a1']" />
  </xsl:template>
  <!--PSMClass: "class2"-->
  <xsl:template match="/purchase/item/product/child2">
    <child2-N>
      <xsl:call-template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class2-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class2-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </child2-N>
  </xsl:template>
  <!--End of: PSMClass: "class2"-->
  <xsl:template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class2-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "class2.nodeAttAren" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'nodeAttA']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class2-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "class2.nodeAttEren" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'nodeAttE']" />
  </xsl:template>
  <!--PSMAttribute: "class1.a1ren" 1..1-->
  <xsl:template match="/purchase/item/product/child1/@a1">
    <xsl:attribute name="a1ren">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>
  <!--End of: PSMAttribute: "class1.a1ren" 1..1-->
  <!--PSMAttribute: "class2.nodeAttEren" 1..1-->
  <xsl:template match="/purchase/item/product/child2/nodeAttE">
    <nodeAttEren>
      <xsl:value-of select="." />
    </nodeAttEren>
  </xsl:template>
  <!--End of: PSMAttribute: "class2.nodeAttEren" 1..1-->
  <!--PSMAttribute: "class2.nodeAttAren" 1..1-->
  <xsl:template match="/purchase/item/product/child2/@nodeAttA">
    <xsl:attribute name="nodeAttAren">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>
  <!--End of: PSMAttribute: "class2.nodeAttAren" 1..1-->
  <!-- Instance generators -->
  <xsl:template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-newAtt-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <newAtt>newAtt<xsl:value-of select="current()" /></newAtt>
    </xsl:for-each>
  </xsl:template>
  <!--No blue nodes-->
  <!--Green nodes template-->
  <xsl:template match="customer | address | customer-info | customer-no | email | zip | city | street | code | subcode | title | weight | amount" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>