<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:saxon="http://saxon.sf.net/"
  xmlns:oclX="http://eXolutio.com/oclX/functional"
  xmlns:oclXin="http://eXolutio.com/oclX/functional/internal"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:xdt="http://www.w3.org/2005/02/xpath-datatypes"
  xmlns:fn="http://www.w3.org/2005/xpath-functions"
  exclude-result-prefixes="saxon xs oclX map math fn oclXin xdt" version="3.0">

  <xsl:import href="../oclX-functional.xsl"/>

  <xsl:param name="debug" as="xs:boolean" select="true()"/>

  <xsl:output indent="yes"/>

  <xsl:function name="oclXin:mapPrint">
    <xsl:param name="m"/>

    <xsl:for-each select="map:keys($m)">
      <xsl:copy-of select="current()"/>
      <xsl:text> := </xsl:text>
      <xsl:copy-of select="($m)(current())"/>
    </xsl:for-each>

  </xsl:function>

  <xsl:function name="oclXin:wrap" as="item()*">
   <xsl:param name="pNode" as="node()" />
   <xsl:variable name="t">
     <www>1</www>
     <xsl:sequence select="$pNode"/>
     <xxx>2</xxx>
   </xsl:variable>
   <xsl:sequence select="$t/*"/>
   </xsl:function>
  
  <xsl:template match="/">
    <xsl:copy-of select="//b/(function($b) { oclXin:wrap($b/c) })(.) "/>
    <xsl:copy-of select="//b/oclXin:wrap(./c)  "/>
    <!--<xsl:copy-of select="concat('asdf', 'fdsa')" />-->

    <!--<xsl:variable name="v" select="map{ 'a' := (1,2,3), 'b' := (3,2,1) }, map{'a' := (6,6,6) } " />
    
    <xsl:for-each select="$v">
      <xsl:copy-of select="oclXin:mapPrint(.)" />  
    </xsl:for-each>-->

    <!--<xsl:for-each select="oclX:groupBy(//b, function($b) { xs:string($b/c) })" >
      <xsl:text>{ </xsl:text>
      <xsl:copy-of select="oclXin:mapPrint(.)" />
      <xsl:text> }</xsl:text>
    </xsl:for-each>
    -->
    <!--
    <xsl:copy-of select=" let $self := //tournaments return
      oclX:any($self/tournament, function($t) { xs:boolean($t/qualification/@open ) })
    "></xsl:copy-of>
    -->

    <!--<xsl:value-of select="oclX:forAll1($c/n, function($x) { xs:integer($x) ge 5 })"/>-->
    <!--<xsl:value-of select="
      oclX:forAllM($c, 'it', 
      function($it, $v) { $it > 2 }, 
      map { })"/>-->
    <!--<xsl:value-of select="
      oclX:forAllM($c/n, 'x', 
      function($x, $v) { oclX:forAllM2($c/n, 'y', function($y, $v) { if ($x is $y) then true() else $x ne $y }, $v) }, 
      map { })"/>-->

    <!--<xsl:evaluate xpath="'1+1'"/>-->

    <!--<xsl:evaluate xpath="'oclX:ss($x)'">
      <xsl:with-param name="x" select="{1}" />
    </xsl:evaluate>-->

    <!--<xsl:value-of
      select="
      oclX:forAll2((4,5,6), function($ as item()) as xs:boolean { $it > 3 },())"/>
