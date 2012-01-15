<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
    exclude-result-prefixes="xd oclX dyn saxon"
    xmlns:oclX="oclXasdfasdf"
    xmlns:dyn="http://exslt.org/dynamic"
    xmlns:saxon="http://saxon.sf.net/"    
    xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    version="2.0">
    <xd:doc scope="stylesheet">
        <xd:desc>
            <xd:p><xd:b>Created on:</xd:b> Apr 7, 2011</xd:p>
            <xd:p><xd:b>Author:</xd:b> Jakub</xd:p>
            <xd:p></xd:p>
        </xd:desc>
    </xd:doc>
    
    <!--
        self.department.forAll(d | d.share = 
        d.salaryExpenses / totalExpenses)
    -->
    
    <xsl:template match="/departments">
        <xsl:variable name="totalExpenses">
            <xsl:value-of select="sum(department/salaryExpenses)"/>
        </xsl:variable>     
        
        
        <xsl:variable name="variables1" as="item()*">
            <self><xsl:copy-of select="."/></self>                
            <e><xsl:copy-of select="//employee[1]"/></e>
        </xsl:variable>
    
        == obsah variable == 
        <xsl:copy-of select="$variables1" />

        == test navigace ==
        <xsl:copy-of select="$variables1[2]/*[1]"/>
        --parent:  
        <xsl:copy-of select="$variables1[2]/*[1]/.."/>

        -- set of employees -- 
        
        <xsl:copy-of select="oclX:collect(department[1]/employees/employee, '$p3/name', $variables1)" />        
        
        
        <!--<xsl:value-of select="if (oclX:set-equal($partCodes, $vnejsiCollectf)) then 'YES' else 'NO'" />-->
        
        ForAll1 :
        <xsl:value-of select="oclX:forAll(department, 
            '$p2/employeeCount > 0', 
            $variables1)"/>      
        
        Sum via iterate: 
        <!-- sum direct iterate --> 
        <xsl:copy-of select="oclX:iterate(
            department[1]/employees/employee,
            '0',
            '$p2 + $p3/salary',
            $variables1
            )" />
        
        Sum:
        <xsl:copy-of select="oclX:sum(department[1]/employees/employee/salary, $variables1)"/>
        
        --- 
        
        ForAll2 : 
        <xsl:value-of select="oclX:forAll(department, 
            'number($p2/share) eq $p2/salaryExpenses div sum($p1/department/salaryExpenses)', 
            $variables1)"/> -->
    </xsl:template>
    
    <xsl:function name="oclX:collect" as="item()*">
        <xsl:param name="collection" />
        <xsl:param name="expression" />
        <xsl:param name="variables" as="item()*" />
        
        <xsl:copy-of select="oclX:iterate($collection, '()', '$p2 union $p3', $variables)"/>
    </xsl:function>
    
    <!-- sum implemented using iterate --> 
    <xsl:function name="oclX:sum">
        <xsl:param name="collection" />
        <xsl:param name="variables" as="item()*" />
        
        <xsl:copy-of select="oclX:iterate($collection, '0', '$p2 + $p3', $variables)"/>
    </xsl:function>
    
    
    <xsl:function name="oclX:iterate">
        <xsl:param name="collection" as="item()*" />
        <xsl:param name="accumulatorInitExpresson" as="xs:string" />        
        <xsl:param name="expression" as="xs:string" />
        <xsl:param name="variables" as="item()*" />
        
        <xsl:variable name="accumulatorInitialValue">
            <xsl:copy-of select="saxon:evaluate($accumulatorInitExpresson)" />
        </xsl:variable>            
        
        <xsl:copy-of select="oclX:iterate-rec($collection, 
            $accumulatorInitialValue, $expression, 1, count($collection), $variables)" />   
    </xsl:function>
    
    <xsl:function name="oclX:iterate-rec">
        <xsl:param name="collection" as="item()*"/>
        <xsl:param name="accumulator" as="item()*" />	        
        <xsl:param name="expression" as="xs:string" />
        <xsl:param name="iteration"  as="xs:integer" />
        <xsl:param name="totalIterations" as="xs:integer"/>
        <xsl:param name="variables" as="item()*" />
        
        <xsl:choose>            
            <xsl:when test="$iteration = $totalIterations + 1">
                <xsl:copy-of select="$accumulator"/>
            </xsl:when>            
            <xsl:otherwise>
                <xsl:variable name="newAccumulator" as="item()*">
                    <xsl:copy-of select="saxon:evaluate($expression, $variables[1]/*[1], $accumulator, $collection[$iteration])" />                     
                </xsl:variable>
                <xsl:copy-of select="oclX:iterate-rec($collection, $newAccumulator, $expression, $iteration +1, $totalIterations, $variables)"/>                                
            </xsl:otherwise>            
        </xsl:choose>   
    </xsl:function>
    
    <xsl:function name="oclX:forAll">
        <xsl:param name="collection" />
        <!--<xsl:param name="iterationVar" />-->
        <xsl:param name="expression" />
        <xsl:param name="variables" as="item()*" />
        
        <xsl:value-of select="oclX:forAll-rec($collection, 
            $expression, 1, count($collection), $variables)" />        
    </xsl:function>
    
    <xsl:function name="oclX:forAll-rec">
        <xsl:param name="collection" />
        <!--<xsl:param name="iterationVar"/>-->
        <xsl:param name="expression" />
        <xsl:param name="iteration" />
        <xsl:param name="totalIterations" />
        <xsl:param name="variables" as="item()*" />
        
        <xsl:choose>            
            <xsl:when test="$iteration = count($collection) + 1">
                <xsl:value-of select="true()"/>
            </xsl:when>            
            <xsl:otherwise>
                <xsl:variable name="forThis" as="xs:boolean">
                    <xsl:value-of select="saxon:evaluate($expression, $variables[1]/*[1], $collection[$iteration])" />                     
                </xsl:variable>
                <xsl:choose>
                    <xsl:when test="$forThis">
                        <xsl:value-of select="oclX:forAll-rec($collection, $expression, $iteration +1, $totalIterations, $variables)"/>        
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select="false()"/>                        
                    </xsl:otherwise>
                </xsl:choose>                
            </xsl:otherwise>            
        </xsl:choose>       
    </xsl:function>
</xsl:stylesheet>