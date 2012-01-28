<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:xs="http://www.w3.org/2001/XMLSchema" 
	xmlns:fn="http://www.w3.org/2005/xpath-functions"
	xmlns:oclX="http://tempuri.org"
>

<!--
Mam zvalidovat OCL
context: Purchase
inv: self.totalprice = 
	self.items.item->sum(i ; acc = 0 | acc + i.price)

coz je ekvivalentni
	self.items.item->iterate(i )
-->
<xsl:template match="/purchase">
		<RESULT>
		<xsl:value-of select="oclX:forAll(item, 1, count(item))" />
		</RESULT>		
	</xsl:template>

	<xsl:function name="oclX:forAll">
		<xsl:param name="collection" />
		<xsl:param name="iteration" />
		<xsl:param name="total-iterations" />
		 
 <!--and oclX:forAll($collection, $iteration + 1, $total-iterations)-->
 
		<xsl:choose>
			<xsl:when test="$iteration &lt;= $total-iterations">
				<xsl:value-of select="(number($collection[$iteration]/price) ge 12) and boolean(oclX:forAll($collection, $iteration + 1, $total-iterations))" />
			</xsl:when>   
			<xsl:otherwise>
				<xsl:value-of select="true()" />
			</xsl:otherwise>
		</xsl:choose>
		<!--<xsl:value-of select="sum($collection/@price)"/>-->
	</xsl:function>


	<xsl:function name="oclX:iterate">
		<xsl:param name="collection" />
		<xsl:param name="accumulator" />	
		<xsl:param name="iteration" />
		<xsl:param name="total-iterations" />
		
		<xsl:choose>
			<xsl:when test="$iteration &lt;= $total-iterations">
				<xsl:variable name="newAccumulator">
					<xsl:value-of select="$accumulator + $collection[$iteration]/price" />
				</xsl:variable>
				<xsl:value-of select="oclX:iterate($collection, $newAccumulator, $iteration + 1, $total-iterations)" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$accumulator" />
			</xsl:otherwise>
		</xsl:choose>
		<!--<xsl:value-of select="sum($collection/@price)"/>-->
	</xsl:function>
	
</xsl:stylesheet>