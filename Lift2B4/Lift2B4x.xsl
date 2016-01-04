<?xml version="1.0" encoding="UTF-8"?>
<!-- #############################################################
    # Name:        Lift2B4x.xsl
    # Purpose:     Convert Filter lift to Byki 4 xml
    #
    # Author:      Greg Trihus <greg_trihus@sil.org>
    #
    # Created:     2015/10/16
    # Copyright:   (c) 2015 SIL International
    # Licence:     <MIT>
    ################################################################-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns:palaso="urn://palaso.org/ldmlExtensions/v1" version="1.0">

    <xsl:param name="WritingSystemsFolder"/>
    <xsl:param name="UnitTitle">Domain Categories</xsl:param>
    <xsl:param name="Unit">01</xsl:param>
    <xsl:param name="Lesson">01</xsl:param>
    <xsl:param name="Category">Animals Birds</xsl:param>
    <xsl:param name="FieldType">Semantic Field</xsl:param>
    <xsl:param name="ListLimit" select="10"/>
    <xsl:param name="Date"/>

    <xsl:output indent="yes"/>

    <xsl:template match="lift">
        <xsl:element name="list">
            <xsl:attribute name="xsi:noNamespaceSchemaLocation">byki_schema.xsd</xsl:attribute>
            <xsl:attribute name="formatVersion">2</xsl:attribute>
            <xsl:element name="head">
                <xsl:element name="name">
                    <xsl:value-of select="$UnitTitle"/>
                    <xsl:text>: Unit </xsl:text>
                    <xsl:value-of select="$Unit"/>
                    <xsl:text>, Lesson </xsl:text>
                    <xsl:value-of select="$Lesson"/>
                </xsl:element>
                <xsl:element name="side1_language_key">
                    <xsl:call-template name="LookupLanguageName">
                        <xsl:with-param name="code"
                            select="//entry[1]//definition[1]//form[1]/@lang"/>
                    </xsl:call-template>
                </xsl:element>
                <xsl:element name="side2_language_key">
                    <xsl:call-template name="LookupLanguageName">
                        <xsl:with-param name="code" select="//entry[1]//citation[1]/form[1]/@lang"/>
                    </xsl:call-template>
                </xsl:element>
                <xsl:element name="description">
                    <xsl:value-of select="$Category"/>
                    <xsl:text> words</xsl:text>
                </xsl:element>
                <xsl:element name="creator">Copyright SIL International</xsl:element>
                <xsl:element name="creator_url">www.sil.org</xsl:element>
                <xsl:element name="creator_application">Lift2B4x.xsl</xsl:element>
                <xsl:element name="subject_name">
                    <xsl:value-of select="$Category"/>
                </xsl:element>
                <xsl:element name="side1_title"/>
                <xsl:element name="side2_title"/>
                <xsl:element name="creation_date">
                    <xsl:value-of select="$Date"/>
                    <!--xsl:value-of select="format-dateTime(current-dateTime(),'[Y0001]-[M01]-[D01]T[H01]:[m01]:[s01]')"></xsl:value-of-->
                </xsl:element>
                <xsl:element name="is_themed">false</xsl:element>
                <xsl:element name="is_government">false</xsl:element>
                <xsl:element name="is_library">false</xsl:element>
                <xsl:element name="is_alternative_side2_present">false</xsl:element>
                <xsl:element name="is_ordered">true</xsl:element>
                <xsl:element name="is_transliteration_shown">true</xsl:element>
                <xsl:element name="ListProtection">
                    <xsl:element name="is_Protected_Editing">false</xsl:element>
                    <xsl:element name="is_Protected_Copying">false</xsl:element>
                    <xsl:element name="is_Protected_Sharing">false</xsl:element>
                </xsl:element>
                <xsl:element name="LearningFeatures">
                    <xsl:element name="is_PreviewIt">true</xsl:element>
                    <xsl:element name="is_SR_RecognizeIt">true</xsl:element>
                    <xsl:element name="is_Written_RecognizeIt">true</xsl:element>
                    <xsl:element name="is_SR_ProduceIt">true</xsl:element>
                    <xsl:element name="is_Written_ProduceIt">true</xsl:element>
                </xsl:element>
            </xsl:element>
            <xsl:element name="cards">
                <xsl:apply-templates/>
            </xsl:element>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="field">
        <xsl:if test="@type=$FieldType and .//*[.=$Category] and ancestor::sense//*[contains(@lang,'-audio')]">
            <xsl:variable name="listPos" select="count(preceding::*[@type=$FieldType and .//*[.=$Category]][ancestor::sense//*[contains(@lang,'-audio')]]) * 2 + 1"/>
            <xsl:if test="$listPos &lt; $ListLimit">
                <xsl:element name="card">
                    <xsl:element name="side1_phrase">
                        <xsl:value-of select="parent::*/definition//text[1]"/>
                    </xsl:element>
                    <xsl:element name="side2_phrase">
                        <xsl:value-of select="ancestor::entry//citation//form[1]"/>
                    </xsl:element>
                    <xsl:element name="guid">
                        <xsl:text>{</xsl:text>
                        <xsl:value-of select="parent::*/@id"/>
                        <xsl:text>}</xsl:text>
                    </xsl:element>
                    <xsl:element name="list_position">
                        <xsl:value-of select="$listPos"/>
                    </xsl:element>
                    <xsl:element name="side2_transliteration">
                        <xsl:value-of select="ancestor::entry/lexical-unit//text"/>
                    </xsl:element>
                    <xsl:element name="is_video_sound_used">false</xsl:element>
                    <xsl:element name="is_video_auto_played">false</xsl:element>
                </xsl:element>
                <xsl:for-each select="parent::*/example[*[contains(@lang,'-audio')]][1]">
                    <xsl:element name="card">
                        <xsl:element name="side1_phrase">
                            <xsl:value-of select="translation//text[1]"/>
                        </xsl:element>
                        <xsl:element name="side2_phrase">
                            <xsl:value-of select="form[1]"/>
                        </xsl:element>
                        <xsl:element name="list_position">
                            <xsl:value-of select="$listPos + 1"/>
                        </xsl:element>
                        <xsl:if test="form[2]">
                            <xsl:element name="side2_transliteration">
                                <xsl:value-of select="form[2]"/>
                            </xsl:element>
                        </xsl:if>
                        <xsl:element name="side2_sound">
                            <xsl:attribute name="url">
                                <xsl:text>sounds/</xsl:text>
                                <xsl:value-of select="form[contains(@lang, '-audio')]/text"/>
                            </xsl:attribute>
                        </xsl:element>
                        <xsl:element name="is_video_sound_used">false</xsl:element>
                        <xsl:element name="is_video_auto_played">false</xsl:element>
                    </xsl:element>
                </xsl:for-each>
            </xsl:if>
        </xsl:if>
    </xsl:template>

    <xsl:template name="LookupLanguageName">
        <xsl:param name="code"/>
        <xsl:call-template name="MakeUc">
            <xsl:with-param name="value" select="document(concat($WritingSystemsFolder, $code, '.ldml'))//palaso:languageName/@value"/>
        </xsl:call-template>
    </xsl:template>
    
    <xsl:template name="MakeUc">
        <xsl:param name="value"/>
        <xsl:value-of select="translate($value, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/>
    </xsl:template>
        
    <xsl:template match="text()"/>
</xsl:stylesheet>
