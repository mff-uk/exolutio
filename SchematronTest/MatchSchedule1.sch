<?xml version="1.0" encoding="UTF-8"?><sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">

  <sch:pattern id="collections">
    <!--Below follow constraints from OCL script 'collections'. -->
    <!-- All Matches in a Tournament occur
    within the Tournament’s time frame --> 
    <sch:rule context="/tournament">
      <!--self.matches.day->Collect(d : Day | d.match)->forAll(m : Match | m.start.after(self.start) and m.end.before(self.end))-->
      <sch:assert test="oclX:forAll(oclX:collect(matches/day, 'd', '$d/match', $variables), 'm', 'oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)', $variables)"/>
    </sch:rule>
    
    <!-- Each Tournament conducts at
      least one Match on the first
      day of the Tournament -->
    <sch:rule context="/tournament">
      <!--self.matches.day->collect(d : Day | d.match)->exists(m : Match | m.start = self.start)-->
      <sch:assert test="oclX:exists(oclX:collect(matches/day, 'd', '$d/match', $variables), 'm', 'xs:dateTime($m/start) eq xs:dateTime($self/start)', $variables)"/>
      <!-- alternative --> 
      <sch:assert test="oclX:exists(matches/day/match, 'm', '$m/start eq $self/start', $variables)"/>      
    </sch:rule>
    
    <!-- A match can only involve players who are
    accepted in the tournament  -->
    <sch:rule context="/tournament/matches/day/match">
      <!--self.matchPlayers.player->forAll(p : MatchPlayer | p.MatchPlayers.Match.Day.Matches.Tournament.participatingPlayers.player->exists(px : Player | px.name = p.name))-->		
      <sch:assert test="oclX:forAll(matchPlayers/player, 'p', 'oclX:exists($p/../../../../../participatingPlayers/player, ''px'', ''$px/name eq $p/name'', $variables)', $variables)"/>
    </sch:rule>      
    <!-- players have unique email -->
    <sch:rule context="/tournament/participatingPlayers">
      <!--self.player->isUnique(p : Player | p.email)-->
      <sch:assert test="oclX:isUnique(player, 'p', 'data($p/email)', $variables)"/>
    </sch:rule>
  </sch:pattern>
  <sch:pattern id="empty">
    <!--Below follow constraints from OCL script 'empty'. -->
  </sch:pattern>
  <sch:pattern id="days">
    <!--Below follow constraints from OCL script 'days'. -->
    <!-- 
      Each day shows only matches taking place that day
      expects conversion dateTime -> date using getDate() function
    -->
    <sch:rule context="/tournament/matches/day">
      <sch:assert test="oclX:forAll(match, 'm', 'oclDate:getDate($m/start) eq xs:date($self/date)', $variables)"/>
    </sch:rule>      
    <!-- days are unique --> 
    <sch:rule context="/tournament">
      <!--self.matches.day->forAll(d1 : Day, d2 : Day | d1 <> d2 implies d1.date <> d2.date)-->
      <sch:assert test="oclX:forAllN(matches/day, 'd1, d2', 'if (not($d1 is $d2)) then xs:date($d1/date) ne xs:date($d2/date) else true()', $variables)"/>
    </sch:rule>
    <!-- Dates consistency  -->
    <sch:rule context="/tournament">
      <!--self.start <= self.end-->
      <sch:assert test="xs:dateTime(start) le xs:dateTime(end)"/>
    </sch:rule> 
  </sch:pattern>
</sch:schema>