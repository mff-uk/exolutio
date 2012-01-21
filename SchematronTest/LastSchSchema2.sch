<?xml version="1.0" encoding="UTF-8"?><sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  
  <sch:pattern id="defaultname">
    
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:forAll(oclX:collect(matches/day, 'd', '$d/match', $variables), 'm', 'oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)', $variables)"/>
    </sch:rule>
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:exists(oclX:collect(matches/day, 'd', '$d/match', $variables), 'm', '$m/start eq $self/start', $variables)"/>
    </sch:rule>
    <sch:rule context="/tournament/matches/day/match">
      
      <sch:assert test="oclX:forAll(matchPlayers/player, 'p', 'oclX:exists($p/../../../../../participatingPlayers/player, ''px'', ''$px/name eq $p/name'', $variables)', $variables)"/>
    </sch:rule>
    <sch:rule context="/tournament">
      
      <sch:assert test="start le end"/>
    </sch:rule>
  </sch:pattern>
  <sch:pattern id="defaultname">
    
  </sch:pattern>
  <sch:pattern id="defaultname">
    
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:forAllN(matches/day, 'd1, d2', 'if (not($d1 is $d2)) then $d1/date ne $d2/date else true()', $variables)"/>
    </sch:rule>
  </sch:pattern>
</sch:schema>