<?xml version="1.0" encoding="utf-8"?>
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
  </xsl:template>
  <xsl:template name="TOP-Purchase-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "Item"-->
    <xsl:apply-templates select="$ci[name() = 'item']" />
    <!--ref PSMClass: "CustomerInfo"-->
    <xsl:apply-templates select="$ci[name() = 'customer-info']" />
  </xsl:template>
  <!--PSMClass: "Item"-->
  <xsl:template match="/purchase/item">
    <item>
      <xsl:call-template name="TOP-Purchase-Item-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-Purchase-Item-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </item>
  </xsl:template>
  <!--End of: PSMClass: "Item"-->
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
    <xsl:apply-templates select="$ci[name() = 'product']" />
    <!--ref PSMClass: "ItemI"-->
    <xsl:call-template name="TOP-Purchase-Item-ItemI-ELM">
      <xsl:with-param name="ci" select="$ci[name() = 'amount'] | $ci[name() = 'unit-price']" />
    </xsl:call-template>
  </xsl:template>
  <!--PSMClass: "CustomerInfo"-->
  <xsl:template match="/purchase/customer-info">
    <customer-info>
      <xsl:call-template name="TOP-Purchase-CustomerInfo-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </customer-info>
  </xsl:template>
  <!--End of: PSMClass: "CustomerInfo"-->
  <xsl:template name="TOP-Purchase-CustomerInfo-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMClass: "Address"-->
    <xsl:apply-templates select="$ci[name() = 'address']" />
    <!--ref PSMClass: "Customer"-->
    <xsl:apply-templates select="$ci[name() = 'customer']" />
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
    <!--ref PSMAttribute: "Product.subcode" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'subcode']" />
    <!--ref PSMAttribute: "Product.code" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'code']" />
    <!--ref PSMAttribute: "Product.weight" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'weight']" />
    <!--ref PSMAttribute: "Product.title" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'title']" />
    <!--ref PSMClass: "ArbitraryGroupNode"-->
    <xsl:for-each-group select="$ci[name() = 'groupatt'] | $ci[name() = 'child2'] | $ci[name() = 'child1']" group-starting-with="groupatt">
      <xsl:call-template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-ELM">
        <xsl:with-param name="ci" select="current-group()" />
      </xsl:call-template>
    </xsl:for-each-group>
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
  <!--PSMClass: "Address"-->
  <xsl:template match="/purchase/customer-info/address">
    <address-d>
      <xsl:call-template name="TOP-Purchase-CustomerInfo-Address-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </address-d>
  </xsl:template>
  <!--End of: PSMClass: "Address"-->
  <xsl:template name="TOP-Purchase-CustomerInfo-Address-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "Address.city" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'city']" />
    <!--ref PSMAttribute: "Address.zip" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'zip']" />
    <!--ref PSMAttribute: "Address.street" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'street']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "ArbitraryGroupNode.groupatt" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'groupatt']" />
    <!--ref PSMClass: "class2"-->
    <xsl:apply-templates select="$ci[name() = 'child2']" />
    <!--ref PSMClass: "class1"-->
    <xsl:apply-templates select="$ci[name() = 'child1']" />
  </xsl:template>
  <!--PSMAttribute: "ItemI.amountr" 1..1-->
  <xsl:template match="/purchase/item/amount">
    <xsl:attribute name="amountr">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>
  <!--End of: PSMAttribute: "ItemI.amountr" 1..1-->
  <!--PSMClass: "class2"-->
  <xsl:template match="/purchase/item/product/child2">
    <child2>
      <xsl:call-template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class2-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class2-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </child2>
  </xsl:template>
  <!--End of: PSMClass: "class2"-->
  <xsl:template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class2-ATT">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "class2.nodeAttAr" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'nodeAttA']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-Item-Product-ArbitraryGroupNode-class2-ELM">
    <xsl:param name="ci" as="item()*" />
    <!--ref PSMAttribute: "class2.nodeAttEr" 1..1-->
    <xsl:apply-templates select="$ci[name() = 'nodeAttE']" />
  </xsl:template>
  <!--PSMAttribute: "class2.nodeAttAr" 1..1-->
  <xsl:template match="/purchase/item/product/child2/@nodeAttA">
    <xsl:attribute name="nodeAttAr">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>
  <!--End of: PSMAttribute: "class2.nodeAttAr" 1..1-->
  <!--PSMAttribute: "class2.nodeAttEr" 1..1-->
  <xsl:template match="/purchase/item/product/child2/nodeAttE">
    <nodeAttEr>
      <xsl:value-of select="." />
    </nodeAttEr>
  </xsl:template>
  <!--End of: PSMAttribute: "class2.nodeAttEr" 1..1-->
  <!--No blue nodes-->
  <!--Green nodes template-->
  <xsl:template match="customer | child1 | @purchase-date | customer-no | email | city | zip | street | subcode | code | weight | title | unit-price | groupatt | @a1" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>