<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" >
  <xsl:output method="text" indent="yes" />
<xsl:template match="/Package">Script Name, Guid, Last Change Date, Last Change User Id, Violation, Message, Severity
<xsl:for-each select="Script/Violations/Violation">
<xsl:value-of select="../../@ScriptName"/>,<xsl:value-of select="../../@Guid"/>,<xsl:value-of select="../../@LastChangeDate"/>,<xsl:value-of select="../../@LastChangeUserId"/>,<xsl:value-of select="@Name"/>,<xsl:value-of select="@Message"/>,<xsl:value-of select="@Severity"/>
<xsl:text>
</xsl:text>
      </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
