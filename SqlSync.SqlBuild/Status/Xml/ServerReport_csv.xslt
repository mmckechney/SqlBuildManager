<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" >
	<xsl:output method="text" indent="yes" />
	<xsl:template match="/ServerStatusDataCollection">
Server,Database,<xsl:for-each select="Servers/Server[1]/Databases[1]/Database[1]/ArrayOfScriptStatusData[1]">
			<xsl:if test="position()=1">
				<xsl:for-each select="ScriptStatusData">
					<xsl:value-of select="ScriptName"/>
					<xsl:if test="position()!=last()">,</xsl:if>
					<xsl:if test="position()=last()">
						<xsl:text>
</xsl:text>
					</xsl:if>
				</xsl:for-each>
			</xsl:if>
		</xsl:for-each>
		<!--</xsl:for-each>-->
		<xsl:for-each select="Servers/Server/Databases/Database">
			<xsl:value-of select="../../@Name"/>,<xsl:value-of select="@Name"/>,<xsl:for-each select="ArrayOfScriptStatusData/ScriptStatusData">
				<xsl:value-of select="ScriptStatus"/>
				<xsl:if test="position()!=last()">,</xsl:if>
				<xsl:if test="position()=last()">
					<xsl:text>
</xsl:text>
				</xsl:if>
			</xsl:for-each>
		</xsl:for-each>
		<!--</xsl:for-each>-->

		</xsl:template>
	<!--<xsl:template match="/ServerStatusDataCollection">


		<xsl:for-each select="Server/Databases/Database">
			<tr>
				<td>
					<xsl:value-of select="../../@Name"/>.<xsl:value-of select="@Name"/>
				</td>
			</tr>
			-->
	<!--<xsl:for-each select="ArrayOfScriptStatusData/ScriptStatusData">
				<tr>
					<td>
						Script:
						<xsl:value-of select="ScriptName"/>
					</td>
				</tr>

			</xsl:for-each>-->
	<!--
		</xsl:for-each>
		-->
	<!--</xsl:for-each>-->
	<!--

	</xsl:template>-->


</xsl:stylesheet>
