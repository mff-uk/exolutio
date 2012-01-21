﻿<?xml version="1.0" encoding="utf-8"?>
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <!-- Generated by eXolutio on 21.1.2012 20:43 from tournaments/TournamentsSchedule. -->
  <sch:pattern id="choice">
    <!--Below follow constraints from OCL script 'choice'. -->
    <sch:rule context="/tournaments/tournament">
      <!--self.qualification.choice_1.OpenTournament.open = True or self.qualification.choice_1.League.leagueName <> null-->
      <sch:assert test="xs:boolean(qualification/@open) eq true() or exists(qualification/@leagueName)" />
    </sch:rule>
    <sch:rule context="/tournaments/tournament">
      <!--self.qualification.choice_1.OpenTournament.open = True or self.qualification.choice_1.League.leagueName <> null-->
      <sch:assert test="xs:boolean(qualification/@open) eq true() or exists(qualification/@leagueName)" />
    </sch:rule>
  </sch:pattern>
</sch:schema>