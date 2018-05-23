<?xml version="1.0" encoding="UTF-8"?>
<!-- #############################################################
    # Name:        UpdSemGuid.xsl
    # Purpose:     Apply Semantic guid to all entries Semantic FIeld in .fwdata
    #
    # Author:      Greg Trihus <greg_trihus@sil.org>
    #
    # Created:     2016/03/24
    # Copyright:   (c) 2016 SIL International
    # Licence:     <MIT>
    ################################################################-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="1.0">

    <xsl:param name="mapUrl">semMap.xml</xsl:param>
    <xsl:variable name="map" select="document($mapUrl)//map"/>
    <xsl:output method="xml"/>

    <!-- Recursive copy -->
    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="/">
        <xsl:text>&#10;</xsl:text>
        <xsl:apply-templates select="*"/>
    </xsl:template>

    <xsl:template match="*[*/@name='Semantic Field']">
        <xsl:variable name="field" select="*[@name='Semantic Field']/AUni"/>
        <xsl:choose>
            <xsl:when test="$map/field = $field">
                <xsl:copy>
                    <xsl:apply-templates select="@*"/>
                    <xsl:apply-templates select="node()[local-name() != 'SemanticDomains']"/>
                    <xsl:element name="SemanticDomains">
                        <xsl:text>&#10;</xsl:text>
                        <xsl:for-each select="$map[field=$field]/objsur">
                            <xsl:copy-of select="."/>
                            <xsl:text>&#10;</xsl:text>
                        </xsl:for-each>
                    </xsl:element>
                    <xsl:text>&#10;</xsl:text>
                </xsl:copy>
            </xsl:when>
            <xsl:otherwise>
                <xsl:copy>
                    <xsl:apply-templates select="node()|@*"/>
                </xsl:copy>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
</xsl:stylesheet>