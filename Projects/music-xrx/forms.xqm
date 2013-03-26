xquery version '3.0';

module namespace music-forms = "http://jakubmaly.cz/music/forms";

import module namespace music-rest = "http://jakubmaly.cz/music/rest" at "music-rest.xqm";

declare namespace xs = "http://www.w3.org/2001/XMLSchema"; 
declare variable $music-forms:restxq := 'http://localhost:8080/exist/restxq';

declare function music-forms:create-form-view-Artist($ID as xs:integer?)
{
<div xmlns:ev="http://www.w3.org/2001/xml-events" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xf="http://www.w3.org/2002/xforms">
	<fieldset>
		<legend>Artist form</legend>
		<fieldset>
			<legend>Artist fields</legend>
			<xf:group appearance="full">
			    <xf:input ref="instance('Artist')/FirstName">
			        <xf:label>FirstName:</xf:label>
			    </xf:input>		    
			    <xf:input ref="instance('Artist')/LastName">
			        <xf:label>LastName:</xf:label>
			    </xf:input>		    		    
			</xf:group>
		</fieldset>
		<xf:group appearance="minimal" class="action-buttons">
			<xf:submit submission="{if ($ID) then 'update-Artist' else 'create-Artist'}">
		        <xf:label>Save</xf:label>        
		    </xf:submit>
		    {
		    if ($ID) then
		    <xf:submit submission="delete-Artist">
		        <xf:label>Delete</xf:label>        
		    </xf:submit>
		    else ()   
		    }
		</xf:group>
	</fieldset>		
</div>
};

declare function music-forms:create-form-model-Artist($ID as xs:integer?)
{
<xf:model 
    xmlns:xf="http://www.w3.org/2002/xforms"
    xmlns:ev="http://www.w3.org/2001/xml-events">
    <xf:instance id="Artist" xmlns="">
    {
        if ($ID) then 
            music-rest:Artist-detail($ID, map:entry('detail', true()))
        else             
            <Artist>
                <FirstName/>
                <LastName/>
            </Artist>
    }
    </xf:instance>    
    { 
        if ($ID) then 
            (<xf:submission id="update-Artist" method="put" replace="instance"
                resource="{$music-forms:restxq || '/music/Artists/' || $ID}"
                ref="instance('Artist')">
                <xf:message ev:event="xforms-submit-done" level="ephemeral">Artist updated.</xf:message>
                <xf:message ev:event="xforms-submit-error" level="ephemeral">Error occurred when updating Artist.</xf:message>
            </xf:submission>,    
            <xf:submission id="delete-Artist" method="delete" replace="none"
                resource="{$music-forms:restxq || '/music/Artists/' || $ID}"
                ref="instance('Artist')">
                <xf:action ev:event="xforms-submit-done">
                    <xf:message level="ephemeral">Artist deleted.</xf:message>
                    <xf:load resource="artists.html" />
                </xf:action>
                <xf:message ev:event="xforms-submit-error" level="ephemeral">Error occurred when deleting Artist.</xf:message>
            </xf:submission>)
        else 
            <xf:submission id="create-Artist" method="post" replace="instance"
                resource="{$music-forms:restxq || '/music/Artists'}"
                ref="instance('Artist')">
                <xf:action ev:event="xforms-submit-done">
                    <xf:message level="ephemeral">Artist created.</xf:message>
                    <xf:load resource="edit-artist.html?ID={{instance('Artist')/@ID}}" />
                </xf:action>
                <xf:message ev:event="xforms-submit-error" level="ephemeral">Error occurred when creating Artist.</xf:message>                
            </xf:submission>    
    }
</xf:model>
};

