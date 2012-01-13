<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:oclX="http://eXolutio.com/oclX/dynamic"
  xmlns:oclXin="http://eXolutio.com/oclX/dynamic/internal"
  xmlns:oclDate="http://eXolutio.com/oclX/types/date" 
  xmlns:saxon="http://saxon.sf.net/"
  exclude-result-prefixes="oclX saxon xs oclDate oclXin" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" version="2.0">

  <xsl:import href="../OclX/oclX-dynamic.xsl"/>
  <xsl:output indent="yes"/>
  
  
  <xsl:template match="tournament">
    <xsl:variable name="variables" as="item()*">
      <variables/>
      <!-- just a placeholder -->
      <dummy/>      
      <!-- name of the $self variable -->
      <self/>      
      <!-- value of the $self variable-->
      <xsl:sequence select="."/>  
    </xsl:variable>
    
    CONTEXT TOURNAMENT
    
    <!-- 
      Each day shows only matches taking place that day
      expects conversion dateTime -> date using Date() function
    -->
    Each day shows only matches taking place that day
    <!--<sch:rule context="/tournament/matches/day">
      <sch:assert test="oclX:forAll(match, 'm', 'oclX:Date(m/start) = date)" />
    </sch:rule>    -->
    
    <!-- Each Tournament conducts at least one Match on the first day of the Tournament 
         * working constraint from ocl * 
         matches.day.match->exists(m:Match | m.start = start)
    -->
    Each Tournament conducts at least one Match on the first day of the Tournament 
    <xsl:copy-of select="oclX:exists(matches/day/match, 'm', 'xs:date(xs:dateTime($m/start)) eq xs:date(xs:dateTime($self/start))', $variables)" />
        
    <!-- working simple constraint -->
    No Will Black
    <xsl:copy-of select="
      oclX:forAll(
      //player/name,
      'n',
      'xs:string($n) ne ''Will Black''',
      $variables
      )      
      "
    />

    <!-- days are different 
         * working constraint from ocl *
         matches.day->forAll(d1, d2 | d1 != d2 implies d1.date <> d2.date) i.e.
         matches.day->forall(d1 | matches.day->forall(d2 | d1 != d2 implies d1.day != d2.day ))

         pay attention to node comparison (not($d1 is $d2)) vs $d1 ne $d2
    -->
    Days are different
    <xsl:copy-of select="
      oclX:forAll(
      matches/day,
      'd1',
      'oclX:forAll($self/matches/day, ''d2'', ''if (not($d1 is $d2)) then $d1/date ne $d2/date else true()'', $variables)',
      $variables
      )      
      "
    />
    
    <!-- All Matches in a Tournament occur within the Tournament’s time frame 
         * working constraint from ocl * 
         matches.day.match->forAll(m:Match | m.start.after(start) and m.end.before(end)) i.e.
         matches.day->collect(it | it.match)->forAll(m:Match | m.start.after(start) and m.end.before(end))
    -->
    All Matches in a Tournament occur within the Tournament’s time frame 
    <xsl:copy-of
      select="
      oclX:forAll(
        oclX:collect(matches/day, 'it', '$it/match', $variables),        
        'm',
        'oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)',
        $variables
      ) "/>

      <xsl:apply-templates select="//match" />
  </xsl:template>
  
  <xsl:template match="match">
    <xsl:variable name="variables" as="item()*">
      <variables/>
      <!-- just a placeholder -->
      <dummy/>      
      <!-- name of the $self variable -->
      <self/>      
      <!-- value of the $self variable-->
      <xsl:sequence select="."/>  
    </xsl:variable>
    
    CONTEXT MATCH 
    
    <!-- A match can only involve players who are accepted in the tournament 
      * working constraint from ocl *
      matchPlayers.player->forAll(p|
      p.parent.parent.parent.parent.parent.participatingPlayers.player->exists(px |
      px.name = p.name))
    -->
    A match can only involve players who are accepted in the tournament
    <xsl:copy-of select="
      oclX:forAll(match-players/player, 
      'p', 'oclX:exists($p/../../../../../participating-players/player, ''regp'', ''$regp/name eq $p/name'', $variables)', $variables)   
      "/>
  </xsl:template>
</xsl:stylesheet>
