xquery version "3.0";

module namespace oclX = "http://eXolutio.com/oclX/dynamic";
declare namespace xs="http://www.w3.org/2001/XMLSchema";

declare function oclX:inc($input as xs:integer)
as xs:integer
{
  $input + 1
};


declare function oclX:forAll(
  $collection as item()*,
  (: funkce = forOne, ma jeden parametr, vraci bool :)
  $body as function(item()) as xs:boolean, 
  $variables as item()*) as xs:boolean
{
  every $it in $collection satisfies $body($it) 
};
