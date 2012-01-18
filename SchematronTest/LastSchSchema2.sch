<?xml version="1.0" encoding="UTF-8"?><sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
 
  <sch:pattern>
     
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:forAll(oclX:collect($self/matches/day, 'it', '$it/match', $variables),'m','oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)', oclX:vars(.)) "/>    
    </sch:rule>
    
    
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:exists(matches/day/match, 'm', '$m/start eq $self/start')"/>
    </sch:rule>
    
    
    <sch:rule context="/tournament/matches/day/match">
      		
      <sch:assert test="oclX:forAll(match-players/player,'p', 'ocl:exists(p/../../../../../participating-players/player, 'px', 'px/name = p/name')')"/> 
    </sch:rule>      
      
    
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:holds('$self/start le $self/end', oclX:vars(.))"/>
    </sch:rule>        
    
    <sch:rule context="/tournament/matches/day">
      <sch:assert test="oclX:forAll(match, 'm', 'oclDate:getDate($m/start) = $self/date', oclX:vars(.))"/>
    </sch:rule>      
  </sch:pattern>
  
</sch:schema>