declare function music-forms:create-form-view-Album($ID as xs:integer?)
{
<div xmlns="http://www.w3.org/1999/xhtml" xmlns:ev="http://www.w3.org/2001/xml-events" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xf="http://www.w3.org/2002/xforms">
       
	<fieldset>
		<legend>Album form</legend>
		<fieldset>
			<legend>Album fields</legend>
			<xf:group appearance="full">
				<xf:input ref="instance('Album')/Title">
				    <xf:label>Title:</xf:label>
				</xf:input>      
				<xf:input ref="instance('Album')/CDcount">
				    <xf:label>CDcount:</xf:label>
				</xf:input>
				<!--
				<xf:textarea ref="instance('Album')/Description" mediatype="text/html">
				    <xf:label>Description:</xf:label>
				</xf:textarea>
				-->				
			</xf:group>
		</fieldset>
		<fieldset>
        	<legend>Album tracks listing</legend>
        	<fieldset>
				<legend>Track fields</legend>
				<table>
					<thead>
					  <th>CDnumber</th>
					  <th>Number</th>
					  <th>Title</th>
					  <th>Length</th>                      
					</thead>	
					<tbody id="Track-repeat" xf:repeat-nodeset="instance('Album')/Tracks/Track">
					  <tr>
					  	  <td>
					          <xf:output ref="CDnumber" />
					      </td>
					      <td>
					          <xf:output ref="Number" />
					      </td>
					      <td>
					          <xf:output ref="Title"/>
					      </td>
					      <td>
					          <xf:output ref="Length"/>
					      </td>
					  </tr>
					</tbody>                  
				</table>         
				
				<xf:group appearance="minimal" class="action-buttons">
					<xf:trigger>
						<xf:label>Delete track</xf:label>
						<xf:action>
						      <xf:delete nodeset="instance('Album')/Tracks/Track" at="index('Track-repeat')"/>                                    
						</xf:action>
					</xf:trigger>
					<xf:trigger>
						<xf:label>New track</xf:label>
						<xf:action>											
							<xf:insert nodeset="instance('Album')/Tracks/Track" at="index('Track-repeat')" 
								position="after" origin="instance('Track-template')" 
								if="exists(instance('Album')/Tracks/Track)"/>
							<xf:insert context="/Album/Tracks" at="index('Track-repeat')" 
								position="after" origin="instance('Track-template')" 
								if="not(exists(instance('Album')/Tracks/Track))"/>		
						</xf:action>
					</xf:trigger>
				</xf:group>
	                         
				<xf:group ref="instance('Album')/Tracks/Track[index('Track-repeat')]" appearance="full">				
					<xf:input ref="CDnumber">
					  <xf:label>CDnumber:</xf:label>
					  <xf:alert>CD number must be less than or equal to the number of CDs of the album</xf:alert>
					</xf:input>
					<xf:input ref="Number">
					  <xf:label>Number:</xf:label>
					  <xf:alert>Numbers of tracks must be consecutive</xf:alert>
					</xf:input>
					<xf:input ref="Title">
					  <xf:label>Title:</xf:label>
					</xf:input>
					<xf:input ref="Length">
					  <xf:label>Length:</xf:label>
					</xf:input>
					<xf:textarea ref="Lyrics">
					    <xf:label>Lyrics:</xf:label>
					</xf:textarea>
				</xf:group>
			</fieldset>
			<fieldset>
				<legend>Track artists listing</legend>
				<xf:group appearance="full">
					<xf:select ref="instance('Album')/Tracks/Track[index('Track-repeat')]/Artists/@ID" appearance="full">
						<xf:label>Artists: </xf:label>
						<xf:itemset nodeset="instance('Artists')/Artist" >
							<xf:label>
							    <xf:output value="if (data(LastName)) then concat(LastName, ', ', FirstName) else FirstName" />
							</xf:label>
					        <xf:value ref="@ID"></xf:value>
					    </xf:itemset>
					</xf:select>
				</xf:group>
			</fieldset>
		</fieldset>
		
		<fieldset>
			<legend>Album artists listing</legend>
			<xf:group appearance="full">
				<xf:select ref="instance('Album')/Artists/@ID" incremental="true" appearance="full">
					<xf:label>Artists: </xf:label>
					<xf:itemset nodeset="instance('Artists')/Artist" >
						<xf:label>
						    <xf:output value="if (data(LastName)) then concat(LastName, ', ', FirstName) else FirstName" />
						</xf:label>
				        <xf:value ref="@ID"></xf:value>
				    </xf:itemset>
				</xf:select>
			</xf:group>
		</fieldset>
        
        <xf:submit submission="{if ($ID) then 'update-Album' else 'create-Album'}">
            <xf:label>Save</xf:label>        
        </xf:submit>
        {
        if ($ID) then
        <xf:submit submission="delete-Album">
            <xf:label>Delete</xf:label>        
        </xf:submit>
        else ()   
        }
    </fieldset>
</div>
};

