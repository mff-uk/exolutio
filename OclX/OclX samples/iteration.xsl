<?xml version="1.0"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:oclX="http://tempuri.org"
>

	<xsl:template match="/items">
		<RESULT>
		<!--<xsl:value-of select="oclX:iterate(item)" />-->
		<xsl:value-of select="oclX:iterate(item, 0, 1, count(item/@price), '$acc + $collection[$iteration]/@price')" />
		</RESULT>
	</xsl:template>


	<xsl:function name="oclX:iterate">
		<xsl:param name="collection" />
		<xsl:param name="accumulator" />	
		<xsl:param name="iteration" />
		<xsl:param name="total-iterations" />
		<xsl:param name="expr" />
		
		<xsl:choose>
			<xsl:when test="$iteration &lt;= $total-iterations">
				<xsl:variable name="newAccumulator">
					<xsl:value-of select="$accumulator + $collection[$iteration]/@price" />
				</xsl:variable>
				<xsl:value-of select="oclX:iterate($collection, $newAccumulator, $iteration + 1, $total-iterations, $expr)" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$accumulator" />
			</xsl:otherwise>
		</xsl:choose>
		<!--<xsl:value-of select="sum($collection/@price)"/>-->
	</xsl:function>
</xsl:stylesheet>