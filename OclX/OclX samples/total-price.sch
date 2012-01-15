<?xml version="1.0" encoding="utf-8"?>
<sch:schema xmlns="http://purl.oclc.org/dsdl/schematron" xmlns:sch="http://purl.oclc.org/dsdl/schematron" queryBinding="xslt2" schemaVersion="ISO19757-3">
	<sch:title>Test ISO schematron file. Introduction mode</sch:title>
	<sch:pattern>
		<sch:rule context="/purchase">
			<sch:assert test="totalprice = sum(item/price)">
					Wrong total price
			</sch:assert>
		</sch:rule>
	</sch:pattern>
</sch:schema>
