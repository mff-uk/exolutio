<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:oclX="http://eXolutio.com/oclX/dynamic"
  xmlns:oclXin="http://eXolutio.com/oclX/dynamic/internal"
  xmlns:oclDate="http://eXolutio.com/oclX/types/date" 
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  exclude-result-prefixes="oclX saxon xs oclDate oclXin" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" version="3.0">

  <xsl:import href="../OclX/oclX-dynamic.xsl"/>
  <xsl:output indent="yes"/>

  <xsl:template match="tournament">
    <!--<xsl:sequence select="oclXin:combined-index-of(//day, //day[3])" />-->
    <xsl:variable name="variables" as="item()*" select="oclX:vars(.)" />    
    
      
    CONTEXT TOURNAMENT
    
     
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
    
    Days are different 2
    <xsl:copy-of select="
      oclX:forAllN(
      matches/day,
      'd1, d2',
      'if (not($d1 is $d2)) then xs:date($d1/date) ne xs:date($d2/date) else true()',
      oclX:vars(.)
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
    
    <!-- dates consistency
         start <= end
    -->
    Dates consistency
    <xsl:copy-of
      select="
      oclX:holds('$self/start le $self/end', $variables)
      "/>
    
    <xsl:apply-templates select="//match" />
    <xsl:apply-templates select="//day"/>
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

  <xsl:template match="day">
    CONTEXT DAY
    <!-- 
      Each day shows only matches taking place that day
      expects conversion dateTime -> date using getDate() function
      * working constraint from ocl *
      self.match->forAll(m | m.start.Date() = date)
    -->
    Each day shows only matches taking place that day
    <xsl:copy-of select="oclX:forAll(match, 'm', 'oclDate:getDate($m/start) = $self/date', oclX:vars(.))" />    
  </xsl:template>


  <!--<xsl:template match="/tournament">
    <xsl:copy-of select="//node()[not(node())]"/>
    <!-\-<xsl:apply-templates select="matches//*" />-\->
    <!-\-<xsl:copy-of select="//*[not(*)]"/>-\->
  </xsl:template>-->

 <xsl:template match="* | text() | @*">NO MATCHING RULE! (<xsl:value-of select="name()" />). </xsl:template>
</xsl:stylesheet>