declare function music-forms:create-form-model-Album($ID as xs:integer?)
{
<xf:model xmlns:ev="http://www.w3.org/2001/xml-events" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xf="http://www.w3.org/2002/xforms"> 
    <xf:instance id="Album" xmlns="">
        {
        if ($ID) then
        	let $album := music-rest:Album-detail($ID, map:entry('detail', true())), 
        		$artists-function :=  music-forms:fix-xforms-Artists-collections#1 
        	return 
        	if ($album instance of element(Album)) then
        	music-forms:fix-xforms-collections($album, map { 'Artists' := $artists-function })
        	else 
        	<Album />
        else             
            <Album>
                <Title />
			    <Description />
			    <CDcount>1</CDcount>
			    <Tracks />			    	
			    <Artists ID="" />
            </Album>
        }
    </xf:instance>
    <xf:bind id="Album-Title" nodeset="instance('Album')/Title" required="true()" />
    <xf:bind id="Album-CDcount" nodeset="instance('Album')/CDcount" type="positiveInteger" required="true()" />
    <xf:bind id="Album-Tracks-Track-CDnumber" nodeset="instance('Album')/Tracks/Track/CDnumber" 
	type="positiveInteger" required="true()"
	constraint="xs:positiveInteger(.) le xs:positiveInteger(../../../CDcount)" />
	<xf:bind id="Album-Tracks-Track-Number" nodeset="instance('Album')/Tracks/Track/Number" 
		type="positiveInteger" required="true()" 
		constraint="
		(. = '') or 
		(for $n in xs:positiveInteger(../CDnumber) return 
		xs:positiveInteger(.) eq (count(../preceding-sibling::Track[xs:positiveInteger(CDnumber) eq $n]) + 1))     	
		"
	/>
    <xf:bind id="Album-Tracks-Track-Title" nodeset="instance('Album')/Tracks/Track/Title" type="xs:string" required="true()" />
    <xf:bind id="Album-Tracks-Track-Length" nodeset="instance('Album')/Tracks/Track/Length" />
    <!-- 
    <xf:bind id="Album-Tracks-Track" 
    	ref="instance('Album')/Tracks/Track" 
    	constraint="every $i in instance('Album')/Tracks/Track satisfies (xs:integer($i/CDnumber) le 3)" />
  	-->
    <xf:instance id="Track-template" xmlns="">
        <Track>
        	<CDnumber>1</CDnumber>
            <Number/>
            <Title/>
            <Length/>
            <Artists ID="" />      	
        </Track>
    </xf:instance>
    <xf:instance id="Artists" xmlns="">   
    	{ music-rest:Artists-list(map:entry('detail', false())) } 
    </xf:instance>    
    { 
        if ($ID) then 
            (<xf:submission id="update-Album" method="put" replace="instance"
                resource="{$music-forms:restxq || '/music/Albums/' || $ID}"
                ref="instance('Album')">
                <xf:message ev:event="xforms-submit-done" level="ephemeral">Album updated.</xf:message>
                <xf:message ev:event="xforms-submit-error" level="ephemeral">Error occurred when updating Album.</xf:message>
            </xf:submission>,    
            <xf:submission id="delete-Album" method="delete" replace="none"
                resource="{$music-forms:restxq || '/music/Albums/' || $ID}"
                ref="instance('Album')">
                <xf:action ev:event="xforms-submit-done">
                    <xf:message level="ephemeral">Album deleted.</xf:message>
                    <xf:load resource="albums.html" />
                </xf:action>
                <xf:message ev:event="xforms-submit-error" level="ephemeral">Error occurred when deleting Album.</xf:message>
            </xf:submission>)
        else 
            <xf:submission id="create-Album" method="post" replace="instance"
                resource="{$music-forms:restxq || '/music/Albums'}"
                ref="instance('Album')">
                <xf:action ev:event="xforms-submit-done">
                    <xf:message level="ephemeral">Album created.</xf:message>
                    <xf:load resource="edit-album.html?ID={{instance('Album')/@ID}}" />
                </xf:action>
                <xf:message ev:event="xforms-submit-error" level="ephemeral">Error occurred when creating Album.</xf:message>                
            </xf:submission>    
    }
</xf:model>
};

declare 
%private
function music-forms:fix-xforms-collections($element as element(), $collections as map(*))
{
	element { local-name($element) }
	{		
		for $subelement in $element/child::node()
		return 
			if ($subelement instance of element() and 
				map:contains($collections, local-name($subelement))) then
				let $f := $collections(local-name($subelement))
				return $f($subelement)
			else if ($subelement instance of element()) then  
				music-forms:fix-xforms-collections($subelement, $collections)
			else $subelement
	}
};

declare
%private
function music-forms:fix-xforms-Artists-collections($artists as element(Artists)) as element(Artists)
{
	<Artists ID="{string-join($artists/Artist/@ID, ' ')}" />	
};
