<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" >
	<xsl:output method="html" indent="yes" />
	<xsl:template match="/ServerStatusDataCollection">
		<html>
			<head>
				<style>
					body {font-family:Calibri;}
					tr.alt #script {background-color:#ffffdd;}
					.header {background-color:#F1EFE2;}
					#badStatus { color: red }
					tr.alt #badStatus {background-color:#ffffdd;}
					table {border-style: solid; border-color: #F1EFE2; border-width: 1px; border-collapse: collapse;}
					td {border-style: solid ; border-color: #F1EFE2; border-width: 1px;}
					.info {font-family:Calibri; font-size : 10pt }
					#alt { background-color:#F1EFE2;}
					.infoHeader {font-family:Calibri; font-size : 10pt; font-weight : bold }
				</style>
			</head>
		<body>
			<h2>Sql Build File / Database Comparison</h2>
			<h4>Build File Name: <xsl:value-of select="BuildFileNameShort" />
		</h4>
			<h5>(<xsl:value-of select="BuildFileNameShort" />)
		</h5>
			<table>
		<tr>
			<td class="header">Server</td>
			<td class="header">Database</td>
		<xsl:for-each select="Servers/Server[1]/Databases[1]/Database[1]/ArrayOfScriptStatusData[1]">
			<xsl:if test="position()=1">
				<xsl:for-each select="ScriptStatusData">
					<td class="header"><xsl:value-of select="ScriptName"/></td>
				</xsl:for-each>
			</xsl:if>
		
		</xsl:for-each>
	</tr>
		
		<xsl:for-each select="Servers/Server/Databases/Database">
			<tr id="script">
				<xsl:if test="position() mod 2 = 0">
					<xsl:attribute name="class">alt</xsl:attribute>
				</xsl:if>
				<td id="script"><xsl:value-of select="../../@Name"/>
							</td><td id="script">
								<xsl:value-of select="@Name"/>
							</td><xsl:for-each select="ArrayOfScriptStatusData/ScriptStatusData">
								<td id="script">
									<xsl:if test="ScriptStatus != 'UpToDate' and ScriptStatus !='Locked'">
										<xsl:attribute name="id">badStatus</xsl:attribute>
									</xsl:if>
									<xsl:value-of select="ScriptStatus"/>
								</td>
			</xsl:for-each>
			</tr>
		</xsl:for-each>
				
				
			</table>

			<BR/>
			<div class="infoHeader">Status Code Help:</div>
			<table class="info">
				<tr>
					<td>UptoDate</td>
					<td>The script has been run and the database has been successfully updated</td>
				</tr>
				<tr id="alt">
					<td>Locked</td>
					<td>The script has been run and the database has been successfully updated. However, this script is marked NOT to be repeated</td>
				</tr>
				<tr>
					<td>NotRun</td>
					<td>This script has not been run on the databaase</td>
				</tr>
				<tr id="alt">
					<td>ChangedSinceCommit</td>
					<td>This script has been run prior, but is has been changed since last run (needs to be re-run)</td>
				</tr>
				<tr>
					<td>ServerChange</td>
					<td>This script has been run prior, but the database has been manually updated since last run. The build file script may be out of date.</td>
				</tr>
				<tr id="alt">
					<td>NotRunButOlderVersion</td>
					<td>This script has NOT been run and the database has been manually updated since the build file was created. The build file script may be out of date.</td>
				</tr>
				<tr>
					<td>Unknown</td>
					<td>The status of this script can not be determined</td>
				</tr>
			</table>
		</body>
	</html>
		</xsl:template>
	

</xsl:stylesheet>
