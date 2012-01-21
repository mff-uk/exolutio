<?xml version="1.0" encoding="utf-8"?>
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
    
  <sch:pattern id="applications">
    <!--Below follow constraints from OCL script 'applications'. -->
    <sch:rule context="/tournamentApplication">
      <!--self.requiredQualificationPoints = null or self.player->forAll(p : Player | p.points >= self.requiredQualificationPoints)-->
      <sch:assert test="not(exists(requiredQualificationPoints)) or oclX:forAll(player, 'p', '$p/points ge $self/requiredQualificationPoints', oclX:vars(.))" />
    </sch:rule>
       
    <sch:rule context="/tournamentApplication">
      <!--self.requiredQualificationPoints = null or self.player.points >= self.requiredQualificationPoints-->
      <sch:assert test="not(exists(requiredQualificationPoints)) or xs:integer(player/points) ge xs:nonNegativeInteger(requiredQualificationPoints)"/> 
    </sch:rule>
    
    <sch:rule context="/tournamentApplication/player">
      <!--self.playerLeague.name = self.Tournament.tournamentLeague.name-->
      <sch:assert test="playerLeague/name eq ../tournamentLeague/name" />
    </sch:rule>
  
  </sch:pattern>
</sch:schema>

<!--
  
  context Tournament
  inv: requiredQualificationPoints = null 
  or player->forAll(p | p.points >= requiredQualificationPoints)
  
  /*
  smarter enginge should rewrite the preceding expression into a simpler one 
  (seeing that the collection player can have only one instance)
  The rewrited expression would be: 
  */
  
  context Tournament
  inv: requiredQualificationPoints = null or (player.points >= requiredQualificationPoints)
  
  /*
  is rewriting different for different functions?
  
  forAll, exists, sum: substitute by body
  collect: substitute by "if body = true then body else ()" 
  reject: substitute by "if body = false then body else ()"
  iterate: more complicated 
  */
  
  /* leagues are correct */
  context Player
  inv: league.name = parent().tournament-league.name
  -->