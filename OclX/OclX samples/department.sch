<?xml version="1.0" encoding="utf-8"?>
<sch:schema queryBinding="xslt2" schemaVersion="ISO19757-3" 
	xmlns:sch="http://purl.oclc.org/dsdl/schematron"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	>
	<!-- allow-foreign="true" -->
	<sch:title>Test ISO schematron file. Introduction mode</sch:title>
	<sch:ns prefix="oclX" uri="http://tempuri.org" />
	<sch:pattern>
		<sch:rule context="/departments/department">
			<sch:assert test="number(employeeCount) eq count(employees/employee)">
				Wrong employee count
			</sch:assert>
			<sch:assert test="number(salaryExpenses) eq sum(employees/employee/salary)">
				Wrong salary
			</sch:assert>
			<sch:assert test="oclX:iterate(employees/employee, 0, 1, count(employees/employee/salary)) eq salaryExpenses">
				Wrong salary (oclx) 
			</sch:assert>	
		</sch:rule>
		<!-- budu overovat, ze zadny employee neni ve dvou ruznych oddeleni, tedy constraint -->
		
		<!-- zadny employee neni v jednom oddeleni dvakrat 
			context: department
			inv: self.employee->forAll(e1, e2 | e1 != e2 => e1.name != e2.name)
			coz je zkratka za: 
			inv: self.employee->forAll(e1 | department.employee->forAll(e2 | e1 != e2 => e1.name != e2.name))
		-->

	</sch:pattern>
	
	
	<xsl:function name="oclX:iterate">
		<xsl:param name="collection"/>
		<xsl:param name="accumulator"/>
		<xsl:param name="iteration" />
		<xsl:param name="total-iterations"/>
		
		<!--<xsl:value-of select="sum($collection/salary)" />-->
		<xsl:choose>
			<xsl:when test="$iteration le $total-iterations">
				<xsl:variable name="newAccumulator">
					<xsl:value-of select="$accumulator + $collection[$iteration]/salary"/>
				</xsl:variable>				
				<xsl:value-of select="oclX:iterate($collection, $newAccumulator, $iteration + 1, $total-iterations)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$accumulator"/>
			</xsl:otherwise>
		</xsl:choose>
		<!--<xsl:value-of select="sum($collection/@price)"/>-->
	</xsl:function>
</sch:schema>
