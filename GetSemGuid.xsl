<?xml version="1.0" encoding="UTF-8"?>
<!-- #############################################################
    # Name:        GetSemGuid.xsl
    # Purpose:     Get Guid for Semantic Field from .fwdata
    #
    # Author:      Greg Trihus <greg_trihus@sil.org>
    #
    # Created:     2016/03/24
    # Copyright:   (c) 2016 SIL International
    # Licence:     <MIT>
    ################################################################-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="1.0">

    <xsl:output method="xml" indent="yes"/>

    <!-- Recursive traversal -->
    <xsl:template match="node()|@*">
        <xsl:apply-templates select="node()|@*"/>
    </xsl:template>

    <xsl:template match="/">
        <xsl:element name="root">
            <xsl:apply-templates/>
        </xsl:element>
    </xsl:template>

    <xsl:template match="*[*/@name='Semantic Field' and *[local-name(.)='SemanticDomains']]">
        <xsl:element name="map">
            <xsl:element name="field">
                <xsl:value-of select="*[@name='Semantic Field']/AUni"/>
            </xsl:element>
            <xsl:copy-of select="SemanticDomains/*"/>
        </xsl:element>
    </xsl:template>
</xsl:stylesheet>