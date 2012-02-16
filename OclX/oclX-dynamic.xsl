<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
<!ENTITY vardesc "Values of variables defined outside the expression 
(variable self and iteration variables from containing expressions). 
Required for proper OclX function. 
Use <xd:i>oclX:vars</xd:i> in your custom templates for outermost calls. 
In Schematron schemas, use <xd:i>$variables</xd:i> (the value is assigned internally). 
In not-outermost calls, always use <xd:i>$variables</xd:i>. See 
documentation of <xd:i>oclX:evaluate</xd:i> for more information.">
]>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:oclX="http://eXolutio.com/oclX/dynamic"
  xmlns:oclXin="http://eXolutio.com/oclX/dynamic/internal" 
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:oclDate="http://eXolutio.com/oclX/types/date"
  xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
  exclude-result-prefixes="oclX saxon xd"
  version="2.0">

  <xsl:import href="types/date.xsl"/>
  <xsl:import href="oclX-dynamic-internal.xsl"/>

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
      <xd:p>Go to http://exolutio.com and download eXolutio - a tool which allows translation
        from OCL scripts to Schematron schemas. You can also find the modified Schematron pipeline
        XSLTs on this address. </xd:p>
      <xd:p> The library uses SAXON extensions for dynamic evaluation of expressions. (See function
          <xd:i>oclX:evaluate</xd:i> for more information). </xd:p>
    </xd:desc>
  </xd:doc>

  <xd:doc>
    <xd:desc>
      <xd:p>Implements the fundamental OCL iterator expression <xd:i>iterate</xd:i>. </xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVar"><xd:i>Name</xd:i> of the iteration variable, the iteration
      variable can be referenced from <xd:i>body</xd:i>.</xd:param>
    <xd:param name="accumulatorVar"><xd:i>Name</xd:i> of the accumulator variable, the iteration
      variable can be referenced from <xd:i>body</xd:i>.</xd:param>
    <xd:param name="accumulatorInitExpresson">This expression is evaluated and the result assigned
      to <xd:i>acc</xd:i> in the first iteration.</xd:param>
    <xd:param name="body">expression evaluated for each member of <xd:i>collection</xd:i></xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return> Returns the value of accumulator variable after the last iteration.</xd:return>
  </xd:doc>
  <xsl:function name="oclX:iterate" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="accumulatorVar" as="xs:string"/>
    <xsl:param name="accumulatorInitExpresson" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
      
    <xsl:variable name="accumulatorInitialValue" as="item()*">
      <xsl:sequence select="oclXin:evaluate($accumulatorInitExpresson, $variables)"/>
    </xsl:variable>

    <xsl:sequence
      select="oclXin:iterate-rec($collection, $iterationVar, $accumulatorVar,
      $accumulatorInitialValue, $body, 1, count($collection), $variables)"
    />
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>This function is called recursively from <xd:i>oclX:iterate</xd:i> and is not meant for
        direct usage. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:function name="oclXin:iterate-rec" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="accumulatorVar" as="xs:string"/>
    <xsl:param name="accumulator" as="item()*"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="iteration" as="xs:integer"/>
    <xsl:param name="totalIterations" as="xs:integer"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:choose>
      <xsl:when test="$iteration = $totalIterations + 1">
        <!-- return accumulated value -->
        <xsl:sequence select="$accumulator"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="variablesWithIteration" as="item()*">
          <xsl:sequence
            select="oclXin:appendVarToSequence($variables, $iterationVar, $collection[$iteration])"
          />
        </xsl:variable>
        <xsl:variable name="variablesWithAccumulator" as="item()*">
          <xsl:sequence
            select="oclXin:appendVarToSequence($variablesWithIteration, $accumulatorVar, $accumulator)"
          />
        </xsl:variable>
        <!-- compute new accumulator value -->
        <xsl:variable name="newAccumulator" as="item()*">
          <xsl:sequence select="oclXin:evaluate($body, $variablesWithAccumulator)"/>
        </xsl:variable>
        <!-- call recursively -->
        <xsl:sequence
          select="oclXin:iterate-rec($collection, $iterationVar, $accumulatorVar, $newAccumulator, $body, $iteration +1, $totalIterations, $variables)"
        />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>Returns true, when <xd:i>body</xd:i> expression evaluates to true, false otherwise.</xd:p>
    </xd:desc>
    <xd:param name="body">evaluated expression</xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return/>
  </xd:doc>
  <xsl:function name="oclX:holds" as="xs:boolean">
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:sequence select="oclXin:evaluate($body, $variables)"/>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>Implements <xd:i>forAll</xd:i> function from OCL. Checks, whether a condition holds for
        all member of a collection.</xd:p>
      <xd:p>Results in <xd:i>true</xd:i> if the body expression evaluates to true for each element in the source <xd:i>collection</xd:i>; otherwise, result is <xd:i>false</xd:i>.</xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</xd:param>
    <xd:param name="body">Condition expression evaluated for each member of
        <xd:i>collection</xd:i>. </xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return>Returns true, if all members of <xd:i>collection</xd:i> satisfy the condition
      defined by <xd:i>expression</xd:i>. </xd:return>
  </xd:doc>
  <xsl:function name="oclX:forAll" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:sequence
      select="oclXin:forAll-rec($collection, $iterationVar,
      $body, 1, count($collection), $variables)"
    />
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>This function is called recursively from <xd:i>oclX:forAll</xd:i> and is not meant for
        direct usage. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:function name="oclXin:forAll-rec" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="iteration" as="xs:integer"/>
    <xsl:param name="totalIterations" as="xs:integer"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:choose>
      <xsl:when test="$iteration = count($collection) + 1">
        <xsl:sequence select="true()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="variablesForThisIteration" as="item()*"
          select="oclXin:appendVarToSequence($variables, $iterationVar, $collection[$iteration])"/>
        <xsl:variable name="forThis" as="xs:boolean">
          <xsl:sequence select="oclXin:evaluate($body, $variablesForThisIteration)"/>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="$forThis">
            <xsl:sequence
              select="oclXin:forAll-rec($collection, $iterationVar, $body, $iteration +1, $totalIterations, $variables)"
            />
          </xsl:when>
          <xsl:otherwise>
            <xsl:sequence select="false()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>Implements <xd:i>forAll</xd:i> function from OCL. Unlike <xd:i>oclX:forAll</xd:i>,
          <xd:i>oclX:forAllN</xd:i> allows iteration using multiple iteration variables. </xd:p>
      <xd:p>Results in true if the body expression evaluates to true for each element in the source collection; otherwise, result is false.</xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVars">The <xd:i>names</xd:i> of the iteration variables, comma
      separated.</xd:param>
    <xd:param name="body">Condition expression evaluated for each member of
        <xd:i>collection</xd:i>. </xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return>Returns true, if all members of <xd:i>collection</xd:i> satisfy the condition
      defined by <xd:i>expression</xd:i>. </xd:return>
  </xd:doc>
  <xsl:function name="oclX:forAllN" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVars" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:variable name="iterationVarsSplitted" select="tokenize($iterationVars, '\s*[,;]\s*')"/>
    <xsl:sequence
      select="oclXin:forAllN-rec($collection, $iterationVarsSplitted,      
      $body, 1, oclXin:power(count($collection), count($iterationVarsSplitted)), $variables)"
    />
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>This function is called recursively from <xd:i>oclX:forAllN</xd:i> and is not meant for
        direct usage. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:function name="oclXin:forAllN-rec" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVars" as="xs:string*"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="iterationE" as="xs:integer"/>
    <xsl:param name="totalIterations" as="xs:integer"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:choose>
      <xsl:when test="$iterationE = $totalIterations + 1">
        <xsl:sequence select="true()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="indices" as="xs:integer*"
          select="oclXin:getIndices($iterationE - 1, count($collection), count($iterationVars), ())"/>
        <!-- ted mam v indices treba (2, 3) -->
        <xsl:variable name="variablesForThisIteration" as="item()*"
          select="oclXin:appendVarToSequenceN($variables, $iterationVars, $collection, $indices)"/>
        <xsl:variable name="forThis" as="xs:boolean">
          <xsl:sequence select="oclXin:evaluate($body, $variablesForThisIteration)"/>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="$forThis">
            <xsl:sequence
              select="oclXin:forAllN-rec($collection, $iterationVars, $body, $iterationE +1, $totalIterations, $variables)"
            />
          </xsl:when>
          <xsl:otherwise>            
            <xsl:sequence select="false()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>Computes transitive closure over a collection using an expression. </xd:p>
      <xd:p>The closure of applying body transitively to every element of the source collection</xd:p>
    </xd:desc>
    <xd:param name="collection">The initial collection</xd:param>
    <xd:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</xd:param>
    <xd:param name="body">The expression is evaluated for each member of the initial
      collection and for all other members that are added be the previous calls. It represents one
      step of computation of transitive closure.</xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return>The sequence of items collected during the consecutive evaluations of
        <xd:i>expression</xd:i> over the initial collection and over the items visited by the
      computation during computing transitive closure.</xd:return>
  </xd:doc>
  <xsl:function name="oclX:closure" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:sequence
      select="oclXin:closure-rec(reverse($collection), (), 
      $iterationVar, $body, $variables)"
    />
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>This function is called recursively from <xd:i>oclX:closure</xd:i> and is not meant for
        direct usage. </xd:p>
    </xd:desc>
  </xd:doc>
  <xsl:function name="oclXin:closure-rec" as="item()*">
    <xsl:param name="toDoStack" as="item()*"/>
    <xsl:param name="result" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:choose>
      <xsl:when test="count($toDoStack) eq 0">
        <xsl:sequence select="$result"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="i" select="$toDoStack[last()]" as="item()"/>
        <xsl:variable name="variablesForThisIteration" as="item()*">
          <xsl:sequence select="oclXin:appendVarToSequence($variables, $iterationVar, $i)"/>
        </xsl:variable>
        <xsl:variable name="contribution"
          select="oclXin:evaluate($body, $variablesForThisIteration)" as="item()*"/>
        <xsl:sequence
          select="oclXin:closure-rec(
          ($toDoStack[position() lt last()], reverse($contribution)), 
          ($result, $i), 
          $iterationVar, $body, $variables) "
        />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>Implements <xd:i>collect</xd:i> function from OCL. The implementation uses
          <xd:i>oclX:iterate</xd:i> internally. From a sequence and a mapping expressoin returns a
        mapped sequence. </xd:p>
      <xd:p>
        The sequence of elements that results from applying body to every member of the source sequence.        
      </xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</xd:param>
    <xd:param name="body">Expression evaluated for each member of <xd:i>collection</xd:i>.
      The result of each call is added to the result sequence. </xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return>The sequence of items collected during the consecutive evaluations of
        <xd:i>expression</xd:i>.</xd:return>
  </xd:doc>
  <xsl:function name="oclX:collect" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:variable name="result" as="item()*">
      <xsl:sequence
        select="oclX:iterate($collection, $iterationVar, 'acc',  '()', concat('$acc , ', $body), $variables)"
      />
    </xsl:variable>
    <xsl:sequence select="$result"/>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>Implements <xd:i>exists</xd:i> function from OCL. The implementation uses
          <xd:i>oclX:iterate</xd:i> internally. Checks, whether a condition holds for some member of
        a collection.</xd:p>
      <xd:p>Results in true if body evaluates to true for at least one element in the source collection.</xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</xd:param>
    <xd:param name="body">Condition expression evaluated for each member of
        <xd:i>collection</xd:i>. </xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return>Returns true, if some member of <xd:i>collection</xd:i> satisfies the condition
      defined by <xd:i>expression</xd:i>. </xd:return>
  </xd:doc>
  <xsl:function name="oclX:exists" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="expression" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:variable name="result" as="item()*">
      <xsl:sequence
        select="oclX:iterate($collection, $iterationVar, 'acc', 'false()', concat('$acc or (', $expression, ')'), $variables)"
      />
    </xsl:variable>
    <xsl:sequence select="$result"/>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>Implements <xd:i>isUnique</xd:i> function from OCL. The implementation uses
        <xd:i>oclX:iterate</xd:i> internally. </xd:p>
      <xd:p>Results in true if body evaluates to a different value for each element in the source collection; otherwise, result is false.</xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</xd:param>
    <xd:param name="body">Mapping expression evaluated for each member of <xd:i>collection</xd:i>.</xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return></xd:return>
  </xd:doc>
  <xsl:function name="oclX:isUnique" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:variable name="uniqueItems" as="item()*">
      <xsl:sequence
        select="oclX:iterate(
        $collection, $iterationVar, 'acc', '()',            
        concat('$acc, if (oclXin:combined-first-index-of($acc, ', $body, ') eq -1) then (',$body,') else ()'),               
        $variables)"
      />
    </xsl:variable>
    <xsl:sequence select="if (count($uniqueItems) eq count($collection)) then true() else false()"/>
  </xsl:function>
  
  <xd:doc>
    <xd:desc>
      <xd:p>Implements <xd:i>any</xd:i> function from OCL. The implementation uses
        <xd:i>oclX:iterate</xd:i> internally. </xd:p>
      <xd:p>Returns any element in the source collection for which <xd:i>body</xd:i> evaluates to <xd:i>true</xd:i>. 
        If there is more than one element for which
        <xd:i>body</xd:i> is <xd:i>true</xd:i>, one of them is returned. 
        There must be at least one element fulfilling <xd:i>body</xd:i>, otherwise the result is an empty sequence.</xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</xd:param>
    <xd:param name="body">Condition evaluated for each member of <xd:i>collection</xd:i>.</xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return>A member of <xd:i>collection</xd:i> satisfying <xd:i>body</xd:i>, if such an element exists, an
      empty sequence otherwise. </xd:return>
  </xd:doc>
  <xsl:function name="oclX:any" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
    
    <xsl:variable name="satisfyingItems" as="item()*">
      <xsl:sequence
        select="oclX:iterate(
        $collection, $iterationVar, 'acc', '()',            
        concat('$acc, if ((', $body, ') eq true()) then ($', $iterationVar, ') else ()'),               
        $variables)"
      />
    </xsl:variable>
    <xsl:sequence select="if (count($satisfyingItems) ge 1) then $satisfyingItems[1] else ()  "/>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>Implements <xd:i>one</xd:i> function from OCL. The implementation uses
        <xd:i>oclX:iterate</xd:i> internally. </xd:p>
      <xd:p>Results in <xd:i>true</xd:i> if there is exactly one element in the source 
        <xd:i>collection</xd:i> for which <xd:i>body</xd:i> is <xd:i>true</xd:i>.</xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</xd:param>
    <xd:param name="body">Condition evaluated for each member of <xd:i>collection</xd:i>.</xd:param>
    <xd:param name="variables"></xd:param>    
  </xd:doc>
  <xsl:function name="oclX:one" as="xs:boolean">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
    
    <xsl:variable name="satisfyingItems" as="item()*">
      <xsl:sequence
        select="oclX:iterate(
        $collection, $iterationVar, 'acc', '()',            
        concat('$acc, if ((', $body, ') eq true()) then ($', $iterationVar, ') else ()'),               
        $variables)"
      />
    </xsl:variable>
    <xsl:sequence select="if (count($satisfyingItems) eq 1) then true() else false()  "/>
  </xsl:function>
  
  <xd:doc>
    <xd:desc>
      <xd:p>Implements <xd:i>select</xd:i> function from OCL. The implementation uses
        <xd:i>oclX:iterate</xd:i> internally. </xd:p>
      <xd:p>Returns a sequence of items from <xd:i>collection</xd:i> for which which <xd:i>body</xd:i> is <xd:i>true</xd:i>.</xd:p>
    </xd:desc>
    <xd:param name="collection">The iterated collection.</xd:param>
    <xd:param name="iterationVar">The <xd:i>name</xd:i> of the iteration variable.</xd:param>
    <xd:param name="body">Condition evaluated for each member of <xd:i>collection</xd:i>.</xd:param>
    <xd:param name="variables"></xd:param>
    <xd:return>A member of <xd:i>collection</xd:i> satisfying <xd:i>body</xd:i>, if such an element exists, an
      empty sequence otherwise. </xd:return>
  </xd:doc>
  <xsl:function name="oclX:select" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
    
    
  </xsl:function>
  
  <xsl:function name="oclX:reject" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
    
    
  </xsl:function>
  
  <xsl:function name="oclX:sortedBy" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
    
    
  </xsl:function>
  
  <xd:doc>
    <xd:desc>
      <xd:p>Dynamically evaluates an expression passed as string. The expression can contain
        variables defined in <xd:i>variables</xd:i>. The function is used internally by many
        functions in OclX library, but is not meant to be used directly (although it is possible). </xd:p>
      <xd:p>The dynamic evaluation uses extension function defined by SAXON. When using other
        processor (which allows dynamic evaluation, e.g. a XSLT 3.0 compliant processor), redefine
        this function. </xd:p>
    </xd:desc>
    <xd:param name="expression">The expression to evaluate. C</xd:param>
    <xd:param name="variables">Contains variables used in the expression. The value must be a
      sequence of items, where tag names match the names of variables followed by values of the
      variables. Values, which are sequences, are written as trees (with root SEQ and chilren
      ITEMs). The first node in the sequence must be &lt;variables/&gt;, the second member of the
      sequence must be a filling node &lt;dummy/&gt;. Empty values are represented using the string
      '##EMPTY'. An example of definition of three variables <xd:i>a</xd:i>, <xd:i>b</xd:i>,
        <xd:i>c</xd:i>, where value of <xd:i>a</xd:i> is 'string', value of <xd:i>b</xd:i> is empty
      sequence and value of <xd:i>c</xd:i> is a sequence ('1', '2', '3') would be: <![CDATA[
      <variables />
      <dummy />
      <a />
      string
      <b />
      ##EMPTY
      <c>
      <SEQ>
        <ITEM>1</ITEM>
        <ITEM>2</ITEM>
        <ITEM>3</ITEM>
      </SEQ>
      ]]>
    </xd:param>
    <xd:return/>
  </xd:doc>
  <xsl:function name="oclXin:evaluate" as="item()*">
    <xsl:param name="expression" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
    <xsl:variable name="expressionReplaced"
      select="oclXin:expressionForSaxon($expression, $variables)" as="xs:string"/>
    <xsl:variable name="result" as="item()*">
      <xsl:choose>
        <xsl:when test="count($variables) div 2 = 0">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables)"/>
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 1">
          <xsl:sequence select="saxon:evaluate($expressionReplaced, $variables)"/>
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 2">
          <xsl:sequence
            select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]))"
          />
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 3">
          <xsl:sequence
            select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]), oclXin:retrieveVarFromSequence($variables[6]))"
          />
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 4">
          <xsl:sequence
            select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]), oclXin:retrieveVarFromSequence($variables[6]), oclXin:retrieveVarFromSequence($variables[8]))"
          />
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 5">
          <xsl:sequence
            select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]), oclXin:retrieveVarFromSequence($variables[6]), oclXin:retrieveVarFromSequence($variables[8]), oclXin:retrieveVarFromSequence($variables[10]))"
          />
        </xsl:when>
        <xsl:when test="count($variables) div 2 = 6">
          <xsl:sequence
            select="saxon:evaluate($expressionReplaced, $variables, oclXin:retrieveVarFromSequence($variables[4]), oclXin:retrieveVarFromSequence($variables[6]), oclXin:retrieveVarFromSequence($variables[8]), oclXin:retrieveVarFromSequence($variables[10]), oclXin:retrieveVarFromSequence($variables[12]))"
          />
        </xsl:when>
        <xsl:otherwise>
          <xsl:message>ERROR IN EVALUATE</xsl:message> ERROR IN EVALUATE </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:sequence select="$result"/>
  </xsl:function>

  <xd:doc>
    <xd:desc>
      <xd:p>This function creates a sequence required by <xd:i>variables</xd:i> parameter of other
        function. See the documentation of <xd:i>oclX:evaluate</xd:i></xd:p>. </xd:desc>
    <xd:param name="current-node">Context node must be passed as a value for this parameter.
    </xd:param>
  </xd:doc>
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
  
</xsl:stylesheet>
