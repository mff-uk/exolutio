<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl" 
  xmlns:oclX="http://eXolutio.com/oclX/functional"
  xmlns:oclXin="http://eXolutio.com/oclX/functional/internal" 
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:oclDate="http://eXolutio.com/oclX/types/date"
  xmlns:oclString="http://eXolutio.com/oclX/types/string"
  xmlns:doc="http://www.oxygenxml.com/ns/doc/xsl" 
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" 
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  exclude-result-prefixes="xd oclX saxon doc map math"
  version="3.0">
  
  <xsl:include href="types/date.xsl"/>
  <xsl:include href="oclX-standard-library.xsl"/>
  
  <xd:doc scope="stylesheet">
    <xd:desc>
      <xd:p><xd:b>Created on:</xd:b> Jan 1, 2012</xd:p>
      <xd:p><xd:b>Author:</xd:b> Jakub Maly</xd:p>
      <xd:p><xd:b>email:</xd:b> jakub@maly.cz</xd:p>
      
      <xd:p>
        <xd:b>OclX function library</xd:b> contains definition of functions, which enable the use of
        OCL iterator expressions in XPath expressions. The library can be also used in Schematron
        schemas, providing that the Schematron validation process is based on XSLT and uses modified
        Schematron pipeline XSLTs, which add imports of OclX library. </xd:p>
      <xd:p> Look to http://exolutio.com and download eXolutio - a tool which allows translation
        from OCL scripts to Schematron schemas. You can also find the modified Schematron pipeline
        XSLTs on this address. </xd:p>
      <xd:p> The library uses higher-order functions for evaluation of iterator expressions. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:param name="debug" as="xs:boolean" select="false()" />
  
  <!-- 
    Iterator Expressions 
  --> 
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <xd:i>forAll</xd:i> function from OCL. Checks, whether a condition holds for
        all member of a collection.</doc:p>
      <doc:p>The function allows multiple iterators in <xd:i>body</xd:i>.</doc:p>
      <doc:p>Results in <xd:i>true</xd:i> if the body expression evaluates to true for each element in the source <xd:i>collection</xd:i>; otherwise, result is <xd:i>false</xd:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
      <xd:i>collection</xd:i>. All parameters and result must be of type <xd:i>boolean</xd:i>.</doc:param>
    <doc:return>Returns true, if all members of <xd:i>collection</xd:i> satisfy the condition
      defined by <xd:i>expression</xd:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:forAllN" as="xs:boolean">    
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(*)"/>
    
    <xsl:variable name="iteratorCount" select="function-arity($body)" as="xs:integer" />
    
    <xsl:iterate select="1 to xs:integer(math:pow(count($collection), $iteratorCount))">
      <!-- expect true in the beginning --> 
      <xsl:param name="satisfied" as="xs:boolean" select="true()" />
      
      <xsl:variable name="indices" select="
        oclXin:getIndices(. - 1, count($collection), $iteratorCount, ())"/>
      
      <xsl:variable name="forThis" select="oclXin:functionItemCall(
        $body, $iteratorCount, $indices, $collection)" as="xs:boolean" />
      
      <xsl:choose>
        <xsl:when test="$forThis eq false()">
          <xsl:break>
            <xsl:sequence select="false()"/>
          </xsl:break>
        </xsl:when>
        <xsl:otherwise>
          <xsl:next-iteration>
            <xsl:with-param name="satisfied" select="$satisfied and $forThis" />  
          </xsl:next-iteration>    
        </xsl:otherwise>  
      </xsl:choose>
      
      <xsl:on-completion>
        <xsl:sequence select="$satisfied" />
      </xsl:on-completion>
    </xsl:iterate>    
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <xd:i>forAll</xd:i> function from OCL. The implementation uses
        <xd:i>every/satisfies</xd:i> expression internally. Checks, whether a condition holds for
        all member of a collection.</doc:p>
      <doc:p>Results in <xd:i>true</xd:i> if the body expression evaluates to true for each element 
        in the source <xd:i>collection</xd:i>; otherwise, result is <xd:i>false</xd:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
      <xd:i>collection</xd:i>. </doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>Returns true, if all members of <xd:i>collection</xd:i> satisfy the condition
      defined by <xd:i>expression</xd:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:forAll" as="xs:boolean">    
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean"/>    
    <xsl:sequence select="every $it in $collection satisfies $body($it) "/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <xd:i>exists</xd:i> function from OCL. The implementation uses
        <xd:i>some/satisfies</xd:i> expression internally. Checks, whether a condition holds for some member of
        a collection.</doc:p>
      <doc:p>Results in true if body evaluates to true for at least one element in the source collection.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>    
    <doc:param name="body">Condition expression evaluated for each member of
      <xd:i>collection</xd:i>. </doc:param>
    <doc:return>Returns true, if some member of <xd:i>collection</xd:i> satisfies the condition
      defined by <xd:i>expression</xd:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:exists" as="xs:boolean">    
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean"/>    
    <!--<xsl:sequence select="some $it in $collection satisfies $body($it) "/>-->    
    
    <xsl:sequence select="oclX:iterate($collection, false(), 
      function($it, $acc) { $acc or ($body($it)) })" /> 
      
  </xsl:function>  

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <xd:i>exists</xd:i> function from OCL. Checks, whether a condition holds for some member of
        a collection.</doc:p>      
      <doc:p>The function allows multiple iterators in <xd:i>body</xd:i>.</doc:p>
      <doc:p>Results in true if body evaluates to true for at least one element in the source collection.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>    
    <doc:param name="body">Condition expression evaluated for each member of
      <xd:i>collection</xd:i>. All parameters and result must be of type <xd:i>boolean</xd:i>. </doc:param>
    <doc:return>Returns true, if some member of <xd:i>collection</xd:i> satisfies the condition
      defined by <xd:i>expression</xd:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:existsN" as="xs:boolean">        
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(*)"/>
    
    <xsl:variable name="iteratorCount" select="function-arity($body)" as="xs:integer" />
    
    <xsl:iterate select="1 to xs:integer(math:pow(count($collection), $iteratorCount))">
      <!-- expect true in the beginning --> 
      <xsl:param name="satisfied" as="xs:boolean" select="true()" />
      
      <xsl:message>Iteration: <xsl:value-of select="."/></xsl:message>
      <xsl:variable name="indices" select="
        oclXin:getIndices(. - 1, count($collection), $iteratorCount, ())"/>
      <xsl:message>Indices: <xsl:value-of select="$indices"/></xsl:message>
      
      <xsl:variable name="forThis" select="oclXin:functionItemCall(
        $body, $iteratorCount, $indices, $collection)" as="xs:boolean" />
      
      <xsl:choose>
        <xsl:when test="$forThis eq true()">
          <xsl:break>
            <xsl:sequence select="true()"/>
          </xsl:break>
        </xsl:when>
        <xsl:otherwise>
          <xsl:next-iteration>
            <xsl:with-param name="satisfied" select="$satisfied or $forThis" />  
          </xsl:next-iteration>    
        </xsl:otherwise>  
      </xsl:choose>
      
      <xsl:on-completion>
        <xsl:sequence select="$satisfied" />
      </xsl:on-completion>
    </xsl:iterate>    
  </xsl:function>  
  
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements the fundamental OCL iterator expression <xd:i>iterate</xd:i>. </doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="accInit">The initial value of the accumulator.</doc:param>
    <doc:param name="body">The expression evaluated for each member of <xd:i>collection</xd:i>.</doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:iterate" as="item()*">        
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="accInit" as="item()*"/>
    <xsl:param name="body" as="function(item(), item()*) as item()*"/>
           
    <xsl:iterate select="1 to count($collection)">      
      <xsl:param name="acc" select="$accInit" as="item()*" />
      <xsl:if test="$debug">
        <xsl:message>Iteration: <xsl:value-of select="."/>/<xsl:value-of select="count($collection)"/></xsl:message>
        <xsl:message>Current: <xsl:value-of select="$collection[current()]"/>"/></xsl:message>  
      </xsl:if>
      <xsl:next-iteration>
        <xsl:with-param name="acc" select="$body($collection[current()], $acc)" />
      </xsl:next-iteration>      
      <xsl:on-completion>
        <xsl:sequence select="$acc" />
      </xsl:on-completion>
    </xsl:iterate>    
  </xsl:function>  
  
  
  <doc:doc>
    <doc:desc>
      <doc:p>The collection of elements that results from 
        applying body to every member of the source set.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">The expression evaluated for each member of
      <xd:i>collection</xd:i>.</doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:collect" as="item()*">   
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()*"/>
    <xsl:sequence select="oclX:iterate($collection, (),
      function($it, $acc) { $body($it), $acc } 
      )" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>The closure of applying body transitively to every distinct element of the source
        collection.</doc:p>
      <doc:p>Computes transitive closure over a collection using an expression. </doc:p>
    </doc:desc>
    <doc:param name="collection">The initial collection</doc:param>
    <doc:param name="body">The expression is evaluated for each member of the initial
      collection and for all other members that are added be the previous calls. It represents one
      step of computation of transitive closure.</doc:param>
    <doc:return>The sequence of items collected during the consecutive evaluations of
      <doc:i>expression</doc:i> over the initial collection and over the items visited by the
      computation during computing transitive closure.</doc:return>
  </doc:doc>
  <xsl:function name="oclX:closure" as="item()*">
    <xsl:param name="collection" as="item()*"/>    
    <xsl:param name="body" as="function(item()) as item()*"/>

    <xsl:sequence
      select="oclXin:closure-rec(reverse($collection), (), $body)"
    />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Helper function used to cumpute transitive closure using recursion. </doc:p>
    </doc:desc>
    <doc:param name="toDoStack">Items discovered by transitive closure, but not yet processed. </doc:param>
    <doc:param name="result">Transitive closure computed so far. </doc:param>
    <doc:param name="body">The expression is evaluated for each member of the initial
      collection and for all other members that are added be the previous calls. It represents one
      step of computation of transitive closure.</doc:param>
    <doc:return>Transitive closure</doc:return>
  </doc:doc>
  <xsl:function name="oclXin:closure-rec" as="item()*">
    <xsl:param name="toDoStack" as="item()*"/>
    <xsl:param name="result" as="item()*"/>    
    <xsl:param name="body" as="function(item()) as item()*"/>
    
    <xsl:choose>
      <xsl:when test="count($toDoStack) eq 0">
        <xsl:sequence select="$result"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="i" select="$toDoStack[last()]" as="item()"/>        
        <xsl:variable name="contribution" select="$body($i)" as="item()*"/>
        <xsl:sequence
          select="oclXin:closure-rec(
          ($toDoStack[position() lt last()], reverse($contribution)), 
          ($result, $i), $body) "
        />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <xd:i>isUnique</xd:i> function from OCL. The implementation uses
        <xd:i>oclX:iterate</xd:i> internally. </doc:p>
      <doc:p>Results in true if body evaluates to a different value for each element in the source collection; otherwise, result is false.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>    
    <doc:param name="body">Mapping expression evaluated for each member of <xd:i>collection</xd:i>.</doc:param>
    <doc:return>Returns <xd:i>true</xd:i> when elements are distinct in the collection,
        <xd:i>false</xd:i> otherwise. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:isUnique" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()"/>
    
    <xsl:variable name="uniqueItems" as="item()*">      
      <xsl:sequence
        select="oclX:iterate(
        $collection, (), function($it, $acc) { 
        $acc, if (oclXin:combined-contains($acc, $body($it))) then $body($it) else ()
        })" />
    </xsl:variable>
    <xsl:sequence select="if (count($uniqueItems) eq count($collection)) then true() else false()"/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>The subsequence of the source <xd:i>collection</xd:i> for which body is
          <xd:i>true</xd:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">The expression evaluated for each member of
      <xd:i>collection</xd:i>.</doc:param>
    <doc:return>The subsequence of the source <xd:i>collection</xd:i> for which body is
        <xd:i>true</xd:i>.</doc:return>
  </doc:doc>
  <xsl:function name="oclX:select" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()"/>
    
    <xsl:sequence select="oclX:iterate($collection, (), 
      function($it, $acc) { if ($body($it)) then oclX:including($acc, $it) else $acc })"/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>The subsequence of the source <xd:i>collection</xd:i> for which <xd:i>body</xd:i> is
        false.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">The expression evaluated for each member of
      <xd:i>collection</xd:i></doc:param>
    <doc:return>The subsequence of the source <xd:i>collection</xd:i> for which <xd:i>body</xd:i> is
      false.</doc:return>
  </doc:doc>
  <xsl:function name="oclX:reject" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()"/>
    
    <xsl:sequence select="oclX:iterate($collection, (), 
      function($it, $acc) { if (not($body($it))) then oclX:including($acc, $it) else $acc })"/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <xd:i>any</xd:i> function from OCL. The implementation uses
        <xd:i>oclX:iterate</xd:i> internally. </doc:p>
      <doc:p>Returns any element in the source collection for which <xd:i>body</xd:i> evaluates to <xd:i>true</xd:i>. 
        If there is more than one element for which
        <xd:i>body</xd:i> is <xd:i>true</xd:i>, one of them is returned. 
        There must be at least one element fulfilling <xd:i>body</xd:i>, otherwise the result is an empty sequence.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>    
    <doc:param name="body">Condition evaluated for each member of <xd:i>collection</xd:i>.</doc:param>
    <doc:return>A member of <xd:i>collection</xd:i> satisfying <xd:i>body</xd:i>, if such an element exists, an
      empty sequence otherwise. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:any" as="item()">
    <xsl:param name="collection" as="item()*"/>    
    <xsl:param name="body" as="function(item()) as item()"/>
    
    <xsl:variable name="satisfyingItems" as="item()*">
      <xsl:sequence
        select="oclX:iterate(
        $collection, (),            
        function($it, $acc) {
        $acc, if ($body($it) eq true()) then $it else ()
        }
        )"
      />
    </xsl:variable>
    <xsl:sequence select="if (count($satisfyingItems) ge 1) then $satisfyingItems[1] else ()  "/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <xd:i>one</xd:i> function from OCL. The implementation uses
        <xd:i>oclX:iterate</xd:i> internally. </doc:p>
      <doc:p>Results in <xd:i>true</xd:i> if there is exactly one element in the source 
        <xd:i>collection</xd:i> for which <xd:i>body</xd:i> is <xd:i>true</xd:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition evaluated for each member of <xd:i>collection</xd:i>.</doc:param>        
  </doc:doc>
  <xsl:function name="oclX:one" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>    
    <xsl:param name="body" as="function(item()) as item()"/>
    
    <xsl:variable name="satisfyingItems" as="item()*">
      <xsl:sequence
        select="oclX:iterate(
        $collection, (),            
        function($it, $acc) {
        $acc, if ($body($it) eq true()) then $it else ()
        }
        )"
      />
    </xsl:variable>
    <xsl:sequence select="if (count($satisfyingItems) eq 1) then true() else false()  "/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Results in the collection containing all elements of the source
        <xd:i>collection</xd:i>. The element for which <doc:i>body</doc:i> has the lowest value
        comes first, and so on. Uses <xd:i>xsl:perform-sort</xd:i> internally. </doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition evaluated for each member of
      <xd:i>collection</xd:i>.</doc:param>
    <doc:return>Ordered sequence. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:sortedBy" as="item()*">   
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()*"/>
    
    <xsl:perform-sort select="$collection" >
      <xsl:sort select="$body(.)" />
    </xsl:perform-sort> 
    
  </xsl:function>
  
  <!-- 
    collection 
  -->
  
  <doc:doc>
    <doc:desc>
      <doc:p>Returns <xd:i>true</xd:i> if <xd:i>item</xd:i> is an element of
        <xd:i>collection</xd:i>, <xd:i>false</xd:i> otherwise.</doc:p>
    </doc:desc>
    <doc:param name="collection">The examined collection.</doc:param>
    <doc:param name="item">Searched item.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:includes" as="xs:boolean">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="item" as="item()"/>
    
    <xsl:sequence select="oclX:exists($collection, function ($it) { oclXin:combined-contains($item, $it)}) "/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Returns <xd:i>true</xd:i> if every item in <doc:i>collection2</doc:i> is an element of
          <xd:i>collection</xd:i>, <xd:i>false</xd:i> otherwise. </doc:p>
    </doc:desc>
    <doc:param name="collection">Examined collection. </doc:param>
    <doc:param name="collection2">Searched items. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:includesAll" as="xs:boolean">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="collection2" as="item()*"/>
    
    <xsl:sequence select="oclX:forAll($collection2, function($it) { oclX:includes($collection, $it) }) "/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Returns <xd:i>false</xd:i> if <xd:i>item</xd:i> is an element of
          <xd:i>collection</xd:i>, <xd:i>true</xd:i> otherwise.</doc:p>
    </doc:desc>
    <doc:param name="collection">The examined collection.</doc:param>
    <doc:param name="item">Searched item.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:excludes" as="xs:boolean">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="item" as="item()"/>
    
    <xsl:sequence select="oclX:forAll($collection, function ($it) { not(oclXin:combined-contains($item, $it))}) "/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Returns <xd:i>true</xd:i> if every item in <doc:i>collection2</doc:i> is not an element
        of <xd:i>collection</xd:i>, <xd:i>false</xd:i> otherwise. </doc:p>
    </doc:desc>
    <doc:param name="collection">Examined collection. </doc:param>
    <doc:param name="collection2">Searched items. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:excludesAll" as="xs:boolean">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="collection2" as="item()*"/>
    
    <xsl:sequence select="oclX:forAll($collection2, function($it) { oclX:excludes($collection, $it) }) "/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Returns sequence containing all elements of <doc:i>collection</doc:i> without
          <doc:i>item</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">Examined collection. </doc:param>
    <doc:param name="item">Excluded item.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:excluding" as="item()*">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="item" as="item()"/>
    <xsl:sequence select="oclX:iterate($collection, (), 
      function($it, $acc) { if (oclXin:combined-eq($it, $item)) then $acc else $acc, $it })" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection1"></doc:param>
    <doc:param name="collection2"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:intersection" as="item()*">
    <xsl:param name="collection1" as="item()*" />
    <xsl:param name="collection2" as="item()*" />
    
    <xsl:sequence select="oclX:iterate(($collection1, $collection2), (), 
      function($it, $acc) { if ((oclXin:combined-contains($collection1, $it)) and 
                                (oclXin:combined-contains($collection2, $it))) 
                                then ($acc,$it) else $acc })" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:first" as="item()">
    <xsl:param name="collection" as="item()*" />
    <xsl:sequence select="oclX:at($collection, 1)" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:last" as="item()">
    <xsl:param name="collection" as="item()*" />
    <xsl:sequence select="oclX:at($collection, count($collection))" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="item"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:indexOf" as="xs:integer">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="item" as="item()" />
    
    <xsl:sequence select="oclXin:combined-first-index-of($collection, $item)" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="index"></doc:param>
    <doc:param name="item"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:insertAt" as="xs:integer">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="index" as="xs:integer" />
    <xsl:param name="item" as="item()" />
    
    <xsl:sequence select="for $i in 1 to count($collection) return 
      if ($i eq $index) then ($item, $collection($i))
      else $collection($i)"/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="item"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:count" as="xs:integer">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="item" as="item()" />
    
    <xsl:sequence select="oclX:iterate($collection, 0, function($it, $acc) { if (oclXin:combined-eq($it, $item)) then $acc + 1 else $acc})" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection1"></doc:param>
    <doc:param name="collection2"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:product" as="item()*">
    <xsl:param name="collection1" as="item()*" />
    <xsl:param name="collection2" as="item()*" />
    
    <!-- OCL definition: 
        self->iterate (e1; acc: Set(Tuple(first: T, second: T2)) = Set{} |
                       c2->iterate (e2; acc2: Set(Tuple(first: T, second: T2)) = acc |
                                    acc2->including (Tuple{first = e1, second = e2}) ) )-->
    
    <!-- implementation according to definition --> 
    <!--<xsl:sequence select="
      oclX:iterate($collection1, (), function($e1, $acc) { oclX:iterate($collection2, $acc, function($e2, $acc2) { oclX:including($acc2, map{'first' := $e1, 'second' := $e2}) } ) } )
      " />-->
    
    <!-- implementation using for --> 
    <xsl:sequence select="for $e1 in $collection1 return for $e2 in $collection2 return map{'first' := $e1, 'second' := $e2}" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="item"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:append" as="item()*">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="item" as="item()"/>  
    <xsl:sequence select="$collection, $item" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="item"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:prepend" as="item()*">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="item" as="item()"/>  
    <xsl:sequence select="$item, $collection" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="item"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:including" as="item()*">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="item" as="item()"/>
    <!--<xsl:message>Adding: <xsl:value-of select="$item('first')"/>,<xsl:value-of select="$item('second')"/></xsl:message>-->
    <xsl:sequence select="oclX:append($collection, $item)" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:isEmpty" as="xs:boolean">
    <xsl:param name="collection" as="item()*" />
    <xsl:sequence select="count($collection) eq 0" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:notEmpty" as="xs:boolean">
    <xsl:param name="collection" as="item()*" />
    <xsl:sequence select="count($collection) ne 0" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="index"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:at" as="item()">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="index" as="xs:integer" />
    <xsl:sequence select="$collection[$index]" />
  </xsl:function>
  
  <!-- todo: union,  setMinus, symmetricDifference,  -->
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="lower"></doc:param>
    <doc:param name="upper"></doc:param>
  </doc:doc>
  <xsl:function name="oclX:subOrderedSet">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="lower" as="xs:integer" />
    <xsl:param name="upper" as="xs:integer" />
    
    <xsl:sequence select="for $index in $lower to $upper return $collection[$index]" />
    
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p></doc:p>
    </doc:desc>
    <doc:param name="collection"></doc:param>
    <doc:param name="lower"></doc:param>
    <doc:param name="upper"></doc:param>
  </doc:doc>
  <xsl:function name="oclX:subSequence">
    <xsl:param name="collection" as="item()*" />
    <xsl:param name="lower" as="xs:integer" />
    <xsl:param name="upper" as="xs:integer" />
    
    <xsl:sequence select="for $index in $lower to $upper return $collection[$index]" />
    
  </xsl:function>
  
  
  <!-- String --> 
  
  <xsl:function name="oclString:at" as="xs:string">
    <xsl:param name="string" as="xs:string" />
    <xsl:param name="index" as="xs:integer" />
    <xsl:sequence select="substring($string, $index, 1)" />
  </xsl:function>
  
  <xsl:function name="oclString:indexOf" as="xs:string">
    <xsl:param name="string" as="xs:string" />
    <xsl:param name="string" as="xs:string" />
    <xsl:sequence select="substring($string, $index, 1)" />
  </xsl:function>
  
  <xsl:function name="oclString:equalsIgnoreCase" as="xs:boolean">
    <xsl:param name="string1" as="xs:string" />
    <xsl:param name="string2" as="xs:string" />
    <xsl:sequence select="lower-case($string1) eq lower-case($string2)" />
  </xsl:function>
  
  <!--
    Helper functions   
  -->
  <xsl:function name="oclXin:functionItemCall">
    <xsl:param name="function" as="function(*)" />
    <xsl:param name="arity" as="xs:integer" />
    <xsl:param name="indices" as="xs:integer*" />
    <xsl:param name="parameters" as="item()*" />
    
    <xsl:sequence select="if ($arity eq 1) then $function($parameters[$indices[1] + 1])
      else if ($arity eq 2) then $function($parameters[$indices[1] + 1], $parameters[$indices[2] + 1])
      else if ($arity eq 3) then $function($parameters[$indices[1] + 1], $parameters[$indices[2] + 1], $parameters[$indices[3] + 1])
      else error(QName('http://eXolutio.com/oclX/functional/error', 'oclXer:E001'), 'A maximum of three different iterators are allowed. ')" />      
  </xsl:function>

  <xsl:function name="oclXin:getIndices" as="xs:integer*">
    <xsl:param name="scalar" as="xs:integer" /> 
    <xsl:param name="base" as="xs:integer" />
    <xsl:param name="length" as="xs:integer" />
    <xsl:param name="result" as="xs:integer*" />
    <xsl:choose>      
      <xsl:when test="$scalar eq 0">
        <xsl:sequence select="
          (for $i in 1 to ($length - count($result)) return 0),
          $result" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="mod" select="$scalar mod $base"/>
        <xsl:sequence select="            
          oclXin:getIndices(xs:integer($scalar div $base), $base, $length, (($mod), $result))" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <xsl:function name="oclXin:combined-first-index-of" as="xs:integer">
    <xsl:param name="sequence" as="item()*"/>
    <xsl:param name="item" as="item()"/>
    
    <xsl:variable name="foundIndices" as="xs:integer*" select="for $i in $sequence return if (oclXin:combined-eq($item, $i)) then $i else ()">
           
    </xsl:variable>
    <xsl:sequence select="if (count($foundIndices) > 0) then $foundIndices[1] else -1" />
  </xsl:function>
  
  <xsl:function name="oclXin:combined-eq" as="xs:boolean">
    <xsl:param name="item1" as="item()"/>
    <xsl:param name="item2" as="item()"/>
    
    <xsl:choose>
      <xsl:when test="$item1 instance of node() and $item2 instance of node()">
        <xsl:sequence select="$item1 is $item2" />
      </xsl:when>
      <xsl:when test="not(exists($item1)) and not(exists($item2))">
        <xsl:sequence select="true()" /> <!-- both () -->
      </xsl:when>
      <xsl:when test="not(exists($item1)) or not(exists($item2))">
        <xsl:sequence select="false()" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:sequence select="deep-equal($item1, $item2)" />
      </xsl:otherwise>      
    </xsl:choose>    
  </xsl:function>

  <xsl:function name="oclXin:combined-contains" as="xs:boolean">
    <xsl:param name="sequence" as="item()*"/>
    <xsl:param name="item" as="item()"/>
    
    <xsl:sequence select="some $i in $sequence satisfies oclXin:combined-eq($item, $i)" />
  </xsl:function>
</xsl:stylesheet>