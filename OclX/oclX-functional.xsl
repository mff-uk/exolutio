<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:oclX="http://eXolutio.com/oclX/functional"
  xmlns:oclXin="http://eXolutio.com/oclX/functional/internal" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:oclDate="http://eXolutio.com/oclX/types/date"
  xmlns:oclString="http://eXolutio.com/oclX/types/string"
  xmlns:oclBoolean="http://eXolutio.com/oclX/types/xor"
  xmlns:doc="http://www.oxygenxml.com/ns/doc/xsl"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:err="http://www.w3.org/2005/xqt-errors"
  xmlns:oclError="http://eXolutio.com/oclX/functional/error"
  exclude-result-prefixes="oclX oclError doc map math err xs oclXin oclDate oclString" 
  version="3.0">

  <doc:doc scope="stylesheet">
    <doc:desc>
      <doc:p><doc:b>Created on:</doc:b> Jan 1, 2012</doc:p>
      <doc:p><doc:b>Author:</doc:b> Jakub Maly</doc:p>
      <doc:p><doc:b>email:</doc:b> jakub@maly.cz</doc:p>

      <doc:p><doc:b>OclX function library</doc:b> contains definition of functions, which enable the
        use of OCL iterator expressions in XPath expressions. The library can be also used in
        Schematron schemas, providing that the Schematron validation process is based on XSLT and
        uses modified Schematron pipeline XSLTs, which add imports of OclX library. </doc:p>
      <doc:p>Look to http://exolutio.com and download eXolutio - a tool which allows translation from
        OCL scripts to Schematron schemas. You can also find the modified Schematron pipeline XSLTs
        on this address. </doc:p>
      <doc:p>The library uses higher-order functions for evaluation of iterator expressions. </doc:p>
      <doc:p>Please, be aware that at this moment, OclX does not support nested collections. </doc:p>
    </doc:desc>
  </doc:doc>

  <doc:doc>
    <doc:desc>
      <doc:p>Print debugging messages</doc:p>
    </doc:desc>
  </doc:doc>
  <xsl:param name="debug" as="xs:boolean" select="false()"/>

  <doc:doc>
    <doc:desc>
      <doc:p>Definitions of runtime OclX errors. </doc:p>
    </doc:desc>
  </doc:doc>
  <xsl:variable name="errors" as="element()*" select="document('oclX-error.xml')//oclError:error" />    
      
  <!-- 
    Iterator Expressions 
  -->

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>forAll</doc:i> function from OCL. Checks, whether a condition holds for
        all member of a collection.</doc:p>
      <doc:p>The function allows multiple iterators in <doc:i>body</doc:i>.</doc:p>
      <doc:p>Results in <doc:i>true</doc:i> if the body expression evaluates to true for each element
        in the source <doc:i>collection</doc:i>; otherwise, result is <doc:i>false</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
      <doc:i>collection</doc:i>. All parameters and result must be of type
      <doc:i>boolean</doc:i>.</doc:param>
    <doc:return>Returns true, if all members of <doc:i>collection</doc:i> satisfy the condition
      defined by <doc:i>expression</doc:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:forAllN" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(*)"/>

    <xsl:variable name="iteratorCount" select="function-arity($body)" as="xs:integer"/>

    <xsl:iterate select="1 to xs:integer(math:pow(count($collection), $iteratorCount))">
      <!-- expect true in the beginning -->
      <xsl:param name="satisfied" as="xs:boolean" select="true()"/>

      <xsl:variable name="indices"
        select="
        oclXin:getIndices(. - 1, count($collection), $iteratorCount, ())"/>

      <xsl:variable name="forThis"
        select="oclXin:functionItemCall(
        $body, $iteratorCount, $indices, $collection)"
        as="xs:boolean?"/>

      <xsl:choose>
        <xsl:when test="boolean($forThis) eq false()">
          <xsl:break>
            <xsl:sequence select="false()"/>
          </xsl:break>
        </xsl:when>
        <xsl:otherwise>
          <xsl:next-iteration>
            <xsl:with-param name="satisfied" select="$satisfied and $forThis"/>
          </xsl:next-iteration>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:on-completion>
        <xsl:sequence select="$satisfied"/>
      </xsl:on-completion>
    </xsl:iterate>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>forAll</doc:i> function from OCL. The implementation uses
          <doc:i>every/satisfies</doc:i> expression internally. Checks, whether a condition holds for
        all member of a collection.</doc:p>
      <doc:p>Results in <doc:i>true</doc:i> if the body expression evaluates to true for each element
        in the source <doc:i>collection</doc:i>; otherwise, result is <doc:i>false</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
      <doc:i>collection</doc:i>. </doc:param>
    <doc:param name="variables"/>
    <doc:return>Returns true, if all members of <doc:i>collection</doc:i> satisfy the condition
      defined by <doc:i>expression</doc:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:forAll" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean?"/>
    <xsl:sequence select="every $it in $collection satisfies $body($it) "/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>exists</doc:i> function from OCL. The implementation uses
          <doc:i>some/satisfies</doc:i> expression internally. Checks, whether a condition holds for
        some member of a collection.</doc:p>
      <doc:p>Results in true if body evaluates to true for at least one element in the source
        collection.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
      <doc:i>collection</doc:i>. </doc:param>
    <doc:return>Returns true, if some member of <doc:i>collection</doc:i> satisfies the condition
      defined by <doc:i>expression</doc:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:exists" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean?"/>
    
    <xsl:sequence select="some $it in $collection satisfies $body($it) "/>

    <!--
    <xsl:sequence
      select="oclX:iterate($collection, false(), 
      function($it, $acc) { $acc or boolean($body($it)) })"/>
    -->
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>exists</doc:i> function from OCL. Checks, whether a condition holds for
        some member of a collection.</doc:p>
      <doc:p>The function allows multiple iterators in <doc:i>body</doc:i>.</doc:p>
      <doc:p>Results in true if body evaluates to true for at least one element in the source
        collection.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
      <doc:i>collection</doc:i>. All parameters and result must be of type <doc:i>boolean</doc:i>. </doc:param>
    <doc:return>Returns true, if some member of <doc:i>collection</doc:i> satisfies the condition
      defined by <doc:i>expression</doc:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:existsN" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(*)"/>

    <xsl:variable name="iteratorCount" select="function-arity($body)" as="xs:integer"/>

    <xsl:iterate select="1 to xs:integer(math:pow(count($collection), $iteratorCount))">
      <!-- expect true in the beginning -->
      <xsl:param name="satisfied" as="xs:boolean" select="true()"/>

      <xsl:message>Iteration: <xsl:value-of select="."/></xsl:message>
      <xsl:variable name="indices"
        select="
        oclXin:getIndices(. - 1, count($collection), $iteratorCount, ())"/>
      
      <xsl:variable name="forThis"
        select="oclXin:functionItemCall(
        $body, $iteratorCount, $indices, $collection)"
        as="xs:boolean?"/>

      <xsl:choose>
        <xsl:when test="boolean($forThis) eq true()">
          <xsl:break>
            <xsl:sequence select="true()"/>
          </xsl:break>
        </xsl:when>
        <xsl:otherwise>
          <xsl:next-iteration>
            <xsl:with-param name="satisfied" select="$satisfied or $forThis"/>
          </xsl:next-iteration>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:on-completion>
        <xsl:sequence select="$satisfied"/>
      </xsl:on-completion>
    </xsl:iterate>
  </xsl:function>


  <doc:doc>
    <doc:desc>
      <doc:p>Implements the fundamental OCL iterator expression <doc:i>iterate</doc:i>. </doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="accInit">The initial value of the accumulator.</doc:param>
    <doc:param name="body">The expression evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
    <doc:return/>
  </doc:doc>
  <xsl:function name="oclX:iterate" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="accInit" as="item()*"/>
    <xsl:param name="body" as="function(item(), item()*) as item()*"/>

    <xsl:iterate select="1 to count($collection)">
      <xsl:param name="acc" select="$accInit" as="item()*"/>
      <xsl:if test="$debug">
        <xsl:message>Iteration: <xsl:value-of select="."/>/<xsl:value-of select="count($collection)"
          /></xsl:message>
        <xsl:message>Current: <xsl:copy-of select="$collection[current()]"/></xsl:message>
      </xsl:if>
      <xsl:next-iteration>
        <xsl:with-param name="acc" select="$body($collection[current()], $acc)"/>
      </xsl:next-iteration>
      <xsl:on-completion>
        <xsl:sequence select="$acc"/>
      </xsl:on-completion>
    </xsl:iterate>
  </xsl:function>


  <doc:doc>
    <doc:desc>
      <doc:p>The collection of elements that results from applying body to every member of the
        source set.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">The expression evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
    <doc:return/>
  </doc:doc>
  <xsl:function name="oclX:collect" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()*"/>
    <xsl:sequence
      select="oclX:iterate($collection, (),
      function($it, $acc) { $body($it), $acc } 
      )"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>The closure of applying body transitively to every distinct element of the source
        collection.</doc:p>
      <doc:p>Computes transitive closure over a collection using an expression. </doc:p>
    </doc:desc>
    <doc:param name="collection">The initial collection</doc:param>
    <doc:param name="body">The expression is evaluated for each member of the initial collection and
      for all other members that are added be the previous calls. It represents one step of
      computation of transitive closure.</doc:param>
    <doc:return>The sequence of items collected during the consecutive evaluations of
        <doc:i>expression</doc:i> over the initial collection and over the items visited by the
      computation during computing transitive closure.</doc:return>
  </doc:doc>
  <xsl:function name="oclX:closure" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()*"/>

    <xsl:sequence select="oclXin:closure-rec(reverse($collection), (), $body)"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Helper function used to cumpute transitive closure using recursion. </doc:p>
    </doc:desc>
    <doc:param name="toDoStack">Items discovered by transitive closure, but not yet processed. </doc:param>
    <doc:param name="result">Transitive closure computed so far. </doc:param>
    <doc:param name="body">The expression is evaluated for each member of the initial collection and
      for all other members that are added be the previous calls. It represents one step of
      computation of transitive closure.</doc:param>
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
      <doc:p>Implements <doc:i>isUnique</doc:i> function from OCL. The implementation uses
          <doc:i>oclX:iterate</doc:i> internally. </doc:p>
      <doc:p>Results in true if body evaluates to a different value for each element in the source
        collection; otherwise, result is false.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Mapping expression evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
    <doc:return>Returns <doc:i>true</doc:i> when elements are distinct in the collection,
        <doc:i>false</doc:i> otherwise. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:isUnique" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()"/>

    <xsl:variable name="uniqueItems" as="item()*">
      <xsl:sequence
        select="oclX:iterate(
        $collection, (), function($it, $acc) { 
        $acc, if (oclXin:combined-contains($acc, $body($it))) then () else $body($it)
        })"
      />
    </xsl:variable>
    <xsl:message select="count($uniqueItems)"></xsl:message>
    <xsl:message select="count($collection)"></xsl:message>
    <xsl:sequence select="if (count($uniqueItems) eq count($collection)) then true() else false()"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>select</doc:i> function from OCL. </doc:p>
      <doc:p>The subsequence of the source <doc:i>collection</doc:i> for which body is
          <doc:i>true</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">The expression evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
    <doc:return>The subsequence of the source <doc:i>collection</doc:i> for which body is
        <doc:i>true</doc:i>.</doc:return>
  </doc:doc>
  <xsl:function name="oclX:select" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean?"/>

    <xsl:sequence
      select="oclX:iterate($collection, (), 
      function($it, $acc) { if (boolean($body($it))) then oclX:including($acc, $it) else $acc })"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>reject</doc:i> function from OCL. </doc:p>
      <doc:p>The subsequence of the source <doc:i>collection</doc:i> for which <doc:i>body</doc:i> is
        false.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">The expression evaluated for each member of
      <doc:i>collection</doc:i></doc:param>
    <doc:return>The subsequence of the source <doc:i>collection</doc:i> for which <doc:i>body</doc:i> is
      false.</doc:return>
  </doc:doc>
  <xsl:function name="oclX:reject" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean?"/>

    <xsl:sequence
      select="oclX:iterate($collection, (), 
      function($it, $acc) { if (not($body($it))) then oclX:including($acc, $it) else $acc })"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>any</doc:i> function from OCL. The implementation uses
          <doc:i>oclX:iterate</doc:i> internally. </doc:p>
      <doc:p>Returns any element in the source collection for which <doc:i>body</doc:i> evaluates to
          <doc:i>true</doc:i>. If there is more than one element for which <doc:i>body</doc:i> is
          <doc:i>true</doc:i>, one of them is returned. There must be at least one element fulfilling
          <doc:i>body</doc:i>, otherwise the result is an empty sequence.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
    <doc:return>A member of <doc:i>collection</doc:i> satisfying <doc:i>body</doc:i>, if such an element
      exists, an empty sequence otherwise. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:any" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean?"/>

    <xsl:variable name="satisfyingItems" as="item()*">
      <xsl:sequence
        select="oclX:iterate(
        $collection, (),            
        function($it, $acc) { $acc, if (boolean($body($it)) eq true()) then $it else () }
        )"
      />
    </xsl:variable>    
    <xsl:sequence select="if (count($satisfyingItems) ge 1) then $satisfyingItems[1] else ()  "/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>one</doc:i> function from OCL. The implementation uses
          <doc:i>oclX:iterate</doc:i> internally. </doc:p>
      <doc:p>Results in <doc:i>true</doc:i> if there is exactly one element in the source
          <doc:i>collection</doc:i> for which <doc:i>body</doc:i> is <doc:i>true</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:one" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean?"/>

    <xsl:variable name="satisfyingItems" as="item()*">
      <xsl:sequence
        select="oclX:iterate(
        $collection, (),            
        function($it, $acc) { $acc, if (boolean($body($it)) eq true()) then $it else () }
        )"
      />
    </xsl:variable>
    <xsl:sequence select="if (count($satisfyingItems) eq 1) then true() else false()  "/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>sortedBy</doc:i> function from OCL. </doc:p>
      <doc:p>Results in the collection containing all elements of the source
        <doc:i>collection</doc:i>. The element for which <doc:i>body</doc:i> has the lowest value
        comes first, and so on. Uses <doc:i>xsl:perform-sort</doc:i> internally. </doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
    <doc:return>Ordered sequence. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:sortedBy" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()*"/>

    <xsl:perform-sort select="$collection">
      <xsl:sort select="$body(.)"/>
    </xsl:perform-sort>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Groups elements in the input sequence by the value of the grouping 
      key into distinct partitions. </doc:p>      
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Expression evaluated for each member of
      <doc:i>collection</doc:i>. </doc:param>
    <doc:return>Result of groupBy is a sequence of maps. For each distinct grouping key, one 
    map is added to the result. Each map has 2 keys - 'key' containing the value of the grouping
    key - and 'partition' containing the sequence of items, for which <doc:i>body</doc:i> returns
    the key value. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:groupBy" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as item()"/>
    
    <xsl:iterate select="$collection">
      <!-- expect true in the beginning -->
      <xsl:param name="grouped" as="item()*" select="()"/>      
      <xsl:variable name="key" select="$body(current())" />         
      <xsl:variable name="match" select="$grouped[oclX:oclEqual(.('key'), $key)]" as="item()*" />
      <xsl:choose>
        <xsl:when test="exists($match)">
          <xsl:variable name="updatedPartition" select="
            map { 'key' := $key, 'partition' := ($match('partition'), current()) }" />          
          <xsl:next-iteration>            
            <xsl:with-param name="grouped" select="
              for $partition in $grouped return if (oclX:oclEqual($partition('key'), $key)) then $updatedPartition else $partition"/>
          </xsl:next-iteration>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="newPartition" select="map { 'key' := $key, 'partition' := current() }" />
          <xsl:next-iteration>
            <xsl:with-param name="grouped" select="$grouped, $newPartition"/>
          </xsl:next-iteration>
        </xsl:otherwise>
      </xsl:choose>      
      <xsl:on-completion>
        <xsl:sequence select="$grouped"/>
      </xsl:on-completion>
    </xsl:iterate>    
  </xsl:function>
  
  <!-- 
    collection 
  -->

  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>true</doc:i> if <doc:i>item</doc:i> is an element of
        <doc:i>collection</doc:i>, <doc:i>false</doc:i> otherwise.</doc:p>
    </doc:desc>
    <doc:param name="collection">The examined collection.</doc:param>
    <doc:param name="item">Searched item.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:includes" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="item" as="item()"/>

    <xsl:sequence
      select="oclX:exists($collection, function ($it) { oclXin:combined-contains($item, $it)}) "/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>true</doc:i> if every item in <doc:i>collection2</doc:i> is an element of
          <doc:i>collection</doc:i>, <doc:i>false</doc:i> otherwise. </doc:p>
    </doc:desc>
    <doc:param name="collection">Examined collection. </doc:param>
    <doc:param name="collection2">Searched items. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:includesAll" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>

    <xsl:sequence
      select="oclX:forAll($collection2, function($it) { oclX:includes($collection, $it) }) "/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>false</doc:i> if <doc:i>item</doc:i> is an element of
          <doc:i>collection</doc:i>, <doc:i>true</doc:i> otherwise.</doc:p>
    </doc:desc>
    <doc:param name="collection">The examined collection.</doc:param>
    <doc:param name="item">Searched item.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:excludes" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="item" as="item()"/>

    <xsl:sequence
      select="oclX:forAll($collection, function ($it) { not(oclXin:combined-contains($item, $it))}) "
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>true</doc:i> if every item in <doc:i>collection2</doc:i> is not an element
        of <doc:i>collection</doc:i>, <doc:i>false</doc:i> otherwise. </doc:p>
    </doc:desc>
    <doc:param name="collection">Examined collection. </doc:param>
    <doc:param name="collection2">Searched items. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:excludesAll" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>

    <xsl:sequence
      select="oclX:forAll($collection2, function($it) { oclX:excludes($collection, $it) }) "/>
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
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="item" as="item()"/>
    <xsl:sequence
      select="oclX:iterate($collection, (), 
      function($it, $acc) { if (oclXin:combined-eq($it, $item)) then $acc else $acc, $it })"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Computes intersection of <doc:i>collection1</doc:i> and <doc:i>collection2</doc:i>.
      </doc:p>
    </doc:desc>
    <doc:param>First intersected collection.</doc:param>
    <doc:param name="collection2">Second intersected collection. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:intersection" as="item()*">
    <xsl:param name="collection1" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>

    <xsl:sequence
      select="oclX:iterate(($collection1, $collection2), (), 
      function($it, $acc) { if ((oclXin:combined-contains($collection1, $it)) and 
                                (oclXin:combined-contains($collection2, $it))) 
                                then ($acc,$it) else $acc })"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns the first item in <doc:i>collection</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The input collection. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:first" as="item()">
    <xsl:param name="collection" as="item()*"/>
    <xsl:choose>
      <xsl:when test="count($collection) eq 0">
        <xsl:if test="$debug">
          <xsl:message select="'Attempt to get first item in an empty collection'"></xsl:message>
        </xsl:if>
        <xsl:sequence select="
          error(QName('http://eXolutio.com/oclX/functional/error', 'oclError:E004'), 
          $errors[@xml:id='E004']/oclError:description/text())" />
      </xsl:when>
      <xsl:otherwise>        
        <xsl:sequence select="oclX:at($collection, 1)"/>
      </xsl:otherwise>
    </xsl:choose>       
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns the last item in <doc:i>collection</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The input collection. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:last" as="item()">
    <xsl:param name="collection" as="item()*"/>
    
    <xsl:choose>
      <xsl:when test="count($collection) eq 0">
        <xsl:if test="$debug">
          <xsl:message select="'Attempt to get last item in an empty collection'"></xsl:message>
        </xsl:if>
        <xsl:sequence select="
          error(QName('http://eXolutio.com/oclX/functional/error', 'oclError:E004'), 
          $errors[@xml:id='E004']/oclError:description/text())" />
      </xsl:when>
      <xsl:otherwise>        
        <xsl:sequence select="oclX:at($collection, count($collection))"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns the index of <doc:i>item</doc:i> in the <doc:i>collection</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The searched collection. </doc:param>
    <doc:param name="item">The searched item. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:indexOf" as="xs:integer">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="item" as="item()"/>

    <xsl:sequence select="oclXin:combined-first-index-of($collection, $item)"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns a new collection, which is a copy of <doc:i>collection</doc:i> with
          <doc:i>item</doc:i> inserted at the <doc:i>index</doc:i>. </doc:p>
    </doc:desc>
    <doc:param name="collection">The input collection. </doc:param>
    <doc:param name="index">Index, where item is inserted. </doc:param>
    <doc:param name="item">Added item. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:insertAt" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="index" as="xs:integer"/>
    <xsl:param name="item" as="item()"/>

    <xsl:sequence
      select="
      if ((count($collection) + 1) ge $index) then
        for $i in 1 to (count($collection) + 1) return 
          if ($i eq $index) then ($item, $collection[$i])
          else $collection[$i]
      else error(QName('http://eXolutio.com/oclX/functional/error', 'oclError:E002'), 
                 $errors[@xml:id='E002']/oclError:description/text(), $index)"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns the count of occurences of <doc:i>item</doc:i> in <doc:i>collection</doc:i>.
      </doc:p>
    </doc:desc>
    <doc:param name="collection">The searched collection.</doc:param>
    <doc:param name="item">Searched item. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:count" as="xs:integer">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="item" as="item()"/>

    <xsl:sequence
      select="oclX:iterate($collection, 0, function($it, $acc) { if (oclXin:combined-eq($it, $item)) then $acc + 1 else $acc})"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>The cartesian product of collection1 and collection2.</doc:p>
    </doc:desc>
    <doc:param name="collection1">First collection. </doc:param>
    <doc:param name="collection2">Second collection. </doc:param>
    <doc:return>The sequence of maps (2-tuples), The maps have keys <doc:i>first</doc:i> and
        <doc:i>second</doc:i>. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:product" as="item()*">
    <xsl:param name="collection1" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>

    <!-- OCL definition: 
        self->iterate (e1; acc: Set(Tuple(first: T, second: T2)) = Set{} |
                       c2->iterate (e2; acc2: Set(Tuple(first: T, second: T2)) = acc |
                                    acc2->including (Tuple{first = e1, second = e2}) ) )-->

    <!-- implementation according to definition -->
    <!--<xsl:sequence select="
      oclX:iterate($collection1, (), function($e1, $acc) { oclX:iterate($collection2, $acc, function($e2, $acc2) { oclX:including($acc2, map{'first' := $e1, 'second' := $e2}) } ) } )
      " />-->

    <!-- implementation using for -->
    <xsl:sequence
      select="for $e1 in $collection1 return for $e2 in $collection2 return map{'first' := $e1, 'second' := $e2}"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Appends <doc:i>item</doc:i> to <doc:i>collection</doc:i>. </doc:p>
    </doc:desc>
    <doc:param name="collection">The initial collection. </doc:param>
    <doc:param name="item">Item to append. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:append" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="item" as="item()"/>
    <xsl:sequence select="$collection, $item"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>Adds <doc:i>item</doc:i> to the beginning of <doc:i>collection</doc:i>. </doc:desc>
    <doc:param name="collection">The initial collection. </doc:param>
    <doc:param name="item">Item to append.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:prepend" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="item" as="item()"/>
    <xsl:sequence select="$item, $collection"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Adds <doc:i>item</doc:i> to <doc:i>collection</doc:i>. </doc:p>
    </doc:desc>
    <doc:param name="collection">The initial collection.</doc:param>
    <doc:param name="item">Item to add.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:including" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="item" as="item()"/>
    <!--<xsl:message>Adding: <xsl:value-of select="$item('first')"/>,<xsl:value-of select="$item('second')"/></xsl:message>-->
    <xsl:sequence select="oclX:append($collection, $item)"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>true</doc:i> when <doc:i>collection</doc:i> is empty,
          <doc:i>false</doc:i> otherwise. </doc:p>
    </doc:desc>
    <doc:param name="collection">The examined collection. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:isEmpty" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:sequence select="count($collection) eq 0"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>true</doc:i> when <doc:i>collection</doc:i> is not empty,
          <doc:i>false</doc:i> otherwise. </doc:p>
    </doc:desc>
    <doc:param name="collection">The examined collection. </doc:param>
  </doc:doc>
  <xsl:function name="oclX:notEmpty" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:sequence select="count($collection) ne 0"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns an item on a given index. </doc:p>
    </doc:desc>
    <doc:param name="collection">The searched collection. </doc:param>
    <doc:param name="index">Index in collection (1-based).</doc:param>
  </doc:doc>
  <xsl:function name="oclX:at" as="item()">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="index" as="xs:integer"/>
    
    <xsl:sequence select="
      if (count($collection) ge $index) then $collection[$index] else 
        error(QName('http://eXolutio.com/oclX/functional/error', 'oclError:E002'), 
        $errors[@xml:id='E002']/oclError:description/text(), $index)"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Computes a union of the two collections (duplicates removed). </doc:p>
    </doc:desc>
    <doc:param name="collection1">The first collection. </doc:param>
    <doc:param name="collection2">The second collection.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:union" as="item()*">
    <xsl:param name="collection1" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>

    <xsl:sequence
      select="oclX:iterate($collection2, oclX:asSet($collection1), 
      function($it, $acc) { if (oclX:includes($acc, $it)) then $acc else oclX:including($acc, $it)  }) "
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Computes a union of the two collections (duplicates removed). </doc:p>
    </doc:desc>
    <doc:param name="collection1">The first collection. </doc:param>
    <doc:param name="collection2">The second collection.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:setMinus" as="item()*">
    <xsl:param name="collection1" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>

    <xsl:sequence
      select="oclX:iterate($collection2, oclX:asSet($collection1), 
      function($it, $acc) { if (not(oclX:includes($collection2, $it))) then $acc else oclX:excluding($acc, $it)  }) "
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Computes a union of the two collections (duplicates removed). </doc:p>
    </doc:desc>
    <doc:param name="collection1">The first collection. </doc:param>
    <doc:param name="collection2">The second collection.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:symmetricDifference" as="item()*">
    <xsl:param name="collection1" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>

    <xsl:sequence
      select="oclX:iterate(oclX:asSet($collection2), oclX:asSet($collection1), 
      function($it, $acc) { if (oclX:includes($acc, $it)) then oclX:excluding($acc, $it) else oclX:including($acc, $it) } )"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Removes duplicates from <doc:i>collection</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The initial collection.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:asSet" as="item()*">
    <xsl:param name="collection" as="item()*"/>

    <xsl:sequence
      select="oclX:iterate($collection, (), 
      function($it, $acc) { if (oclX:includes($acc, $it)) then $acc else oclX:including($acc, $it) } )"
    />
  </xsl:function>

  <xsl:function name="oclX:setEqual" as="item()*">
    <xsl:param name="collection1" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>
    
    <xsl:sequence
      select="(count($collection1) eq count($collection2)) and 
        oclX:includesAll($collection1, $collection2)"
    />
  </xsl:function>
  
  <xsl:function name="oclX:seqEqual" as="item()*">
    <xsl:param name="collection1" as="item()*"/>
    <xsl:param name="collection2" as="item()*"/>
    
    <xsl:sequence
      select="(count($collection1) eq count($collection2)) and 
      (every $index in 1 to count($collection1) satisfies oclX:oclEqual($collection1[$index], $collection2[$index]))"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns an ordered sub-collection of an ordered collection. </doc:p>
    </doc:desc>
    <doc:param name="collection">The initial collection (ordered). </doc:param>
    <doc:param name="lower">Start index in <doc:i>collection</doc:i>.</doc:param>
    <doc:param name="upper">End index in <doc:i>collection</doc:i>.</doc:param>
    <doc:return>Collection including items between indices <doc:i>lower</doc:i> and
        <doc:i>upper</doc:i> (both inclusive)</doc:return>
  </doc:doc>
  <xsl:function name="oclX:subOrderedSet">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="lower" as="xs:integer"/>
    <xsl:param name="upper" as="xs:integer"/>

    <xsl:choose>
      <xsl:when test="($lower gt $upper) or
        ($lower gt count($collection)) or
        ($upper gt count($collection)) or
        ($lower le 0) or 
        ($upper le 0)">
        <xsl:if test="$debug">
          <xsl:message select="
            'Failed attemt to get a subseqeunce of a sequence of length: ' || count($collection) || 
            ' , lower index: ' || $lower || ' upper index: ' || $upper "/>
        </xsl:if>
        <xsl:sequence select="
          error(QName('http://eXolutio.com/oclX/functional/error', 'oclError:E002'), 
          $errors[@xml:id='E002']/oclError:description/text(), ($lower, $upper))" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:sequence select="for $index in $lower to $upper return $collection[$index]"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns a subsequence of a sequence. </doc:p>
    </doc:desc>
    <doc:param name="collection">The initial collection (ordered). </doc:param>
    <doc:param name="lower">Start index in <doc:i>collection</doc:i>.</doc:param>
    <doc:param name="upper">End index in <doc:i>collection</doc:i>.</doc:param>
    <doc:return>Collection including items between indices <doc:i>lower</doc:i> and
        <doc:i>upper</doc:i> (both inclusive).</doc:return>
  </doc:doc>
  <xsl:function name="oclX:subSequence">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="lower" as="xs:integer"/>
    <xsl:param name="upper" as="xs:integer"/>

    <xsl:choose>
      <xsl:when test="($lower gt $upper) or
        ($lower gt count($collection)) or
        ($upper gt count($collection)) or
        ($lower le 0) or 
        ($upper le 0)">
        <xsl:if test="$debug">
          <xsl:message select="
            'Failed attemt to get a subseqeunce of a sequence of length: ' || count($collection) || 
            ' , lower index: ' || $lower || ' upper index: ' || $upper "/>
        </xsl:if>
        <xsl:sequence select="
          error(QName('http://eXolutio.com/oclX/functional/error', 'oclError:E002'), 
          $errors[@xml:id='E002']/oclError:description/text(), ($lower, $upper))" />      
      </xsl:when>
      <xsl:otherwise>
        <xsl:sequence select="for $index in $lower to $upper return $collection[$index]"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <!-- String -->

  <doc:doc>
    <doc:desc>
      <doc:p>Reterns a character at a given index in the input string. </doc:p>
    </doc:desc>
    <doc:param name="string">Input string. </doc:param>
    <doc:param name="index">Index in input string. </doc:param>
  </doc:doc>
  <xsl:function name="oclString:at" as="xs:string">
    <xsl:param name="string" as="xs:string"/>
    <xsl:param name="index" as="xs:integer"/>
    
    <xsl:sequence select="
      if (string-length($string) ge $index) then substring($string, $index, 1) else 
        error(QName('http://eXolutio.com/oclX/functional/error', 'oclError:E005'), 
        $errors[@xml:id='E005']/oclError:description/text())"/>
 
  </xsl:function>

  <doc:doc>
    <doc:desc>Returns index of <doc:i>subString</doc:i> in the input
      <doc:i>string</doc:i>.</doc:desc>
    <doc:param name="string">Examined string. </doc:param>
    <doc:param name="subString">Searched substring.</doc:param>
    <doc:return>First index of <doc:i>subString</doc:i> in string when found, -1 otherwise.
    </doc:return>
  </doc:doc>
  <xsl:function name="oclString:indexOf" as="xs:integer">
    <xsl:param name="string" as="xs:string"/>
    <xsl:param name="subString" as="xs:string"/>
    <xsl:sequence
      select="if (contains($string, $subString)) then string-length(substring-before($string, $subString)) else -1"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns whether the strings are equal when case is ignored. </doc:p>
    </doc:desc>
    <doc:param name="string1">First compared string. </doc:param>
    <doc:param name="string2">Second compared string. </doc:param>
  </doc:doc>
  <xsl:function name="oclString:equalsIgnoreCase" as="xs:boolean">
    <xsl:param name="string1" as="xs:string"/>
    <xsl:param name="string2" as="xs:string"/>
    <xsl:sequence select="lower-case($string1) eq lower-case($string2)"/>
  </xsl:function>

  <!-- Date -->

  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>true</doc:i> when <doc:i>date2</doc:i> follows after
          <doc:i>date1</doc:i> (also when the dates are equal). </doc:p>
    </doc:desc>
    <doc:param name="date1">First date. </doc:param>
    <doc:param name="date2">Second date. </doc:param>
  </doc:doc>
  <xsl:function name="oclDate:after" as="xs:boolean">
    <xsl:param name="date1"/>
    <xsl:param name="date2"/>
    <xsl:sequence select="xs:dateTime($date1) ge xs:dateTime($date2)"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>Returns <doc:i>true</doc:i> when <doc:i>date1</doc:i> follows after
        <doc:i>date2</doc:i> (also when the dates are equal). </doc:desc>
    <doc:param name="date1">First date. </doc:param>
    <doc:param name="date2">Second date. </doc:param>
  </doc:doc>
  <xsl:function name="oclDate:before" as="xs:boolean">
    <xsl:param name="date1"/>
    <xsl:param name="date2"/>
    <xsl:sequence select="xs:dateTime($date2) ge xs:dateTime($date1)"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>Returns date from a dateTime.</doc:desc>
    <doc:param name="dateTime">Input date. </doc:param>
  </doc:doc>
  <xsl:function name="oclDate:getDate" as="xs:date">
    <xsl:param name="dateTime" as="xs:dateTime"/>
    <xsl:sequence select="xs:date(format-dateTime($dateTime, '[Y]-[M,2]-[D,2]'))"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>Returns a dateTime value where the time component is truncated.</doc:desc>
    <doc:param name="dateTime">Input dateTime. </doc:param>
  </doc:doc>
  <xsl:function name="oclDate:trunc" as="xs:dateTime">
    <xsl:param name="dateTime" />
    <xsl:sequence select="xs:dateTime(xs:date(format-dateTime($dateTime, '[Y]-[M,2]-[D,2]')))"/>
  </xsl:function>

  <!-- 
    Boolean
    -->
  <doc:doc>
    <doc:desc>Computes logical xor.</doc:desc>
  </doc:doc>
  <xsl:function name="oclBoolean:xor" as="xs:boolean">
    <xsl:param name="arg1" as="xs:boolean"/>    
    <xsl:param name="arg2" as="xs:boolean"/>
    
    <xsl:sequence select="($arg1 and not($arg2)) or (not($arg1) and $arg2)"/>
  </xsl:function>

  <!--
    Helper functions   
  -->

  <doc:doc>
    <doc:desc>
      <doc:p>Evaluates <doc:i>func</doc:i> and returns the result. 
      If the evaluation fails due to a dynamic error, the error 
      is caught and the result is <doc:i>false</doc:i></doc:p>
    </doc:desc>
    <doc:param name="func"></doc:param>    
  </doc:doc>
  <xsl:function name="oclX:checked" as="item()*">
    <xsl:param name="func" as="function() as item()*" />
    <xsl:try select="$func()">
      <xsl:catch>
        <xsl:if test="$debug">
          <xsl:message select="'Runtime error occured during execution making the result ''invalid''. '"/>
          <xsl:message select="'  - code: ' || $err:code"/>
          <xsl:message select="'  - description: ' || $err:description"/>
          <xsl:message select="'  - value: ' || $err:value"/>
        </xsl:if>
        <xsl:sequence select="false()" />
      </xsl:catch>
    </xsl:try>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>false</doc:i> if <doc:i>func</doc:i> evaluates 
        successfuly, <doc:i>true</doc:i> otherwise. The execution of this function never 
        fails due to a dynamic error. </doc:p>
    </doc:desc>
    <doc:param name="func">Evaluated function.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:oclIsInvalid" as="xs:boolean">
    <xsl:param name="func" as="function() as item()*" />
    
    <!-- evaluate func and forget the result, return false if evaluation succeeds --> 
    <xsl:try select="let $result := $func() return false()">
      <xsl:catch>
        <xsl:if test="$debug">
          <xsl:message select="'Runtime error occured during execution making the result ''invalid''. '"/>
          <xsl:message select="'  - code: ' || $err:code"/>
          <xsl:message select="'  - description: ' || $err:description"/>
          <xsl:message select="'  - value: ' || $err:value"/>
        </xsl:if>
        <!-- if function call fails, return true  -->
        <xsl:sequence select="true()" />
      </xsl:catch>
    </xsl:try>    
  </xsl:function>
   
  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>true</doc:i> if <doc:i>func</doc:i> evaluates 
      successfuly, <doc:i>false</doc:i> otherwise. The execution of this function never 
      fails due to a dynamic error. </doc:p>
    </doc:desc>
    <doc:param name="func">Evaluated function.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:oclIsValid" as="xs:boolean">
    <xsl:param name="func" as="function() as item()*" /> 
    <xsl:sequence select="not(oclX:oclIsInvalid($func))" />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Always results in dynamic error http://eXolutio.com/oclX/functional/error:E010.
        The function is used to represent <doc:i>invalid</doc:i> literal from OCL. </doc:p>
    </doc:desc>
    <doc:return></doc:return>
  </doc:doc>
  <xsl:function name="oclX:invalid" as="item()*">
    <xsl:sequence
      select="error(QName('http://eXolutio.com/oclX/functional/error', 'oclError:E010'), 
      $errors[@xml:id='E010']/oclError:description/text())" 
    />
    
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Helper function used internally by iterator functions allowing multiple iterators, such
        as <doc:i>forAllN</doc:i> and <doc:i>existsN</doc:i>.</doc:p>
      <doc:p>Expects a function item with arity equal to the value of <doc:i>arity</doc:i>. The item
        is called, members of the sequence <doc:i>parameters</doc:i> are passed as parameters to the
        function call, member at index $indices[1] is passed as the first parameters, member at
        index $indes[2] as second parameter etc. </doc:p>
    </doc:desc>
    <doc:param name="function">Function item. </doc:param>
    <doc:param name="arity">Arity of the function item. </doc:param>
    <doc:param name="indices">Sequence of numbers of length equal to
      <doc:i>arity</doc:i>.</doc:param>
    <doc:param name="parameters">Sequence of parameters to the function item call. </doc:param>
  </doc:doc>
  <xsl:function name="oclXin:functionItemCall">
    <xsl:param name="function" as="function(*)"/>
    <xsl:param name="arity" as="xs:integer"/>
    <xsl:param name="indices" as="xs:integer*"/>
    <xsl:param name="parameters" as="item()*"/>

    <xsl:sequence
      select="if ($arity eq 1) then $function($parameters[$indices[1] + 1])
      else if ($arity eq 2) then $function($parameters[$indices[1] + 1], $parameters[$indices[2] + 1])
      else if ($arity eq 3) then $function($parameters[$indices[1] + 1], $parameters[$indices[2] + 1], $parameters[$indices[3] + 1])
      else error(QName('http://eXolutio.com/oclX/functional/error', 'oclXer:E001'), 
                 $errors[@xml:id='E001']/oclError:description/text())"
    />
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Returns the n-th number in a number system with base equal to <doc:i>base</doc:i>,
        where the value n is passed as <doc:i>scalar</doc:i> argument. The number is zero-filled
        from on the left to length equal to <doc:i>length</doc:i>. </doc:p>
    </doc:desc>
    <doc:param name="scalar">The desired number as a natural number. </doc:param>
    <doc:param name="base">Base of the number system. </doc:param>
    <doc:param name="length">Desired length of the result.</doc:param>
    <doc:param name="result">Used in recursive calls, expects empty sequence when not called
      recursively. </doc:param>
    <doc:return>The result is a sequence of integers of length equal to
      <doc:i>length</doc:i>.</doc:return>
  </doc:doc>
  <xsl:function name="oclXin:getIndices" as="xs:integer*">
    <xsl:param name="scalar" as="xs:integer"/>
    <xsl:param name="base" as="xs:integer"/>
    <xsl:param name="length" as="xs:integer"/>
    <xsl:param name="result" as="xs:integer*"/>
    <xsl:choose>
      <xsl:when test="$scalar eq 0">
        <xsl:sequence
          select="
          (for $i in 1 to ($length - count($result)) return 0),
          $result"
        />
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="mod" select="$scalar mod $base"/>
        <xsl:sequence
          select="            
          oclXin:getIndices(xs:integer($scalar div $base), $base, $length, (($mod), $result))"
        />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Helper function used by those library functions comparing items. </doc:p>
      <doc:p>Returns the first index of <doc:i>item</doc:i> in <doc:i>sequence</doc:i>. Sequence may
        contain atomic values and nodes. Nodes are compared by identity. When item is not found, the
        result is -1.</doc:p>
    </doc:desc>
    <doc:param name="sequence">Examined sequence.</doc:param>
    <doc:param name="item"/>
  </doc:doc>
  <xsl:function name="oclXin:combined-first-index-of" as="xs:integer">
    <xsl:param name="sequence" as="item()*"/>
    <xsl:param name="item" as="item()"/>

    <xsl:variable name="foundIndices" as="xs:integer*"
      select="for $i in $sequence return if (oclXin:combined-eq($item, $i)) then $i else ()"> </xsl:variable>
    <xsl:sequence select="if (count($foundIndices) > 0) then $foundIndices[1] else -1"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Helper function used by those library functions comparing items. </doc:p>
      <doc:p>Returns true when the two items are equal, false otherwise. Items may be atomic values
        and nodes. Nodes are compared by identity. </doc:p>
    </doc:desc>
    <doc:param name="item1">The first item. </doc:param>
    <doc:param name="item2">The second item. </doc:param>
  </doc:doc>
  <xsl:function name="oclXin:combined-eq" as="xs:boolean">
    <xsl:param name="item1" as="item()"/>
    <xsl:param name="item2" as="item()"/>

    <xsl:choose>
      <xsl:when test="$item1 instance of node() and $item2 instance of node()">
        <xsl:sequence select="$item1 is $item2"/>
      </xsl:when>
      <xsl:when test="not(exists($item1)) and not(exists($item2))">
        <xsl:sequence select="true()"/>
        <!-- both () -->
      </xsl:when>
      <xsl:when test="not(exists($item1)) or not(exists($item2))">
        <xsl:sequence select="false()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:sequence select="deep-equal($item1, $item2)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>
  
  <xsl:function name="oclX:oclEqual" as="xs:boolean">
    <xsl:param name="collection1" as="item()?" />
    <xsl:param name="collection2" as="item()?" />   
    
    <xsl:choose>
      <xsl:when test="count($collection1) eq 0 and count($collection2) eq 0">
        <xsl:sequence select="true()" />
      </xsl:when>
      
      <xsl:when test="count($collection1) ne count($collection2)">
        <xsl:sequence select="false()" />
      </xsl:when>
      
      <xsl:otherwise>
        <xsl:sequence select="every $index in 1 to count($collection1) satisfies 
          oclXin:combined-eq($collection1[$index], $collection2[$index])"/> 
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:function>

  <doc:doc>
    <doc:desc>Helper function used by the functions which search sequences. <doc:p>Returns true when
          <doc:i>sequence</doc:i> contains <doc:i>item</doc:i>. Items may be atomic values and
        nodes. Nodes are compared by identity. </doc:p></doc:desc>
    <doc:param name="sequence">The examined sequence. </doc:param>
    <doc:param name="item">The searched item. </doc:param>
  </doc:doc>
  <xsl:function name="oclXin:combined-contains" as="xs:boolean">
    <xsl:param name="sequence" as="item()*"/>
    <xsl:param name="item" as="item()"/>

    <xsl:sequence select="some $i in $sequence satisfies oclXin:combined-eq($item, $i)"/>
  </xsl:function>
</xsl:stylesheet>
