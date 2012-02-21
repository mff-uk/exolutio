<?xml version="1.0" encoding="utf-8"?>
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">  
  <sch:pattern id="choice">
    <!-- either open tournament or belongs to some league --> 
    <sch:rule context="/tournaments/tournament">
      <!--self.qualification.choice_1.OpenTournament.open = True or self.qualification.choice_1.League.leagueName <> null-->
      <sch:assert test="xs:boolean(qualification/@open) eq true() or exists(qualification/@leagueName)" />      
    </sch:rule>
    
    
    <!-- now some other methods how to rewrite previous constraint
    to demonstrate choice naming and PSM navigation 
    remark: in schematron they are all the same 
    --> 
    <sch:rule context="/tournaments/tournament">
      <!--self.qualification.choice_1.OpenTournament.open = True or self.qualification.choice_1.League.leagueName <> null-->
      <sch:assert test="xs:boolean(qualification/@open) eq true() or exists(qualification/@leagueName)" />   
    </sch:rule>    
    
  </sch:pattern>
</sch:schema>
  
