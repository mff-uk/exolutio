<?xml version="1.0" encoding="UTF-8"?>
 <xsl:stylesheet version="2.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:oclX="oclXasdfasdf"
    > 
    <xsl:output method="xml" />
    
    <!-- 
        context: Product
        inv: flatten(self.supply.collect(supply | 
        supply.supplied-part.collect(sp | p.code)))
        = self.parts.part->collect(p | p.code)            
    -->
    
    <xsl:template match="product-assembly-report">
		  <RESULT>
		    
        XXXX
		  
        <xsl:variable name="vnitrniCollect" as="element()*">
            <xsl:copy-of select="oclX:collect1(supply, (), 1, count(supply))" />
        </xsl:variable>
        
        <xsl:copy-of select="$vnitrniCollect"></xsl:copy-of>
        
        YYY
        
        <xsl:variable name="vnejsiCollect" as="element()*">
                <xsl:copy-of select="oclX:collect2($vnitrniCollect, (), 1, count($vnitrniCollect))" />    
        </xsl:variable>
            
        <xsl:copy-of select="$vnejsiCollect"></xsl:copy-of>
        
        ZZZZZ
        WWWWW
		  
        <!-- vnitrni collect mi vrati vsechny suppli
        vnejsi kollekt mi nad vnitrnim collectem vrati pro vsechny suppli jejich parts -->
        <xsl:variable name="vnitrniCollectf" as="element()*">
            <xsl:copy-of select="oclX:collect1f(supply, (), 1, count(supply))" />
        </xsl:variable>
        
        <xsl:copy-of select="$vnitrniCollectf"></xsl:copy-of>
        
        FFFFF

    		<xsl:variable name="vnitrniCollectff" as="element()*" >
    			<xsl:copy-of select="for $i in $vnitrniCollect return $i/supply"/>
    		</xsl:variable>
    		
            
        <xsl:variable name="vnejsiCollectf" as="element()*">
          <xsl:copy-of select="oclX:collect2f($vnitrniCollectf, (), 1, count($vnitrniCollectf))" />    
        </xsl:variable>
                
    		<xsl:variable name="vnejsiCollectff" as="element()*">
          <xsl:copy-of select="oclX:collect2f($vnitrniCollectff, (), 1, count($vnitrniCollectff))" />    
        </xsl:variable>
                
        <xsl:copy-of select="$vnejsiCollectf"></xsl:copy-of>
        <xsl:copy-of select="$vnejsiCollectff"></xsl:copy-of>        
    
        HHHHH
     
		  </RESULT>
      
      Test
      <xsl:value-of select="(1,2,(12, 13))" />
      <xsl:variable name="v" as="item()*" select="//part">
        
      </xsl:variable>
      
      <xsl:copy-of select="$v" />
    </xsl:template>

    <!-- verze bez f na konci oddeluji prvky nacollectovane sekvence pomoci T2 -->
    <xsl:function name="oclX:collect2"  as="element()*">
        <xsl:param name="collection" as="element()*" />
        <xsl:param name="accumulator" as="element()*"/>
        <xsl:param name="iteration"/>
        <xsl:param name="total-iterations"/>
        
        <xsl:choose>
            <xsl:when test="$iteration le $total-iterations">
                <xsl:variable name="nextval" as="element()*">
                    <T2><xsl:copy-of select="$collection[$iteration]/supply/supplied-part/code"/></T2>
                </xsl:variable>
                <xsl:variable name="newAccumulator" as="element()*">
                    <xsl:copy-of select="$accumulator union $nextval"/>
                </xsl:variable>
                <xsl:copy-of select="oclX:collect2($collection, $newAccumulator, $iteration + 1, $total-iterations)" />
            </xsl:when>
            <xsl:otherwise>
                <xsl:copy-of select="$accumulator"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <xsl:function name="oclX:collect1"  as="element()*">
        <xsl:param name="collection" as="element()*" />
        <xsl:param name="accumulator" as="element()*"/>
        <xsl:param name="iteration"/>
        <xsl:param name="total-iterations"/>
        
        <xsl:choose>
            <xsl:when test="$iteration le $total-iterations">
                <xsl:variable name="nextval" as="element()*">
                    <T1><xsl:copy-of select="$collection[$iteration]"/></T1>
                </xsl:variable>
                <xsl:variable name="newAccumulator" as="element()*">
                      <xsl:copy-of select="$accumulator union $nextval"/>
                </xsl:variable>              
                <xsl:copy-of select="oclX:collect1($collection, $newAccumulator, $iteration + 1, $total-iterations)" />                
            </xsl:when>
            <xsl:otherwise>
                <xsl:copy-of select="$accumulator"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <!-- f jako flat, je to vlastne prima implementace collect --> 
  	<xsl:function name="oclX:collect1f"  as="element()*">
  		 <xsl:param name="collection" as="element()*" />
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
               <xsl:copy-of select="oclX:collect1f($collection, $newAccumulator, $iteration + 1, $total-iterations)" />                
           </xsl:when>
           <xsl:otherwise>
               <xsl:copy-of select="$accumulator"/>
           </xsl:otherwise>
       </xsl:choose>
     </xsl:function>
    
    <xsl:function name="oclX:collect2f"  as="element()*">
       <xsl:param name="collection" as="element()*" />
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
               <xsl:copy-of select="oclX:collect2f($collection, $newAccumulator, $iteration + 1, $total-iterations)" />
           </xsl:when>
           <xsl:otherwise>
               <xsl:copy-of select="$accumulator"/>
           </xsl:otherwise>
       </xsl:choose>
    </xsl:function>
       
	
</xsl:stylesheet>