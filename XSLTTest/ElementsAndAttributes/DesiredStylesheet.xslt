<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
    <xsl:output method="xml" indent="yes"/>
    <!-- 
        wrapovaci template obyc. classy purchase request
        ma match, protoze classa je existing
        uvnitr ma wrapovaci node 
        a vola BODY templatu, ktera je named (aby sla volat ze SR) a predava ji svuj obsah
        
        *** PRAVIDLO: MATCH template nikdy nema CI, pro select do body pouziva /* ***
    -->
    <xsl:template match="/purchase-request">
        <purchase-request>
            <xsl:call-template name="TOP-Purchase-ATT">
                <xsl:with-param name="ci" select="./(* | @*)"/>
            </xsl:call-template>
            <xsl:call-template name="TOP-Purchase-ELM">
                <xsl:with-param name="ci" select="./(* | @*)"/>
            </xsl:call-template>
        </purchase-request>
    </xsl:template>
    <xsl:template name="TOP-Purchase-ATT">
        <xsl:param name="ci" as="item()*"/>
        <xsl:apply-templates select="$ci[name() = 'purchase-date']"/>
    </xsl:template>
    <!-- 
        body template classy purchase
        pracuje s obsahem 
    -->
    <xsl:template name="TOP-Purchase-ELM">
        <xsl:param name="ci" as="item()*"/>
        <!-- existing classy -->
        <xsl:apply-templates select="$ci[name() = 'customer-info']"/>
        <xsl:apply-templates select="$ci[name() = 'assistant']"/>
        <!-- added sequence CM -->
        <xsl:call-template name="TOP-Purchase-SEQitems"/>
    </xsl:template>

    <xsl:template match="/purchase-request/customer-info">
        <customer-info>
            <xsl:call-template name="TOP-Purchase-SEQcustomer-info-ELM">
                <xsl:with-param name="ci" select="./(* | @*)"/>
            </xsl:call-template>
        </customer-info>
    </xsl:template>
    <xsl:template name="TOP-Purchase-SEQcustomer-info-ELM">
        <xsl:param name="ci" as="item()*"/>
        <xsl:apply-templates select="$ci[name() = 'customer']"/>
    </xsl:template>

    <xsl:template match="/purchase-request/assistant">
        <assistant>
            <xsl:call-template name="TOP-Purchase-SalesAssistant-ELM">
                <xsl:with-param name="ci" select="./(* | @*)"/>
            </xsl:call-template>
        </assistant>
    </xsl:template>
    <xsl:template name="TOP-Purchase-SalesAssistant-ELM">
        <xsl:param name="ci" as="item()*"/>
        <!-- added node (attribute) -->
        <xsl:apply-templates select="$ci[name() = 'emp-no']"/>
        <xsl:call-template name="TOP-Purchase-SalesAssistant-name"/>
    </xsl:template>

    <!-- 
        added sequence CM top template, ma wrapovaci node 
        ?? bude mit ci? 
			 ne, u added zadne ci nebude, nikdy
    -->
    <xsl:template name="TOP-Purchase-SEQitems">
        <!-- wrapovaci node -->
        <items>
            <xsl:call-template name="TOP-Purchase-SEQitems-ELM"/>
        </items>
    </xsl:template>
    <!-- zpracuje itemy -->
    <xsl:template name="TOP-Purchase-SEQitems-ELM">
        <xsl:apply-templates select="item"/>
    </xsl:template>

    <xsl:template match="/purchase-request/customer-info/customer">
        <customer>
            <xsl:call-template name="TOP-Purchase-SEQcustomer-info-Customer-ATT">
                <xsl:with-param name="ci" select="./(* | @*)"/>
            </xsl:call-template>
            <xsl:call-template name="TOP-Purchase-SEQcustomer-info-Customer-ELM">
                <xsl:with-param name="ci" select="./(* | @*)"/>
            </xsl:call-template>
        </customer>
    </xsl:template>
    <xsl:template name="TOP-Purchase-SEQcustomer-info-Customer-ATT">
        <xsl:param name="ci" as="item()*"/>
        <xsl:apply-templates select="$ci[name() = 'customer-no']"/>
    </xsl:template>
    <xsl:template name="TOP-Purchase-SEQcustomer-info-Customer-ELM">
        <xsl:param name="ci" as="item()*"/>
        <xsl:apply-templates select="../address"/>
        <xsl:call-template name="TOP-Purchase-SEQcustomer-info-Customer-CustEmail"/>
    </xsl:template>
    <!-- template of the created attribute -->
    <xsl:template name="TOP-Purchase-SalesAssistant-name">
        <name>###</name>
    </xsl:template>

    <xsl:template match="/purchase-request/customer-info/address">
        <delivery-address>
            <xsl:call-template name="TOP-Purchase-SEQcustomer-info-Customer-Address-ELM">
                <xsl:with-param name="ci" select="./(* | @*)"/>
            </xsl:call-template>
        </delivery-address>
    </xsl:template>
    <xsl:template name="TOP-Purchase-SEQcustomer-info-Customer-Address-ELM">
        <xsl:param name="ci" as="item()*"/>
        <xsl:apply-templates select="$ci[name() = 'postcode']"/>
        <xsl:apply-templates select="$ci[name() = 'city']"/>
        <xsl:apply-templates select="$ci[name() = 'street']"/>
    </xsl:template>

    <xsl:template name="TOP-Purchase-SEQcustomer-info-Customer-CustEmail">
        <emails>
            <xsl:call-template name="TOP-Purchase-SEQcustomer-info-Customer-CustEmail-ELM"/>
        </emails>
    </xsl:template>
    <xsl:template name="TOP-Purchase-SEQcustomer-info-Customer-CustEmail-ELM">
        <!-- body of added class does not have a CI parameter -->
        <xsl:apply-templates select="email"/>
    </xsl:template>

    <xsl:template match="/purchase-request/item/product">
        <product>
            <xsl:call-template name="TOP-Purchase-SEQitems-Item-Product-ATT">
                <xsl:with-param name="ci" select="./(* | @*)"/>
            </xsl:call-template>
        </product>
    </xsl:template>
    <xsl:template name="TOP-Purchase-SEQitems-Item-Product-ATT">
        <xsl:param name="ci" as="item()*"/>
        <xsl:apply-templates select="$ci[name() = 'code']"/>
        <xsl:apply-templates select="$ci[name() = 'subcode']"/>
        <xsl:apply-templates select="$ci[name() = 'title']"/>
    </xsl:template>
    <xsl:template match="title" priority="0">
        <xsl:attribute name="{name()}">
            <xsl:value-of select="."/>
        </xsl:attribute>
    </xsl:template>
    <!--Attribute to element conversion template-->
    <xsl:template match="@emp-no" priority="0">
        <xsl:element name="{name()}">
            <xsl:value-of select="."/>
        </xsl:element>
    </xsl:template>
    <!-- blue nodes -->
    <!-- item -->
    <xsl:template match="item" priority="0">
        <xsl:copy>
            <xsl:copy-of select="@*"/>
            <xsl:apply-templates select="*"/>
        </xsl:copy>
    </xsl:template>

    <!-- green nodes -->
    <xsl:template match="item-info | @purchase-date | @customer-no | postcode | city | street | @code | @subcode | amount | unit-price | email" priority="0">
        <xsl:copy-of select="."/>
    </xsl:template>
</xsl:stylesheet>
