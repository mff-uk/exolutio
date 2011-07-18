<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
    <xsl:output method="xml" indent="yes"/>
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
    <xsl:template name="TOP-Purchase-ELM">
        <xsl:param name="ci" as="item()*"/>
        <xsl:apply-templates select="$ci[name() = 'id']" />
        <xsl:for-each-group select="amount" group-starting-with="amount">
        <xsl:call-template name="TOP-Purchase-Item">
            <xsl:with-param name="ci" select="current-group()"/>
        </xsl:call-template>
        </xsl:for-each-group>
    </xsl:template>
    <!-- again this is important, group nodes 
         are callable, 
         but they have ci parameter 
         (because they are *existing*)    
    -->
    <xsl:template name="TOP-Purchase-Item">
        <xsl:param name="ci" as="item()*"/>
        <item>
            <xsl:call-template name="TOP-Purchase-Item-ATT">
                <!-- this is important! use the ci 
                     since it contains current group -->
                <xsl:with-param name="ci" select="$ci"/>
            </xsl:call-template>
        </item>
    </xsl:template>
    <xsl:template name="TOP-Purchase-Item-ATT">
        <xsl:param name="ci" as="item()*"/>        
        <xsl:apply-templates select="$ci[name() = 'amount']" />
        <xsl:call-template name="TOP-Purchase-Item-unit-price"/> 
    </xsl:template>
    <xsl:template name="TOP-Purchase-Item-unit-price">
        <xsl:attribute name="unit-price">###</xsl:attribute>
    </xsl:template>
    
    <!-- Element to attribute conversion template -->
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
