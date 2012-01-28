xquery version "3.0";

import module namespace oclX = 
"http://eXolutio.com/oclX/dynamic" at "play.xq";

declare namespace map = "http://www.w3.org/2005/xpath-functions/map";
declare namespace xs="http://www.w3.org/2001/XMLSchema";

(:
map:get()
map:get(map { 1 := 'aaa'}, 1)

oclX:forAll((4,5,6),function($it as item()) as xs:boolean { $it > 3 },())

map:get(map { 1 := 'aaa'}, 1)

:)
(:
map:get(map { 1 := 'aaa'}, 1):)
(:
let 
$m2 := map{1:= 'a', 2 := map { 3 := 'aaa'}}
return 
if ($m2 instance of map(*))
then
(\:map:get(map:get($m2, 1),1):\)
$m2(2)(3)
else 
5:)


(:let
$week := map{0:="Sonntag", 1:="Montag", 2:="Dienstag", 3:="Mittwoch", 4:="Donnerstag", 5:="Freitag", 6:="Samstag"}
return
(map:new(($week, map{7:="Unbekannt"})), $week)[2](7):) 
(:returns map{0:="Sonntag", 1:="Montag", 2:="Dienstag", 3:="Mittwoch", 4:="Donnerstag", 5:="Freitag", 6:="Samstag", 7:="Unbekannt"}:)



oclX:forAll((4,5,6),function($it as item()) as xs:boolean { $it > 3 },())






