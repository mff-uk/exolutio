<?xml version="1.0" encoding="utf-8"?>
<sch:schema queryBinding="xslt2" schemaVersion="ISO19757-3" 
    xmlns:sch="http://purl.oclc.org/dsdl/schematron"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
    >
    
    <sch:title></sch:title>
    <sch:ns prefix="oclX" uri="http://tempuri.org" />
    <sch:pattern>
        <sch:rule context="/product-assembly-report/">
            <sch:assert test="number(employeeCount) eq count(employees/employee)">
                Wrong employee count
            </sch:assert>
            <!--<sch:assert test="number(salaryExpenses) eq sum(employees/employee/salary)">
                Wrong salary
            </sch:assert>
            <sch:assert test="oclX:iterate(employees/employee, 0, 1, count(employees/employee/salary)) eq salaryExpenses">
                Wrong salary (oclx) 
            </sch:assert>	-->
        </sch:rule>
        <!-- budu overovat, ze zadny employee neni ve dvou ruznych oddeleni, tedy constraint -->
        
        <!-- 
            context: Product
            inv: flatten(self.supply.collect(supply | 
            supply.supplied-part.collect(sp | p.code)))
            = self.parts.part->collect(p | p.code)            
        -->
        
    </sch:pattern>
</sch:schema>
