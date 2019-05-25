<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" >
	<xsl:output method="html" indent="yes" />
	<xsl:template match="/ArrayOfQueryResultData">
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
				<h2>AdHoc Query Results</h2>
				<table>
					<tr>
						<td class="header">Server</td>
						<td class="header">Database</td>
						
					
						<td class="header">RowCount</td>
						<td class="header">Row #</td>

						<xsl:for-each select="QueryResultData[1]/QueryAppendData[1]/QueryRowItem">
							<td class="header">
								<xsl:value-of select="@ColumnName"/>
							</td>
						</xsl:for-each>
						
						<xsl:for-each select="QueryResultData[1]/ColumnDefinition[1]/Definition">
							<td class="header">
								<xsl:value-of select="@Name"/>
							</td>
						</xsl:for-each>
					

					</tr>
					<!--</xsl:for-each>-->
					<xsl:for-each select ="QueryResultData/Results/Result">
						<tr>
								<xsl:if test="position() mod 2 = 0">
									<xsl:attribute name="class">alt</xsl:attribute>
								</xsl:if>
								<td id="script">
									<xsl:value-of select="../../Server"/>
								</td>
								<td id="script">
									<xsl:value-of select="../../Database"/>
								</td>

							
							<td id="script">
								<xsl:value-of select="../../RowCount"/>
							</td>
							<td id="script">
								<xsl:value-of select="count(preceding-sibling::*)+1"/>
							</td>
							<xsl:for-each select="../../QueryAppendData/QueryRowItem">
								<td id="script">
									<xsl:value-of select="@Value"/>
								</td>
							</xsl:for-each>
						<xsl:for-each select="Row">
							<td id="script">
								<xsl:value-of select="@Value"/>
							</td>

						</xsl:for-each>
						
						</tr>
					</xsl:for-each>
					<!--</xsl:for-each>-->
				</table>
			</body>
		</html>
			</xsl:template>


</xsl:stylesheet>
