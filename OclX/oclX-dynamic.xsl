<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
  xmlns:oclX="http://eXolutio.com/oclX/dynamic"
  xmlns:oclXin="http://eXolutio.com/oclX/dynamic/internal"
  xmlns:saxon="http://saxon.sf.net/"    
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:oclDate="http://eXolutio.com/oclX/types/date"
  exclude-result-prefixes="xd oclX saxon"  
  version="2.0">
  
  <xsl:import href="types/date.xsl"/>
  <xsl:import href="oclX-dynamic-internal.xsl"/>
  
  <!-- iterate function using dynamic evaluation --> 
  <xsl:function name="oclX:iterate" as="item()*" >
    <xsl:param name="collection"  as="item()*" />
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="accumulatorVar" as="xs:string"/>
    <xsl:param name="accumulatorInitExpresson" as="xs:string" />        
    <xsl:param name="expression" as="xs:string" />
    <xsl:param name="variables" as="item()*" />
    
    <xsl:variable name="accumulatorInitialValue" as="item()*">
      <xsl:sequence select="oclXin:evaluate($accumulatorInitExpresson, $variables)"></xsl:sequence>
    </xsl:variable>            
    
    <xsl:sequence select="oclX:iterate-rec($collection, $iterationVar, $accumulatorVar,
      $accumulatorInitialValue, $expression, 1, count($collection), $variables)" />   
  </xsl:function>
  
  <xsl:function name="oclX:iterate-rec" as="item()*" >
    <xsl:param name="collection"  as="item()*" />
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="accumulatorVar" as="xs:string"/>
    <xsl:param name="accumulator" as="item()*" />	        
    <xsl:param name="expression" as="xs:string" />
    <xsl:param name="iteration"  as="xs:integer" />
    <xsl:param name="totalIterations" as="xs:integer"/>
    <xsl:param name="variables" as="item()*" />
    
    <xsl:choose>            
      <xsl:when test="$iteration = $totalIterations + 1">
        <!-- return accumulated value -->
        <xsl:sequence select="$accumulator"/>
      </xsl:when>            
      <xsl:otherwise>
        <xsl:variable name="variablesWithIteration" as="item()*">
          <xsl:sequence select="oclXin:appendVarToSequence($variables, $iterationVar, $collection[$iteration])" />
        </xsl:variable>
        <xsl:variable name="variablesWithAccumulator" as="item()*">
          <xsl:sequence select="oclXin:appendVarToSequence($variablesWithIteration, $accumulatorVar, $accumulator)" />
        </xsl:variable>
        <!-- compute new accumulator value -->        
        <xsl:variable name="newAccumulator" as="item()*"> 
          <xsl:sequence select="oclXin:evaluate($expression, $variablesWithAccumulator)" />                     
        </xsl:variable>
        <!-- call recursively --> 
        <xsl:sequence select="oclX:iterate-rec($collection, $iterationVar, $accumulatorVar, $newAccumulator, $expression, $iteration +1, $totalIterations, $variables)"/>                                
      </xsl:otherwise>            
    </xsl:choose>   
    
  </xsl:function>
  
  <xsl:function name="oclX:holds" as="xs:boolean">
    <xsl:param name="expression"  as="xs:string" />
    <xsl:param name="variables" as="item()*" />
    
    <xsl:sequence select="oclXin:evaluate($expression, $variables)" />
  </xsl:function>
  
    <!-- forAll function using dynamic evaluation --> 
  <xsl:function name="oclX:forAll" as="xs:boolean">
    <xsl:param name="collection"  as="item()*" />
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="expression"  as="xs:string" />
    <xsl:param name="variables" as="item()*" />
  
    <xsl:sequence select="oclX:forAll-rec($collection, $iterationVar,
      $expression, 1, count($collection), $variables)" />        
  </xsl:function>  
  
  <xsl:function name="oclX:forAll-rec" as="xs:boolean">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="expression" as="xs:string" />
    <xsl:param name="iteration" as="xs:integer" />
    <xsl:param name="totalIterations" as="xs:integer" />
    <xsl:param name="variables" as="item()*" />
    
    <xsl:choose>            
      <xsl:when test="$iteration = count($collection) + 1">
        <xsl:sequence select="true()"/>
      </xsl:when>            
      <xsl:otherwise>  
        <xsl:variable name="variablesForThisIteration" as="item()*" 
          select="oclXin:appendVarToSequence($variables, $iterationVar, $collection[$iteration])" />         
        <xsl:variable name="forThis" as="xs:boolean">          
          <xsl:sequence select="oclXin:evaluate($expression, $variablesForThisIteration)" />
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="$forThis">
            <xsl:sequence select="oclX:forAll-rec($collection, $iterationVar, $expression, $iteration +1, $totalIterations, $variables)"/>        
          </xsl:when>
          <xsl:otherwise>
            <xsl:sequence select="false()"/>                        
          </xsl:otherwise>
        </xsl:choose>                
      </xsl:otherwise>            
    </xsl:choose>       
  </xsl:function>
      
  <!-- collect function using iterate -->
  <xsl:function name="oclX:collect" as="item()*">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="iterationVar" as="xs:string" />
    <xsl:param name="expression" as="xs:string" />
    <xsl:param name="variables" as="item()*" />
    
    <xsl:variable name="result" as="item()*">
      <xsl:sequence select="oclX:iterate($collection, $iterationVar, 'acc',  '()', concat('$acc , ', $expression), $variables)" />
    </xsl:variable>
    <xsl:sequence select="$result" />
  </xsl:function>  
  
  <!-- collect function using iterate -->
  <xsl:function name="oclX:exists" as="xs:boolean">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="expression" as="xs:string" />
    <xsl:param name="variables" as="item()*" />
 
    <xsl:variable name="result" as="item()*">
      <xsl:sequence select="oclX:iterate($collection, $iterationVar, 'acc',  'false()', concat('$acc or (', $expression, ')'), $variables)" />
    </xsl:variable>
    <xsl:sequence select="$result" />
  </xsl:function> 
  
  <xsl:function name="oclXin:evaluate" as="item()*">
    <xsl:param name="expression" as="xs:string"/>    
    <xsl:param name="variables" as="item()*" />
    <xsl:variable name="expressionReplaced" select="oclXin:expressionForSaxon($expression, $variables)" as="xs:string" />
    <xsl:variable name="result" as="item()*">
      <xsl:choose>
        <xsl:when test="count($variables) div 2 = 0">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables)"></xsl:sequence>        
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 1">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables)" />        
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 2">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]))" />        
         </xsl:when>
        <xsl:when test="count($variables) div 2 = 3">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]), oclXin:retrieveVarFromSequence($variables[6]))" />        
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 4">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]), oclXin:retrieveVarFromSequence($variables[6]), oclXin:retrieveVarFromSequence($variables[8]))" />        
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 5">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]), oclXin:retrieveVarFromSequence($variables[6]), oclXin:retrieveVarFromSequence($variables[8]), oclXin:retrieveVarFromSequence($variables[10]))" />        
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 6">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]), oclXin:retrieveVarFromSequence($variables[6]), oclXin:retrieveVarFromSequence($variables[8]), oclXin:retrieveVarFromSequence($variables[10]), oclXin:retrieveVarFromSequence($variables[12]))" />        
        </xsl:when>        
        <xsl:otherwise>
          <xsl:message>ERROR IN EVALUATE</xsl:message>
          ERROR IN EVALUATE
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:sequence select="$result" />
  </xsl:function>
  
  <xsl:function name="oclX:vars" as="item()*">
    <xsl:param name="current-node" as="item()"/>
    <variables/>
    <!-- just a placeholder -->
    <dummy/>      
    <!-- name of the $self variable -->
    <self/>      
    <!-- value of the $self variable-->
    <xsl:sequence select="$current-node"/>  
  </xsl:function>
  
  <!-- sum function implemented using dynamic iterate--> 
  <!--<xsl:function name="oclX:sum">
    <xsl:param name="collection" />
    <xsl:param name="variables" as="item()*" />
    
    <xsl:copy-of select="oclX:iterate($collection, '0', '$p2 + $p3', $variables)"/>
    </xsl:function>-->
</xsl:stylesheet>