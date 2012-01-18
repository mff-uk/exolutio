<?xml version="1.0" encoding="UTF-8"?><sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
 
  <sch:pattern>
    <!-- All Matches in a Tournament occur
    within the Tournament’s time frame --> 
    <sch:rule context="/tournament">
      <!--self.matches.day->Collect( : Day | .match)->forAll(m : Match | m.start.after(self.start) and m.end.before(self.end))-->
      <sch:assert test="oclX:forAll(oclX:collect(matches/day, 'it', '$it/match', $variables),'m','oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)', oclX:vars(.)) "/>    
    </sch:rule>
    
    <!-- Each Tournament conducts at
      least one Match on the first
      day of the Tournament -->
    <sch:rule context="/tournament">
      <!--self.matches.day->collect( : Day | .match)->exists(m : Match | m.start = self.start)-->
      <sch:assert test="oclX:exists(oclX:collect(matches/day, '', '$/match', $variables), 'm', '$m/start eq $self/start', oclX:vars(.))"/>
      <!-- alternative --> 
      <sch:assert test="oclX:exists(matches/day/match, 'm', '$m/start eq $self/start', oclX:vars(.))"/>      
    </sch:rule>
    
    <!-- A match can only involve players who are
    accepted in the tournament  -->
    <sch:rule context="/tournament/matches/day/match">
      <!--self.matchPlayers.player->forAll(p : MatchPlayer | p.MatchPlayers.Match.Day.Matches.Tournament.participatingPlayers.player->exists(px : Player | px.name = p.name))-->		
      <sch:assert test="oclX:forAll(matchPlayers/player,'p', 'oclX:exists($p/../../../../../participatingPlayers/player, ''px'', ''$px/name = $p/name'', $variables)', oclX:vars(.))"/> 
    </sch:rule>      
      
    <!-- Dates consistency  -->
    <sch:rule context="/tournament">
      <!--self.start <= self.end-->
      <sch:assert test="oclX:holds('$self/start le $self/end', oclX:vars(.))"/>
    </sch:rule>        
    <!-- 
      Each day shows only matches taking place that day
      expects conversion dateTime -> date using Date() function
    -->
    <sch:rule context="/tournament/matches/day">
      <sch:assert test="oclX:forAll(match, 'm', 'oclDate:getDate($m/start) = $self/date', oclX:vars(.))"/>
    </sch:rule>      
  </sch:pattern>
  
</sch:schema>