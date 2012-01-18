<?xml version="1.0" encoding="utf-8"?>
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
 
  <sch:pattern>
    <!--Below follow constraints from OCL script 'defaultname'. -->
    <!-- All Matches in a Tournament occur
    within the Tournament’s time frame --> 
    <sch:rule context="/tournament">
      <!--self.matches.day->Collect(d : Day | d.match)->forAll(m : Match | m.start.after(self.start) and m.end.before(self.end))-->
      <sch:assert test="oclX:forAll(oclX:collect(matches/day, 'd', '$d/match', $variables), 'm', 'oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)', oclX:vars(.))" />
    </sch:rule>
    
    <!-- Each Tournament conducts at
      least one Match on the first
      day of the Tournament -->
    <sch:rule context="/tournament">
      <!--self.matches.day->collect(d : Day | d.match)->exists(m : Match | m.start = self.start)-->
      <sch:assert test="oclX:exists(oclX:collect(matches/day, 'd', '$d/match', $variables), 'm', '$m/start eq $self/start', oclX:vars(.))" />
      <!-- alternative --> 
      <sch:assert test="oclX:exists(matches/day/match, 'm', '$m/start eq $self/start', oclX:vars(.))" />      
    </sch:rule>
    
    <!-- A match can only involve players who are
    accepted in the tournament  -->
    <sch:rule context="/tournament/matches/day/match">
      <!--self.matchPlayers.player->forAll(p : MatchPlayer | p.MatchPlayers.Match.Day.Matches.Tournament.participatingPlayers.player->exists(px : Player | px.name = p.name))-->		
      <sch:assert test="oclX:forAll(matchPlayers/player, 'p', 'oclX:exists($p/../../../../../participatingPlayers/player, ''px'', ''$px/name eq $p/name'', $variables)', oclX:vars(.))" /> 
    </sch:rule>      
      
    <!-- Dates consistency  -->
    <sch:rule context="/tournament">
      <!--self.start <= self.end-->
      <sch:assert test="start le end" />
    </sch:rule>        
        
  </sch:pattern>
  <sch:pattern>
    <!--Below follow constraints from OCL script 'defaultname'. -->
  </sch:pattern>
  <sch:pattern>    
    <!--Below follow constraints from OCL script 'defaultname'. -->
    <!-- 
      Each day shows only matches taking place that day
      expects conversion dateTime -> date using getDate() function
    -->
    <sch:rule context="/tournament/matches/day">
      <sch:assert test="oclX:forAll(match, 'm', 'oclDate:getDate($m/start) = $self/date', oclX:vars(.))" />
    </sch:rule>      
    <!-- days are unique --> 
    <sch:rule context="/tournament">
      <!--self.matches.day->forAll(d1 : Day, d2 : Day | d1 <> d2 implies d1.date <> d2.date)-->
      <sch:assert test="oclX:forAll(matches/day, 'd1', 'if ($d1 ne $d2) then $d1/date ne $d2/date else false()', oclX:vars(.))" />
    </sch:rule>
  </sch:pattern>
</sch:schema>