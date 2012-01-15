<?xml version="1.0" encoding="UTF-8"?><sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <sch:pattern>
     
    
    <sch:rule context="/tournament">
      <sch:assert test="         oclX:forAll(         oclX:collect(matches/day, 'it', '$it/match', $variables),                 'm',         'oclDate:after($m/start, $self/start) and oclDate:before($m/end, $self/end)',         oclX:vars(.)         ) ">
        All Matches in a Tournament occur
        within the Tournament’s time frame fail.
      </sch:assert>
      
    </sch:rule>
    
    
    
    
    
    
    
    
    
    
    
    
    
    
  </sch:pattern>
  
  <sch:pattern>
     
    
    <sch:rule context="/tournament">
      <sch:assert test="1=1">a
      </sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>