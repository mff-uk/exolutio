<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:oclX="http://eXolutio.com/oclX/dynamic"
  xmlns:oclXin="http://eXolutio.com/oclX/dynamic/internal"
  xmlns:oclDate="http://eXolutio.com/oclX/types/date" 
  xmlns:saxon="http://saxon.sf.net/"
  exclude-result-prefixes="oclX saxon xs oclDate oclXin" 
  
  xmlns:xs="http://www.w3.org/2001/XMLSchema" version="3.0">
  
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
    
    <!-- either open tournament or belongs to some league      
         qualification.choice_1.child_OpenTournament.open = true or 
         qualification.choice_1.child_League.leagueName <> null -->
    <!--<xsl:sequence select="for $i in 1 to 10 return map:entry('a', 'b')" />-->
  </xsl:template>
  
</xsl:stylesheet>
