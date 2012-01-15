<?xml version="1.0" encoding="utf-8"?>
<sch:schema 
  xmlns:sch="http://purl.oclc.org/dsdl/schematron"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  >
    
  <sch:pattern>
    <sch:rule context="/tournament-application">
      <sch:assert test="not(requiredQualificationPoints) or 
        oclX:forall(player, 'p.points >= requiredQualificationPoints', ())" />
 
    </sch:rule>
    
    <sch:rule context="/tournament-application">
      <sch:assert test="
        not(requiredQualificationPoints) or (player/points >= requiredQualificationPoints)"/>      
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