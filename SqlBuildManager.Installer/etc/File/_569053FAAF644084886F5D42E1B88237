<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes" omit-xml-declaration="yes" />

    <xsl:template match="/SqlBuildManagerChangeNotes">
		<html>
			<head>
				<style type="text/css">
					body
					{
					font-family:Calibri, Verdana, Arial;
					font-size: 10pt;
					color: black;
					}
					.added
					{
					font-size: 10pt;
					color: green;
					}
					.fixed
					{
					font-size: 10pt;
					color: red;
					}
					.updated
					{
					font-size: 10pt;
					color: orange;
					}
					.note
					{
					font-size: 10pt;
					color: blue;
					}
					.version
					{
					font-size: 11pt;
					color: dark blue;
					font-weight: bold;
					}


				</style>
			</head>
			<body>
				<xsl:for-each select="Version">
					
					<div class="version">Version <xsl:value-of select="@Major"/>.<xsl:value-of select="@Minor"/>.<xsl:value-of select="@Revision"/></div>
          <xsl:for-each select="child::*">
            <xsl:choose>
              <xsl:when test="name() = 'note'">
                <div>
                  <span class="note">NOTE: </span><xsl:value-of select="."/>
                </div>
              </xsl:when>
              <xsl:when test="name() = 'added'">
                <div>
                  <span class="added">ADDED: </span>
                  <xsl:value-of select="."/>
                </div>
              </xsl:when>
              <xsl:when test="name() = 'updated'">
                <div>
                  <span class="updated">UPDATED: </span>
                  <xsl:value-of select="."/>
                </div>
              </xsl:when>
              <xsl:when test="name() = 'fixed'">
                <div>
                  <span class="fixed">FIXED: </span>
                  <xsl:value-of select="."/>
                </div>
              </xsl:when>
              
            </xsl:choose>
            
          </xsl:for-each>					
          <!--<xsl:for-each select="note">
						<div>
							<span class="note">NOTE: </span>
							<xsl:value-of select="."/>
						</div>
					</xsl:for-each>
					
					<xsl:for-each select="added">
						<div><span class="added">ADDED: </span><xsl:value-of select="."/></div>
					</xsl:for-each>
					<xsl:for-each select="updated">
						<div>
							<span class="updated">UPDATED: </span>
							<xsl:value-of select="."/>
						</div>
					</xsl:for-each>
					<xsl:for-each select="fixed">
						<div>
							<span class="fixed">FIXED: </span>
							<xsl:value-of select="."/>
						</div>
					</xsl:for-each>-->
					<br/>
				</xsl:for-each>
			</body>
		</html>
    </xsl:template>
</xsl:stylesheet>
