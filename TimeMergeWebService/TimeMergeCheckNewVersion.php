<?php

function logToFile($msg)
{ 
    // open file
    $filename = "TimeMerge_Log.txt";
    $fd = fopen($filename, "a");
    // append date/time to message
    $str = "[" . date("Y/m/d H:i:s", mktime()) . "] " . $msg; 
    // write string
    fwrite($fd, $str . "\n");
    // close file
    fclose($fd);
}

$computer = $_REQUEST['computer'];
$version = $_REQUEST['version'];
$telemetry = $_REQUEST['telemetry'];
logToFile("Version check: [$version] $computer $telemetry");
echo "Newest TimeMerge version: [1.2.7], published Apr 27, 2017";
?>