<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
<!ENTITY vardesc "Values of variables defined outside the expression 
(variable self and iteration variables from containing expressions). 
Required for proper OclX function. 
Use <doc:i>oclX:vars</doc:i> in your custom templates for outermost calls. 
In Schematron schemas, use <doc:i>$variables</doc:i> (the value is assigned internally). 
In not-outermost calls, always use <doc:i>$variables</doc:i>. See 
documentation of <doc:i>oclX:evaluate</doc:i> for more information.">
]>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:oclX="http://eXolutio.com/oclX/dynamic"
  xmlns:oclXin="http://eXolutio.com/oclX/dynamic/internal" 
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:oclDate="http://eXolutio.com/oclX/types/date"
  xmlns:doc="http://www.oxygenxml.com/ns/doc/xsl"
  exclude-result-prefixes="oclX saxon doc"
  version="2.0">

  <xsl:import href="oclX-dynamic-internal.xsl"/>

  <doc:doc scope="stylesheet">
    <doc:desc>
      <doc:p><doc:b>Created on:</doc:b> Jan 1, 2012</doc:p>
      <doc:p><doc:b>Author:</doc:b> Jakub Maly</doc:p>
      <doc:p><doc:b>email:</doc:b> jakub@maly.cz</doc:p>

      <doc:p>
        <doc:b>OclX function library</doc:b> contains definition of functions, which enable the use of
        OCL iterator expressions in XPath expressions. The library can be also used in Schematron
        schemas, providing that the Schematron validation process is based on XSLT and uses modified
        Schematron pipeline XSLTs, which add imports of OclX library. </doc:p>
      <doc:p>Go to http://exolutio.com and download eXolutio - a tool which allows translation
        from OCL scripts to Schematron schemas. You can also find the modified Schematron pipeline
        XSLTs on this address. </doc:p>
      <doc:p>The library uses SAXON extensions for dynamic evaluation of expressions. (See function
          <doc:i>oclX:evaluate</doc:i> for more information). </doc:p>
      <doc:p>We strongly advise to use <doc:b>>OclX-functional</doc:b> instead of OclX-dynamic (this library). 
        OclX-functional contains definitions of the majority of functions defined in the 
      specification of OCL. OclX-dynamic focuses almost solely on iterator expressions.</doc:p>
      <doc:p>Please, be aware that at this moment, OclX does not support nested collections. </doc:p>
    </doc:desc>
  </doc:doc>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements the fundamental OCL iterator expression <doc:i>iterate</doc:i>. </doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar"><doc:i>Name</doc:i> of the iteration variable, the iteration
      variable can be referenced from <doc:i>body</doc:i>.</doc:param>
    <doc:param name="accumulatorVar"><doc:i>Name</doc:i> of the accumulator variable, the iteration
      variable can be referenced from <doc:i>body</doc:i>.</doc:param>
    <doc:param name="accumulatorInitExpresson">This expression is evaluated and the result assigned
      to <doc:i>acc</doc:i> in the first iteration.</doc:param>
    <doc:param name="body">expression evaluated for each member of <doc:i>collection</doc:i></doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return> Returns the value of accumulator variable after the last iteration.</doc:return>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>This function is called recursively from <doc:i>oclX:iterate</doc:i> and is not meant for
        direct usage. </doc:p>
    </doc:desc>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>Returns true, when <doc:i>body</doc:i> expression evaluates to true, false otherwise.</doc:p>
    </doc:desc>
    <doc:param name="body">evaluated expression</doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return/>
  </doc:doc>
  <xsl:function name="oclX:holds" as="xs:boolean">
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>

    <xsl:sequence select="oclXin:evaluate($body, $variables)"/>
  </xsl:function>

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>forAll</doc:i> function from OCL. Checks, whether a condition holds for
        all member of a collection.</doc:p>
      <doc:p>Results in <doc:i>true</doc:i> if the body expression evaluates to true for each element in the source <doc:i>collection</doc:i>; otherwise, result is <doc:i>false</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
        <doc:i>collection</doc:i>. </doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>Returns true, if all members of <doc:i>collection</doc:i> satisfy the condition
      defined by <doc:i>expression</doc:i>. </doc:return>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>This function is called recursively from <doc:i>oclX:forAll</doc:i> and is not meant for
        direct usage. </doc:p>
    </doc:desc>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>forAll</doc:i> function from OCL. Unlike <doc:i>oclX:forAll</doc:i>,
          <doc:i>oclX:forAllN</doc:i> allows iteration using multiple iteration variables. </doc:p>
      <doc:p>Results in true if the body expression evaluates to true for each element in the source collection; otherwise, result is false.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVars">The <doc:i>names</doc:i> of the iteration variables, comma
      separated.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
        <doc:i>collection</doc:i>. </doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>Returns true, if all members of <doc:i>collection</doc:i> satisfy the condition
      defined by <doc:i>expression</doc:i>. </doc:return>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>This function is called recursively from <doc:i>oclX:forAllN</doc:i> and is not meant for
        direct usage. </doc:p>
    </doc:desc>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>Computes transitive closure over a collection using an expression. </doc:p>
      <doc:p>The closure of applying body transitively to every element of the source collection</doc:p>
    </doc:desc>
    <doc:param name="collection">The initial collection</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">The expression is evaluated for each member of the initial
      collection and for all other members that are added be the previous calls. It represents one
      step of computation of transitive closure.</doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>The sequence of items collected during the consecutive evaluations of
        <doc:i>expression</doc:i> over the initial collection and over the items visited by the
      computation during computing transitive closure.</doc:return>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>This function is called recursively from <doc:i>oclX:closure</doc:i> and is not meant for
        direct usage. </doc:p>
    </doc:desc>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>collect</doc:i> function from OCL. The implementation uses
          <doc:i>oclX:iterate</doc:i> internally. From a sequence and a mapping expressoin returns a
        mapped sequence. </doc:p>
      <doc:p>
        The sequence of elements that results from applying body to every member of the source sequence.        
      </doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">Expression evaluated for each member of <doc:i>collection</doc:i>.
      The result of each call is added to the result sequence. </doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>The sequence of items collected during the consecutive evaluations of
        <doc:i>expression</doc:i>.</doc:return>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>exists</doc:i> function from OCL. The implementation uses
          <doc:i>oclX:iterate</doc:i> internally. Checks, whether a condition holds for some member of
        a collection.</doc:p>
      <doc:p>Results in true if body evaluates to true for at least one element in the source collection.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">Condition expression evaluated for each member of
        <doc:i>collection</doc:i>. </doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>Returns true, if some member of <doc:i>collection</doc:i> satisfies the condition
      defined by <doc:i>expression</doc:i>. </doc:return>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>isUnique</doc:i> function from OCL. The implementation uses
        <doc:i>oclX:iterate</doc:i> internally. </doc:p>
      <doc:p>Results in true if body evaluates to a different value for each element in the source collection; otherwise, result is false.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">Mapping expression evaluated for each member of <doc:i>collection</doc:i>.</doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return></doc:return>
  </doc:doc>
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
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>any</doc:i> function from OCL. The implementation uses
        <doc:i>oclX:iterate</doc:i> internally. </doc:p>
      <doc:p>Returns any element in the source collection for which <doc:i>body</doc:i> evaluates to <doc:i>true</doc:i>. 
        If there is more than one element for which
        <doc:i>body</doc:i> is <doc:i>true</doc:i>, one of them is returned. 
        There must be at least one element fulfilling <doc:i>body</doc:i>, otherwise the result is an empty sequence.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">Condition evaluated for each member of <doc:i>collection</doc:i>.</doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>A member of <doc:i>collection</doc:i> satisfying <doc:i>body</doc:i>, if such an element exists, an
      empty sequence otherwise. </doc:return>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>one</doc:i> function from OCL. The implementation uses
        <doc:i>oclX:iterate</doc:i> internally. </doc:p>
      <doc:p>Results in <doc:i>true</doc:i> if there is exactly one element in the source 
        <doc:i>collection</doc:i> for which <doc:i>body</doc:i> is <doc:i>true</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">Condition evaluated for each member of <doc:i>collection</doc:i>.</doc:param>
    <doc:param name="variables"></doc:param>    
  </doc:doc>
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
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>select</doc:i> function from OCL. The implementation uses
        <doc:i>oclX:iterate</doc:i> internally. </doc:p>
      <doc:p>Returns a sequence of items from <doc:i>collection</doc:i> for which which <doc:i>body</doc:i> is <doc:i>true</doc:i>.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">Condition evaluated for each member of <doc:i>collection</doc:i>.</doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>A member of <doc:i>collection</doc:i> satisfying <doc:i>body</doc:i>, if such an element exists, an
      empty sequence otherwise. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:select" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>   
    
      <xsl:sequence
        select="oclX:iterate(
        $collection, $iterationVar, 'acc', '()',            
        concat('$acc, if ((', $body, ') eq true()) then ($', $iterationVar, ') else ()'),               
        $variables)"
      />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Implements <doc:i>reject</doc:i> function from OCL. The implementation uses
        <doc:i>oclX:iterate</doc:i> internally. </doc:p>
      <doc:p>The subsequence of the source <doc:i>collection</doc:i> for which <doc:i>body</doc:i> is
        false.</doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="iterationVar">The <doc:i>name</doc:i> of the iteration variable.</doc:param>
    <doc:param name="body">The expression evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
    <doc:param name="variables"></doc:param>
    <doc:return>The subsequence of the source <doc:i>collection</doc:i> for which <doc:i>body</doc:i> is
      false.</doc:return>
  </doc:doc>
  <xsl:function name="oclX:reject" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
    
      <xsl:sequence
        select="oclX:iterate(
        $collection, $iterationVar, 'acc', '()',            
        concat('$acc, if ((', $body, ') eq false()) then ($', $iterationVar, ') else ()'),               
        $variables)"
      />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Results in the collection containing all elements of the source
        <doc:i>collection</doc:i>. The element for which <doc:i>body</doc:i> has the lowest value
        comes first, and so on. Uses <doc:i>xsl:perform-sort</doc:i> and dynamic evaluation
        internally. </doc:p>
    </doc:desc>
    <doc:param name="collection">The iterated collection.</doc:param>
    <doc:param name="body">Condition evaluated for each member of
      <doc:i>collection</doc:i>.</doc:param>
    <doc:return>Ordered sequence. </doc:return>
  </doc:doc>
  <xsl:function name="oclX:sortedBy" as="item()*">
    <xsl:param name="collection" as="item()*"/>
    <xsl:param name="iterationVar" as="xs:string"/>
    <xsl:param name="body" as="xs:string"/>
    <xsl:param name="variables" as="item()*"/>
    
    <xsl:perform-sort select="$collection">
      <xsl:sort select="oclXin:evaluate($body, oclXin:appendVarToSequence($variables, $iterationVar, current()))"/>
    </xsl:perform-sort>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Returns <doc:i>true</doc:i> when <doc:i>date2</doc:i> follows after
        <doc:i>date1</doc:i> (also when the dates are equal). </doc:p>
    </doc:desc>
    <doc:param name="date1">First date. </doc:param>
    <doc:param name="date2">Second date. </doc:param>
  </doc:doc>
  <xsl:function name="oclDate:after" as="xs:boolean">
    <xsl:param name="date1" />
    <xsl:param name="date2" />    
    <xsl:sequence select="xs:dateTime($date1) ge xs:dateTime($date2)"/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>Returns <doc:i>true</doc:i> when <doc:i>date1</doc:i> follows after
      <doc:i>date2</doc:i> (also when the dates are equal). </doc:desc>
    <doc:param name="date1">First date. </doc:param>
    <doc:param name="date2">Second date. </doc:param>
  </doc:doc>
  <xsl:function name="oclDate:before" as="xs:boolean">
    <xsl:param name="date1" />
    <xsl:param name="date2" />    
    <xsl:sequence select="xs:dateTime($date2) ge xs:dateTime($date1)"/>
  </xsl:function>
  
  <doc:doc>
    <doc:desc>Returns date from a dateTime.</doc:desc>
    <doc:param name="dateTime">Input date. </doc:param>
  </doc:doc>
  <xsl:function name="oclDate:getDate" as="xs:date">
    <xsl:param name="dateTime" as="xs:dateTime" />
    <xsl:sequence select="xs:date(format-dateTime($dateTime, '[Y]-[M,2]-[D,2]'))" />
  </xsl:function>
  
  <doc:doc>
    <doc:desc>
      <doc:p>Dynamically evaluates an expression passed as string. The expression can contain
        variables defined in <doc:i>variables</doc:i>. The function is used internally by many
        functions in OclX library, but is not meant to be used directly (although it is possible). </doc:p>
      <doc:p>The dynamic evaluation uses extension function defined by SAXON. When using other
        processor (which allows dynamic evaluation, e.g. a XSLT 3.0 compliant processor), redefine
        this function. </doc:p>
    </doc:desc>
    <doc:param name="expression">The expression to evaluate. C</doc:param>
    <doc:param name="variables">Contains variables used in the expression. The value must be a
      sequence of items, where tag names match the names of variables followed by values of the
      variables. Values, which are sequences, are written as trees (with root SEQ and chilren
      ITEMs). The first node in the sequence must be &lt;variables/&gt;, the second member of the
      sequence must be a filling node &lt;dummy/&gt;. Empty values are represented using the string
      '##EMPTY'. An example of definition of three variables <doc:i>a</doc:i>, <doc:i>b</doc:i>,
        <doc:i>c</doc:i>, where value of <doc:i>a</doc:i> is 'string', value of <doc:i>b</doc:i> is empty
      sequence and value of <doc:i>c</doc:i> is a sequence ('1', '2', '3') would be: <![CDATA[
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
    </doc:param>
    <doc:return/>
  </doc:doc>
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

  <doc:doc>
    <doc:desc>
      <doc:p>This function creates a sequence required by <doc:i>variables</doc:i> parameter of other
        function. See the documentation of <doc:i>oclX:evaluate</doc:i></doc:p>. 
      <doc:p>The context variable is named <doc:i>self</doc:i></doc:p>
    </doc:desc>
    <doc:param name="current-node">Context node must be passed as a value for this parameter.
    </doc:param>
  </doc:doc>
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
  
  <doc:doc>
    <doc:desc>
      <doc:p>This function creates a sequence required by <doc:i>variables</doc:i> parameter of other
        function. See the documentation of <doc:i>oclX:evaluate</doc:i></doc:p>.      
    </doc:desc>
    <doc:param name="current-node">Context node must be passed as a value for this parameter.</doc:param>
    <doc:param name="context-var-name">Name of the context variable.</doc:param>
  </doc:doc>
  <xsl:function name="oclX:vars" as="item()*">
    <xsl:param name="current-node" as="item()"/>
    <xsl:param name="context-var-name" as="xs:string" />
    <variables/>
    <!-- just a placeholder -->
    <dummy/>
    <!-- name of the context variable -->
    <xsl:element name="{$context-var-name}" />
    <!-- value of the $self variable-->
    <xsl:sequence select="$current-node"/>
  </xsl:function>
  
</xsl:stylesheet>
