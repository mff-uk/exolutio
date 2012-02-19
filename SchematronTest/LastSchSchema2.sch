<?xml version="1.0" encoding="UTF-8"?><sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  
  <sch:pattern id="collections">
    
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:forAll(oclX:collect($self/matches/day, function($d) { $d/match }), function($m) { oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end) })"/>
    </sch:rule>
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:exists(oclX:collect($self/matches/day, function($d) { $d/match }), function($m) { $m/start eq $self/start })"/>
    </sch:rule>
    <sch:rule context="/tournament/matches/day/match">
      
      <sch:assert test="oclX:forAll($self/matchPlayers/player, function($p) { oclX:exists($p/../../../../../participatingPlayers/player, function($px) { $px/name eq $p/name }) })"/>
    </sch:rule>
    <sch:rule context="/tournament/participatingPlayers">
      
      <sch:assert test="oclX:isUnique($self/player, function($p) { xs:data($p/email) })"/>
    </sch:rule>
  </sch:pattern>
  <sch:pattern id="empty">
    
  </sch:pattern>
  <sch:pattern id="days">
    
    <sch:rule context="/tournament">
      
      <sch:assert test="oclX:forAllN($self/matches/day, function($d1, $d2) { if (not($d1 is $d2)) then $d1/date ne $d2/date else true() })"/>
    </sch:rule>
    <sch:rule context="/tournament">
      
      <sch:assert test="start le end"/>
    </sch:rule>
  </sch:pattern>
</sch:schema>