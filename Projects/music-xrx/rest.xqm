xquery version "3.0";

module namespace music-rest = "http://jakubmaly.cz/music/rest";
declare namespace music = "http://jakubmaly.cz/music";
import module namespace functx = "http://www.functx.com" at "functx.xqm";
(: import module namespace music = "http://jakubmaly.cz/music" at 'ruian-lib.xqm'; :)

declare namespace rest="http://exquery.org/ns/restxq";
declare namespace req="http://exquery.org/ns/request";
declare namespace http = "http://expath.org/ns/http-client";
declare namespace xs="http://www.w3.org/2001/XMLSchema"; 
declare namespace xmldb = "http://exist-db.org/xquery/xmldb";

declare variable $music-rest:Artists-collection := '/db/apps/music/Artists/'; 
declare variable $music-rest:Albums-collection := '/db/apps/music/Albums/';

(: Artist - read :)
declare
    %rest:path("/music/Artists/{$ID}")
    %rest:GET
function music-rest:Artist-detail($ID as xs:integer) as element()*
{
	let $expands := music-rest:read-expand-parameters(true())
	return music-rest:Artist-detail($ID, $expands)
};

declare
function music-rest:Artist-detail($ID as xs:integer, $expands as map(xs:string, xs:boolean)) as element()*
{ 	
    let $match := 
    for $artist in doc($music-rest:Artists-collection || $ID || '.xml' )/*
    return 
    <Artist>
    {
        (
            for $field in $artist/(* | @*)
            return $field
        ),        
        (: expanding albums reference -- not part of the document stored in collection :)
        if (music-rest:should-expand($expands, 'Albums')) then
        (
        	<Albums>
        	{
	            for $album in music-rest:Albums-list(map:entry('detail', false()))/Album
	            where 
	            	some $album-artist in $album/Artists/Artist
	            	satisfies xs:integer($album-artist/@ID) eq $ID
	            return $album
            }
            </Albums>
        ) else ()
    }
    </Artist>
    return 
    if ($match) then $match 
    else music-rest:response404('Artist', 'ID = ' || $ID)
};

declare
    %rest:path("/music/Artists")
    %rest:GET
function music-rest:Artists-list() as element(Artists)
{
	let $expands := music-rest:read-expand-parameters(false())
	return music-rest:Artists-list($expands)
};

declare
function music-rest:Artists-list($expands as map(xs:string, xs:boolean)) as element(Artists)
{ 
    <Artists>
    {
    if (music-rest:should-expand($expands, ())) then
        for $artist in collection($music-rest:Artists-collection)
        return music-rest:Artist-detail($artist/*/@ID, $expands)
    else 
        for $artist in collection($music-rest:Artists-collection)
        return $artist/*
    }
    </Artists>
};

(: Artist - create :)
declare
    %rest:path("/music/Artists")    
    %rest:POST("{$artist}")
function music-rest:Artist-create($artist) as element(Artist)
{ 
    let $ID := music-rest:gener-id()    
    return music-rest:Artist-create-or-replace($ID, $artist)
};

(: Artist - create/update :)
declare
    %rest:path("/music/Artists/{$ID}")    
    %rest:PUT("{$artist}")
function music-rest:Artist-create-or-replace($ID as xs:integer, $artist) as element(Artist)
{
    let $artist-element := 
        if ($artist instance of document-node()) then $artist/* else $artist
    let $artist-with-ID := 
    	functx:add-attribute-if-not-present($artist-element, 'ID', $ID)        
    let $url := xmldb:store($music-rest:Artists-collection, $ID || '.xml', $artist-with-ID)
    return $artist-with-ID
};

(: Artist - delete :)
declare
    %rest:DELETE
    %rest:path("/music/Artists/{$ID}")
function music-rest:Artist-delete($ID as xs:string*) {
    xmldb:remove($music-rest:Artists-collection, $ID || '.xml')    
};

(: Album - read :)
declare
    %rest:path("/music/Albums/{$ID}")   
    %rest:GET
function music-rest:Album-detail($ID as xs:integer)
{
	let $expands := music-rest:read-expand-parameters(true()) return
	music-rest:Album-detail($ID, $expands)
};

declare
function music-rest:Album-detail($ID as xs:integer, $expands as map(xs:string, xs:boolean)) as element()*
{ 
    let $match :=
    for $album in doc($music-rest:Albums-collection || $ID || '.xml' )/*
    return 
    <Album>
    {
        for $field in $album/(* | @*)
        return 
            if (local-name($field) eq 'Artists' and 
            	music-rest:should-expand($expands, 'Artists')) then
            <Artists>
                {
                    for $artist in $field/Artist
                    return music-rest:Artist-detail($artist/@ID, map:entry('detail', false()))                    
                }
            </Artists>
            (: tracks are always expanded with full detail :) 
            else if (local-name($field) eq 'Tracks' and 
            		 music-rest:should-expand($expands, 'Tracks')) then
            <Tracks>
                {
                    for $track in $field/Track
                    return music-rest:Track-detail($track, true())                    
                }
            </Tracks>    
            else $field
    }
    </Album>
    return 
    if ($match) then $match 
    else music-rest:response404('Album', 'ID = ' || $ID)
};

declare
    %rest:path("/music/Albums")
    %rest:GET
function music-rest:Albums-list() as element(Albums)  
{ 
	let $expands := music-rest:read-expand-parameters(false())
	return music-rest:Albums-list($expands)
};

declare
function music-rest:Albums-list($expands as map(xs:string, xs:boolean)) as element(Albums)
{
	<Albums>
    {
    if (music-rest:should-expand($expands, ())) then
        for $album in collection($music-rest:Albums-collection)
        return music-rest:Album-detail($album/*/@ID, $expands)
    else 
        for $album in collection($music-rest:Albums-collection)
        return $album/*
    }
    </Albums>
};

(: Album - create :)
declare
    %rest:path("/music/Albums")    
    %rest:POST("{$album}")
function music-rest:Album-create($album) as element(Album)
{ 
    let $ID := music-rest:gener-id()    
    return music-rest:Album-create-or-replace($ID, $album)
};

(: Album - create/update :)
declare
    %rest:path("/music/Albums/{$ID}")    
    %rest:PUT("{$album}")
function music-rest:Album-create-or-replace($ID as xs:integer, $album) as element(Album)
{
    let $album-element := 
        if ($album instance of document-node()) then $album/* else $album
    let $album-with-ID := 
        functx:add-attribute-if-not-present($album-element, 'ID', $ID)
    let $album-fixed := music-rest:fix-xforms-collections($album-with-ID,  map { 'Artists' := music-rest:fix-xforms-Artists-collections#1 })
    let $url := xmldb:store($music-rest:Albums-collection, $ID || '.xml', $album-fixed)
    return $album-with-ID
};

(: Album - delete :)
declare
    %rest:DELETE
    %rest:path("/music/Albums/{$ID}")
function music-rest:Albums-delete($ID as xs:string*) {
    xmldb:remove($music-rest:Albums-collection, $ID || '.xml')    
};

(: Track read :)
declare
    %rest:path("/music/Tracks/{$ID}")   
    %rest:GET    
function music-rest:Track-detail($ID as xs:string) as element()*
{ 
    let $match :=
        for $album in collection($music-rest:Albums-collection)
        return music-rest:Track-detail($album/Album/Tracks/Track[@ID = $ID], true())
    return
    if ($match) then $match 
    else music-rest:response404('Album', 'ID = ' || $ID)        
};

declare 
function music-rest:Track-detail($track, $detail as xs:boolean) as element(Track)
{
    <Track>
    {
        for $field in $track/(* | @*)
        return 
            if (local-name($field) eq 'Artists') then
            <Artists>
                {
                    for $artist in $field/Artist
                    return music-rest:Artist-detail($artist/@ID, map:entry('detail', false()))                    
                }           
            </Artists>
            else $field
    }
    </Track>      
};

(: Track - create :)
declare
    %rest:path("/music/Albums/{$album-ID}/Tracks")   
    %rest:query-param("index", "{$index}") 
    %rest:POST("{$track}")
function music-rest:Track-create($album-ID as xs:integer, $track, $index) as element(Track)
{ 
    let $ID := music-rest:gener-id()    
    let $track-element := 
        if ($track instance of document-node()) then $track/* else $track
    let $track-with-ID := 
        functx:add-attribute-if-not-present($track-element, 'ID', $ID)
    let $album-doc := 
    	$music-rest:Albums-collection || $album-ID || '.xml'
    return 
    if ($index gt 1) then 
    	(update insert $track-with-ID following doc($album-doc)/Album/Tracks/Track[$index - 1], $track-with-ID)     	
    else if ($index eq 1) then
    	(update insert $track-with-ID preceding doc($album-doc)/Album/Tracks/Track[1], $track-with-ID)
    else 
    	(update insert $track-with-ID into doc($album-doc)/Album/Tracks, $track-with-ID)    
};

(: Track - replace :)
declare    
    %rest:path("/music/Albums/{$album-ID}/Tracks/{$ID}")    
    %rest:PUT("{$track}")
function music-rest:Track-replace($ID as xs:string, $album-ID as xs:integer, $track) as element(Track)
{
    let $track-element := 
        if ($track instance of document-node()) then $track/* else $track
    let $album-doc := 
    	$music-rest:Albums-collection || $album-ID || '.xml'
    return
    	(update replace doc($album-doc)/Album/Tracks/Track[@ID eq $ID] with $track-element, $track-element)
};


(: Track - delete :)
declare    
    %rest:path("/music/Albums/{$album-ID}/Tracks/{$ID}")
    %rest:DELETE
function music-rest:Track-delete($ID as xs:string, $album-ID as xs:integer) {
    let $album-doc := 
    	$music-rest:Albums-collection || $album-ID || '.xml'
    return 
    	(update delete doc($album-doc)/Album/Tracks/Track[@ID eq $ID])
};

declare 
%private
function music-rest:fix-xforms-Artists-collections($artists as element(Artists)) as element(Artists)
{
	<Artists>
	{
		for $ID in tokenize($artists/@ID, '\s+')
		return <Artist ID="{$ID}" />
	}
	</Artists>
};

declare 
%private
function music-rest:fix-xforms-collections($element as element(), $collections as map(*))
{
	element { local-name($element) }
	{		
		for $subelement in $element/(node() | @*)
		return 
			if ($subelement instance of element() and 
				map:contains($collections, local-name($subelement))) then
				let $f := $collections(local-name($subelement))
				return $f($subelement)
			else if ($subelement instance of element()) then  
				music-rest:fix-xforms-collections($subelement, $collections)
			else $subelement
	}
};

(: util functions :) 

declare 
%private 
function music-rest:gener-id() as xs:integer
{
    999000+util:random(1000)     
};

declare 
%private 
function music-rest:response404($name as xs:string, $identification as xs:string) as element()*
{
    <rest:response>
       <http:response status="404" message="{$name} not found">
           <http:header name="Content-Language" value="en"/>
           <http:header name="Content-Type" value="text/html; charset=utf-8"/>
       </http:response>
    </rest:response>,
    <html>
        <body>
            <p>
            {$name} with {$identification} not found.
            </p>
        </body>
    </html>
};

declare 
%private
function music-rest:read-expand-parameters($default-detail as xs:boolean) 
	as map(xs:string, xs:boolean)
{
	map:new(
		(
			for $name in req:parameter-names()
			where starts-with($name, 'expand-')
			return 
			map:entry(
				substring-after($name, 'expand-'), 
				xs:boolean(req:parameter($name))
			),
			if (req:parameter('detail')) then
				map:entry('detail', xs:boolean(req:parameter('detail')))
			else 
				map:entry('detail', $default-detail)
		)
	)
};

declare 
%private
function music-rest:print-map($map)
{
	string-join(
	for $k in map:keys($map)
	return $k || '=' || map:get($map, $k), ', ')
};

declare 
%private
function music-rest:should-expand($expands as map(xs:string, xs:boolean), $collection as xs:string?) as xs:boolean
{
	if ($collection) then
		if (map:contains($expands, $collection)) then
			map:get($expands, $collection)
		else 
			map:contains($expands, 'detail') and map:get($expands, 'detail')
	else 
		(map:contains($expands, 'detail') and map:get($expands, 'detail'))
		or
		(some $key in map:keys($expands)
		satisfies map:get($expands, $key))
};



