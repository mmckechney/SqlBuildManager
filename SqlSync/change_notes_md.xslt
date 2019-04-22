<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="text" indent="no" omit-xml-declaration="yes" />

    <xsl:template match="/SqlBuildManagerChangeNotes">
## SQL Build Manager Change Notes
      <xsl:for-each select="Version">
					
### Version <xsl:value-of select="@Major"/>.<xsl:value-of select="@Minor"/>.<xsl:value-of select="@Revision"/>
<xsl:for-each select="child::*">
<xsl:choose>
<xsl:when test="name() = 'note'">
- **NOTE:** <xsl:value-of select="."/>
  
  
</xsl:when>
<xsl:when test="name() = 'added'">
- *ADDED:* <xsl:value-of select="."/>
  
  
</xsl:when>
<xsl:when test="name() = 'updated'">
- *UPDATED:* <xsl:value-of select="."/>
  

</xsl:when>
<xsl:when test="name() = 'fixed'">
- *FIXED:* <xsl:value-of select="."/>
  
  
</xsl:when>
</xsl:choose>
            
          </xsl:for-each>					
				</xsl:for-each>
    </xsl:template>
</xsl:stylesheet>
