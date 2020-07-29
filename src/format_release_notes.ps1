
# Code from: https://stackoverflow.com/questions/37024568/applying-xsl-to-xml-with-powershell-exception-calling-transform
function TransformXML{
    param ($xml, $xsl, $output)

    if (-not $xml -or -not $xsl -or -not $output)
    {
        Write-Host "& .\format_release_notes.ps1 [-xml] xml-input [-xsl] xsl-input [-output] transform-output"
        return 0;
    }

    Try
    {
        $xslt_settings = New-Object System.Xml.Xsl.XsltSettings;
        $XmlUrlResolver = New-Object System.Xml.XmlUrlResolver;
        $xslt_settings.EnableScript = 1;

        $xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
        $xslt.Load($xsl,$xslt_settings,$XmlUrlResolver);
        $xslt.Transform($xml, $output);

    }

    Catch
    {
        $ErrorMessage = $_.Exception.Message
        $FailedItem = $_.Exception.ItemName
        Write-Host  'Error'$ErrorMessage':'$FailedItem':' $_.Exception;
        return 0
    }
    return "Saved output to $(Resolve-Path $output)"

}

$xml = Resolve-Path "./SqlSync/change_notes.xml"

$xslHTML = Resolve-Path "./SqlSync/change_notes.xslt"
$outputHTML = Resolve-Path "./SqlSync/change_notes.html"

$xslMD = Resolve-Path "./SqlSync/change_notes_md.xslt"
$outputMD = Resolve-Path "../docs/change_notes.md"

TransformXML -xml $xml -xsl $xslHTML -output $outputHTML
TransformXML -xml $xml -xsl $xslMD -output $outputMD