-->
    <!--<xsl:value-of select="(map { 'f':= 'x', 1 := 'aaa'})('f')" />-->

  </xsl:template>

  <!--  <xsl:template match="/">
     <xsl:copy-of select="/aa/b/oclX:foo(.)"/>
     
  </xsl:template>    
    
  <xsl:function name="oclX:foo" as="item()*" >
    <xsl:param name="a" as="item()*" />
    <x1>1</x1>    
    <xsl:copy-of select="$a"/>
    <x2>2</x2>
  </xsl:function>  -->

  <!--
  <xsl:stream href="employees.xml">
    <xsl:iterate select="*/employee">
      <xsl:param name="highest-earners" 
        as="map(xs:string, element(employee))" 
        select="map:new()"/>
      <xsl:variable name="this" select="copy-of(.)" as="element(employee)"/> 
      <xsl:next-iteration>
        <xsl:with-param name="highest-earners"
          select="let $existing := $highest-earners($this/department)
          return if ($existing/salary gt $this/salary)
          then $highest-earners
          else map:new($highest-earners, map:entry($this/department, $this))"/>
      </xsl:next-iteration>
      <xsl:on-completion>
        <xsl:for-each select="map:keys($highest-earners)">
          <department name="{.}">
            <xsl:copy-of select="$highest-earners(.)"/>
          </department>
        </xsl:for-each>
      </xsl:on-completion>
    </xsl:iterate>
  </xsl:stream>
  -->

  <!--
  <saxon:import-query 
    href="play.xq" 
    namespace="http://eXolutio.com/oclX/dynamic" 
  />-->
  <!--
  <xsl:function name="oclX:forAllPrev" as="xs:boolean">
    <!-\-$collection as item()*,-\->
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean"/>
    <xsl:param name="variables" as="item()*"/>
    
    <xsl:sequence select="every $it in $collection satisfies $body($it) "/>
  </xsl:function>
  
  <xsl:function name="oclX:forAllM" as="xs:boolean">
    <!-\-$collection as item()*,-\->
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVariable" as="xs:string"/>
    <xsl:param name="body" as="function(item(), map(*)) as xs:boolean"/>
    <xsl:param name="v" as="map(*)"/>
    
    <xsl:message><xsl:copy-of select="'in 1'"/></xsl:message>
    <xsl:message><xsl:copy-of select="$collection"/></xsl:message>
    <xsl:message><xsl:copy-of select="map:keys($v)"/></xsl:message>
    <xsl:message>x: <xsl:value-of select="$v('x')"/></xsl:message>
    <xsl:message>y: <xsl:value-of select="$v('y')"/></xsl:message>
    
    <xsl:sequence select="
      every $it in $collection 
      satisfies $body($it, map:new(($v, map {$iterationVariable := $it}))) "/>
    <xsl:message><xsl:copy-of select="'out 1'"/></xsl:message>
  </xsl:function>
  
  <xsl:function name="oclX:forAllM2" as="xs:boolean">
    <!-\-$collection as item()*,-\->
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVariable" as="xs:string"/>
    <xsl:param name="body" as="function(item(), map(*)) as xs:boolean"/>
    <xsl:param name="v" as="map(*)"/>
    
    <xsl:message><xsl:copy-of select="'in 2'"/></xsl:message>
    <xsl:message><xsl:copy-of select="$collection"/></xsl:message>
    <xsl:message><xsl:copy-of select="map:keys($v)"/></xsl:message>
    <xsl:message>x: <xsl:value-of select="$v('x')"/></xsl:message>
    <xsl:message>y: <xsl:value-of select="$v('y')"/></xsl:message>
    
    <xsl:sequence select="
      every $it in $collection 
      satisfies $body($it, map:new(($v, map {$iterationVariable := $it}))) "/>
    <xsl:message><xsl:copy-of select="'out 2'"/></xsl:message>
  </xsl:function>
  -->
  <!--<xsl:function name="oclX:forAll" as="xs:boolean">
    <!-\-$collection as item()*,-\->
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(*)"/>
    
    <xsl:variable name="iteratorCount" select="function-arity($body)" as="xs:integer" />
       
    <xsl:iterate select="1 to xs:integer(math:pow(count($collection), $iteratorCount))">
      <!-\- expect true -\-> 
      <xsl:param name="satisfied" as="xs:boolean" select="true()" />
      
      <xsl:message>Iteration: <xsl:value-of select="."/></xsl:message>
      <xsl:variable name="indices" select="
        oclXin:getIndices(xs:integer(.) - 1, count($collection), $iteratorCount, ())"/>
      <xsl:message>Indices: <xsl:value-of select="$indices"/></xsl:message>
      
      <xsl:variable name="forThis" select="
         if ($iteratorCount eq 1) then $body($collection[$indices[1] + 1])
         else if ($iteratorCount eq 2) then $body($collection[$indices[1] + 1], $collection[$indices[2] + 1])
         else if ($iteratorCount eq 3) then $body($collection[$indices[1] + 1], $collection[$indices[3] + 1])
         else if ($iteratorCount eq 4) then $body($collection[$indices[1] + 1], $collection[$indices[4] + 1])
         else if ($iteratorCount eq 5) then $body($collection[$indices[1] + 1], $collection[$indices[5] + 1])
         else if ($iteratorCount eq 6) then $body($collection[$indices[1] + 1], $collection[$indices[6] + 1])
         else if ($iteratorCount eq 7) then $body($collection[$indices[1] + 1], $collection[$indices[7] + 1])
         else if ($iteratorCount eq 8) then $body($collection[$indices[1] + 1], $collection[$indices[8] + 1])
         else if ($iteratorCount eq 9) then $body($collection[$indices[1] + 1], $collection[$indices[9] + 1])
         else false()"
        as="xs:boolean" />
      
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
  </xsl:function>-->
  <!--
    
  <xsl:function name="oclX:forAll1" as="xs:boolean">
    <!-\-$collection as item()*,-\->
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="body" as="function(item()) as xs:boolean"/>
    <xsl:sequence select="every $it in $collection satisfies $body($it) "/>
  </xsl:function>
-->
</xsl:stylesheet>
