<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
    xmlns:oclX="oclXasdfasdf"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    >
	<xsl:output method="xml"/>
	<!-- 
        context: Product
        inv: flatten(self.supply.collect(supply | 
        supply.supplied-part.collect(sp | p.code)))
        = self.parts.part->collect(p | p.code)            
    -->
	<xsl:template match="product-assembly-report">
		<RESULT>
			<XXXX/>
			<!-- vnitrni collect mi vrati vsechny suppli
        vnejsi kollekt mi nad vnitrnim collectem vrati pro vsechny suppli jejich parts -->
			<xsl:variable name="vnitrniCollectf" as="element()*">
				<xsl:copy-of select="oclX:collect1f(supply, (), 1, count(supply))"/>
			</xsl:variable>
			<xsl:copy-of select="$vnitrniCollectf"/>
			<YYYY/>
			<xsl:variable name="vnejsiCollectf" as="element()*">
				<xsl:copy-of select="oclX:collect2f($vnitrniCollectf, (), 1, count($vnitrniCollectf))"/>
			</xsl:variable>
			<xsl:copy-of select="$vnejsiCollectf"/>
						
			<ZZZZ/>
		    <xsl:variable name="partCodes" as="element()*">
		        <xsl:copy-of select="
		            oclX:collect3(parts/part, (), 1, count(parts/part))" />
		    </xsl:variable>
		    
		    <xsl:copy-of select="$partCodes"/>
		    <WWWW/>
		    
		    Are equal? : 
		    <xsl:value-of select="if (oclX:set-equal($partCodes, $vnejsiCollectf)) then 'YES' else 'NO'" />
		    
		</RESULT>
	</xsl:template>
	
	<xsl:function name="oclX:sequence-equal-ignore-order" as="xs:boolean">
	
    <xsl:function name="oclX:set-equal" as="xs:boolean">
        <xsl:param name="set1" as="element()*" />
        <xsl:param name="set2" as="element()*" />
        
        <xsl:choose>
            <xsl:when test="count($set1) ne count($set2)">
                <xsl:value-of select="false()" />
            </xsl:when>
            <xsl:when test="count($set1) eq 0">
                <xsl:value-of select="true()" />
            </xsl:when>            
            <xsl:otherwise>
                <xsl:value-of select="oclX:set-equal-rec($set1, $set2, 1)" />
            </xsl:otherwise>
        </xsl:choose>        
    </xsl:function>
	
    <xsl:function name="oclX:set-equal-rec" as="xs:boolean">
        <xsl:param name="set1" as="element()*" />
        <xsl:param name="set2" as="element()*" />
        <xsl:param name="index" />
        <xsl:choose>
            <xsl:when test="$index eq count($set1) + 1">
                <xsl:value-of select="true()"/>
            </xsl:when>
            <xsl:when test="index-of($set1, $set2[$index])">    
                <xsl:value-of select="oclX:set-equal-rec($set1, $set2, $index +1)"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="false()"/>
            </xsl:otherwise>
        </xsl:choose>       
        
    </xsl:function>
	
    <xsl:function name="oclX:collect3" as="element()*">
        <xsl:param name="collection" as="element()*"/>
        <xsl:param name="accumulator" as="element()*"/>
        <xsl:param name="iteration"/>
        <xsl:param name="total-iterations"/>
        <xsl:choose>
            <xsl:when test="$iteration le $total-iterations">
                <xsl:variable name="nextval" as="element()*">
                    <xsl:copy-of select="$collection[$iteration]/code"/>
                </xsl:variable>
                <xsl:variable name="newAccumulator" as="element()*">
                    <xsl:copy-of select="$accumulator union $nextval"/>
                </xsl:variable>
                <xsl:copy-of select="oclX:collect3($collection, $newAccumulator, $iteration + 1, $total-iterations)"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:copy-of select="$accumulator"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
	<xsl:function name="oclX:collect2" as="element()*">
		<xsl:param name="collection" as="element()*"/>
		<xsl:param name="accumulator" as="element()*"/>
		<xsl:param name="iteration"/>
		<xsl:param name="total-iterations"/>
		<xsl:choose>
			<xsl:when test="$iteration le $total-iterations">
				<xsl:variable name="nextval" as="element()*">
					<T2>
						<xsl:copy-of select="$collection[$iteration]/supply/supplied-part/code"/>
					</T2>
				</xsl:variable>
				<xsl:variable name="newAccumulator" as="element()*">
					<xsl:copy-of select="$accumulator union $nextval"/>
				</xsl:variable>
				<xsl:copy-of select="oclX:collect2($collection, $newAccumulator, $iteration + 1, $total-iterations)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="$accumulator"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:function>
	<xsl:function name="oclX:collect1" as="element()*">
		<xsl:param name="collection" as="element()*"/>
		<xsl:param name="accumulator" as="element()*"/>
		<xsl:param name="iteration"/>
		<xsl:param name="total-iterations"/>
		<xsl:choose>
			<xsl:when test="$iteration le $total-iterations">
				<xsl:variable name="nextval" as="element()*">
					<T1>
						<xsl:copy-of select="$collection[$iteration]"/>
					</T1>
				</xsl:variable>
				<xsl:variable name="newAccumulator" as="element()*">
					<xsl:copy-of select="$accumulator union $nextval"/>
				</xsl:variable>
				<xsl:copy-of select="oclX:collect1($collection, $newAccumulator, $iteration + 1, $total-iterations)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="$accumulator"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:function>
	<xsl:function name="oclX:collect1f" as="element()*">
		<xsl:param name="collection" as="element()*"/>
		<xsl:param name="accumulator" as="element()*"/>
		<xsl:param name="iteration"/>
		<xsl:param name="total-iterations"/>
		<xsl:choose>
			<xsl:when test="$iteration le $total-iterations">
				<xsl:variable name="nextval" as="element()*">
					<xsl:copy-of select="$collection[$iteration]"/>
				</xsl:variable>
				<xsl:variable name="newAccumulator" as="element()*">
					<xsl:copy-of select="$accumulator union $nextval"/>
				</xsl:variable>
				<xsl:copy-of select="oclX:collect1f($collection, $newAccumulator, $iteration + 1, $total-iterations)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="$accumulator"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:function>
	<xsl:function name="oclX:collect2f" as="element()*">
		<xsl:param name="collection" as="element()*"/>
		<xsl:param name="accumulator" as="element()*"/>
		<xsl:param name="iteration"/>
		<xsl:param name="total-iterations"/>
		<xsl:choose>
			<xsl:when test="$iteration le $total-iterations">
				<xsl:variable name="nextval" as="element()*">
					<xsl:copy-of select="$collection[$iteration]/supplied-part/code"/>
				</xsl:variable>
				<xsl:variable name="newAccumulator" as="element()*">
					<xsl:copy-of select="$accumulator union $nextval"/>
				</xsl:variable>
				<xsl:copy-of select="oclX:collect2f($collection, $newAccumulator, $iteration + 1, $total-iterations)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="$accumulator"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:function>
</xsl:stylesheet>
