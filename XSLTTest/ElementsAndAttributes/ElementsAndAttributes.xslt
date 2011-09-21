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
    <xsl:apply-templates select="$ci[name() = 'purchase-date']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-ELM">
    <xsl:param name="ci" as="item()*" />
    <xsl:apply-templates select="$ci[name() = 'customer-info']" />
    <xsl:apply-templates select="$ci[name() = 'assistant']" />
    <xsl:call-template name="TOP-Purchase-Items" />
  </xsl:template>
  <!--PSMClass: "CustomerInfo"-->
  <xsl:template match="/purchase-request/customer-info">
    <customer-info>
      <xsl:call-template name="TOP-Purchase-CustomerInfo-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </customer-info>
  </xsl:template>
  <!--End of: PSMClass: "CustomerInfo"-->
  <xsl:template name="TOP-Purchase-CustomerInfo-ELM">
    <xsl:param name="ci" as="item()*" />
    <xsl:apply-templates select="$ci[name() = 'customer']" />
  </xsl:template>
  <!--PSMClass: "SalesAssistant"-->
  <xsl:template match="/purchase-request/assistant">
    <assistant>
      <xsl:call-template name="TOP-Purchase-SalesAssistant-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </assistant>
  </xsl:template>
  <!--End of: PSMClass: "SalesAssistant"-->
  <xsl:template name="TOP-Purchase-SalesAssistant-ELM">
    <xsl:param name="ci" as="item()*" />
    <xsl:apply-templates select="$ci[name() = 'emp-no']" />
    <xsl:call-template name="TOP-Purchase-SalesAssistant-name-IG" />
  </xsl:template>
  <!--PSMClass: "Items"-->
  <xsl:template name="TOP-Purchase-Items">
    <items>
      <xsl:call-template name="TOP-Purchase-Items-ELM" />
    </items>
  </xsl:template>
  <!--End of: PSMClass: "Items"-->
  <xsl:template name="TOP-Purchase-Items-ELM">
    <xsl:apply-templates select="item[position() le 5]" />
    <xsl:call-template name="TOP-Purchase-Items-Item-IG">
      <xsl:with-param name="count" select="1 - count(item)" />
    </xsl:call-template>
  </xsl:template>
  <!--PSMClass: "Customer"-->
  <xsl:template match="/purchase-request/customer-info/customer">
    <customer>
      <xsl:call-template name="TOP-Purchase-CustomerInfo-Customer-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
      <xsl:call-template name="TOP-Purchase-CustomerInfo-Customer-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </customer>
  </xsl:template>
  <!--End of: PSMClass: "Customer"-->
  <xsl:template name="TOP-Purchase-CustomerInfo-Customer-ATT">
    <xsl:param name="ci" as="item()*" />
    <xsl:apply-templates select="$ci[name() = 'customer-no']" />
  </xsl:template>
  <xsl:template name="TOP-Purchase-CustomerInfo-Customer-ELM">
    <xsl:param name="ci" as="item()*" />
    <xsl:apply-templates select="../address" />
    <xsl:call-template name="TOP-Purchase-CustomerInfo-Customer-CustEmail" />
  </xsl:template>
  <!--PSMClass: "Address"-->
  <xsl:template match="/purchase-request/customer-info/address">
    <delivery-address>
      <xsl:call-template name="TOP-Purchase-CustomerInfo-Customer-Address-ELM">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </delivery-address>
  </xsl:template>
  <!--End of: PSMClass: "Address"-->
  <xsl:template name="TOP-Purchase-CustomerInfo-Customer-Address-ELM">
    <xsl:param name="ci" as="item()*" />
    <xsl:apply-templates select="$ci[name() = 'postcode']" />
    <xsl:apply-templates select="$ci[name() = 'city']" />
    <xsl:apply-templates select="$ci[name() = 'street']" />
  </xsl:template>
  <!--PSMClass: "CustEmail"-->
  <xsl:template name="TOP-Purchase-CustomerInfo-Customer-CustEmail">
    <emails>
      <xsl:call-template name="TOP-Purchase-CustomerInfo-Customer-CustEmail-ELM" />
    </emails>
  </xsl:template>
  <!--End of: PSMClass: "CustEmail"-->
  <xsl:template name="TOP-Purchase-CustomerInfo-Customer-CustEmail-ELM">
    <xsl:apply-templates select="email[position() le 5]" />
    <xsl:call-template name="TOP-Purchase-CustomerInfo-Customer-CustEmail-email-IG">
      <xsl:with-param name="count" select="1 - count(email)" />
    </xsl:call-template>
  </xsl:template>
  <!--PSMClass: "Product"-->
  <xsl:template match="/purchase-request/item/product">
    <product>
      <xsl:call-template name="TOP-Purchase-Items-Item-Product-ATT">
        <xsl:with-param name="ci" select="./(* | @*)" />
      </xsl:call-template>
    </product>
  </xsl:template>
  <!--End of: PSMClass: "Product"-->
  <xsl:template name="TOP-Purchase-Items-Item-Product-ATT">
    <xsl:param name="ci" as="item()*" />
    <xsl:apply-templates select="$ci[name() = 'code']" />
    <xsl:apply-templates select="$ci[name() = 'subcode']" />
    <xsl:apply-templates select="$ci[name() = 'title']" />
  </xsl:template>
  <!-- Instance generators -->
  <xsl:template name="TOP-Purchase-SalesAssistant-name-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <name>name<xsl:value-of select="current()" /></name>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-CustomerInfo-Customer-CustEmail-email-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <email>email<xsl:value-of select="current()" /></email>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Items-Item-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <item>
        <xsl:call-template name="TOP-Purchase-Items-Item-ELM-IG" />
      </item>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Items-Item-ELM-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <xsl:call-template name="TOP-Purchase-Items-Item-Product-IG" />
      <xsl:call-template name="TOP-Purchase-Items-Item-ItemI-IG" />
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Items-Item-Product-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <product>
        <xsl:call-template name="TOP-Purchase-Items-Item-Product-ATT-IG" />
      </product>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Items-Item-Product-ATT-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <xsl:attribute name="code">code</xsl:attribute>
      <xsl:attribute name="subcode">subcode</xsl:attribute>
      <xsl:attribute name="title">title</xsl:attribute>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Items-Item-ItemI-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <item-info>
        <xsl:call-template name="TOP-Purchase-Items-Item-ItemI-ELM-IG" />
      </item-info>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TOP-Purchase-Items-Item-ItemI-ELM-IG">
    <xsl:param name="count" as="item()" select="1" />
    <xsl:for-each select="1 to $count">
      <amount>amount</amount>
      <unit-price>unit-price</unit-price>
    </xsl:for-each>
  </xsl:template>
  <!--Element to attribute conversion template-->
  <xsl:template match="title" priority="0">
    <xsl:attribute name="{name()}">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>
  <!--Attribute to element conversion template-->
  <xsl:template match="@emp-no" priority="0">
    <xsl:element name="{name()}">
      <xsl:value-of select="." />
    </xsl:element>
  </xsl:template>
  <!--Blue nodes template-->
  <xsl:template match="item" priority="0">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>
  <!--Green nodes template-->
  <xsl:template match="item-info | @purchase-date | @customer-no | postcode | city | street | @code | @subcode | amount | unit-price | email" priority="0">
    <xsl:copy-of select="." />
  </xsl:template>
</xsl:stylesheet>