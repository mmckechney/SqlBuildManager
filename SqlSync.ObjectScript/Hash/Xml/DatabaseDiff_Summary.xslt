<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" >
	<xsl:output method="html" indent="yes" />
	<xsl:template match="/ObjectScriptHashReportData">
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
				<h2>Multiple Database Comparison Summary</h2>
				<h3>
					Baseline Database: <xsl:value-of select="BaseLineServer" />.<xsl:value-of select="BaseLineDatabase" />
				</h3>
				<h5>
					End Process Time: <xsl:value-of select="ProcessTime" />
				</h5>
				<h4>NOTE: This report shows only database objects that do not match the definition of the baseline database</h4>

				<table>
					<tr>
						<td class="header">Server</td>
						<td class="header">Database</td>
						<td class="header">Type</td>
						<td class="header">Name</td>
						<td class="header">Status</td>
					</tr>

					<xsl:for-each select="DatabaseData/ObjectScriptHashData">
						<tr id="script" class="alt">
							<td id="script">
								<xsl:value-of select="Server"/>
								<!-- Server Name-->
							</td>
							<td id="script">
								<xsl:value-of select="Database"/>
								<!-- Database Name -->

							</td>
							<td id="script"></td>
							<td id="script"></td>
							<td id="script"></td>
						</tr>
						<xsl:for-each select="Tables/Table">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										Table
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>

						<xsl:for-each select="StoredProcedures/StoredProcedure">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										Stored Procedure
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>

						<xsl:for-each select="Functions/Function">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										Function
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>

						<xsl:for-each select="Views/View">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										View
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>


						<xsl:for-each select="KeysAndIndexes/KeysAndIndexe">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										Key/Index
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>


						<xsl:for-each select="Roles/Role">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										Role
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>

						<xsl:for-each select="Logins/Login">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										Login
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>

						<xsl:for-each select="Schemas/Schema">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										Schema
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>

						<xsl:for-each select="Schemas/Schema">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										Schema
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>

						<xsl:for-each select="Users/User">
							<xsl:if test="@ComparisonValue!= 'Same'">
								<tr>
									<td></td>
									<td></td>
									<td id="script">
										User
									</td>
									<td>
										<xsl:value-of select="@Name"/>
									</td>
									<td>
										<xsl:value-of select="@ComparisonValue"/>
									</td>
								</tr>
							</xsl:if>
						</xsl:for-each>

					</xsl:for-each>


				</table>

				<BR/>
				<div class="infoHeader">Status Code Help:</div>
				<table class="info">
					<tr>
						<td>Different</td>
						<td>The object definition in the target database differs from that in the baseline database</td>
					</tr>
					<tr id="alt">
						<td>Missing</td>
						<td>The object was found in the baseline database is missing from the target database</td>
					</tr>
					<tr>
						<td>Added</td>
						<td>The object was found in the target database, but not in the baseline database</td>
					</tr>
					<tr id="alt">
						<td>Missing on both</td>
						<td>The object is not found in either the baseline nor the targe database.</td>
					</tr>
				</table>
			</body>
		</html>
	</xsl:template>


</xsl:stylesheet>
