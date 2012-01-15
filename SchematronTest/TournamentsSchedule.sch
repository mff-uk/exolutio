<?xml version="1.0" encoding="UTF-8"?>
<sch:schema 
  xmlns:sch="http://purl.oclc.org/dsdl/schematron"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  >
  
  <sch:pattern>
    <!-- either open tournament or belongs to some league --> 
    <sch:rule context="/tournaments/tournament">
      <sch:assert test="
        qualification/@open == true or qualification/@league-name != '' 
        " />      
    </sch:rule>
    
    
    <!-- now some other methods how to rewrite previous constraint
    to demonstrate choice naming and PSM navigation 
    remark: in schematron they are all the same 
    --> 
    <sch:rule context="/tournaments/tournament">
      <sch:assert test="
        qualification/@open == true or qualification/@league-name != '' 
        " />      >
      <sch:assert test="
        qualification/@open == true or qualification/@league-name != '' 
        " />      
    </sch:rule>
    
  </sch:pattern>
</sch:schema>
  
