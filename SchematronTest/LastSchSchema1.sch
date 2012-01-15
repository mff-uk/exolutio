<?xml version="1.0" encoding="UTF-8"?><sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <sch:pattern>
    <!-- All Matches in a Tournament occur
    within the Tournament’s time frame --> 
    
    <sch:rule context="/tournament">
      <sch:assert test="         oclX:forAll(         oclX:collect(matches/day, 'it', '$it/match', $variables),                 'm',         'oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)',         oclX:vars(.)         ) ">
        All Matches in a Tournament occur
        within the Tournament’s time frame fail.
      </sch:assert>
      
    </sch:rule>
    
    <!-- Each Tournament conducts at
      least one Match on the first
      day of the Tournament -->
    <!--<sch:rule context="/tournament">
      <sch:assert test="
        oclX:exists(matches/day/match, 'm', 'm/start/ eq start')
        ">
      </sch:assert>      
    </sch:rule>-->
    
    <!-- A match can only involve players who are
    accepted in the tournament  -->
    
    <!--<sch:rule context="/tournament/matches/day/match">
      <sch:assert test="
        oclX:forAll(match-players/player, 
        'p', 'ocl:exists(p/../../../../../participating-players/player, &apos;px&apos;, &apos;px/name = p/name&apos;)') 
        ">
      </sch:assert>      
    </sch:rule>-->
    
    
    <!-- 
      Each day shows only matches taking place that day
      expects conversion dateTime -> date using Date() function
    -->
    <!--<sch:rule context="/tournament/matches/day">
      <sch:assert test="oclX:forAll(match, 'm', 'oclX:Date(m/start) = date)" />
    </sch:rule>-->
    
    
    
  </sch:pattern>
  
  <sch:pattern>
    <!-- All Matches in a Tournament occur
    within the Tournament’s time frame --> 
    
    <sch:rule context="/tournament">
      <sch:assert test="1=1">a
      </sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>