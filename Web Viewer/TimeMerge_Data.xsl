<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet xmlns="http://www.w3.org/1999/xhtml"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  <!-- Root element is the whole Month -->
  <xsl:template match="SingleMonthData">
    <html>
      <head>
        <link rel="stylesheet" type="text/css" href="TimeMerge_Data.css"/>
        <title><xsl:value-of select="/SingleMonthData/CachedTitleOfMonth" /></title>
      </head>
      <body>
        <div class="layoutRoot">
		  <table>
		  <thead>
		    <tr>
			  <td />
			  <td colspan="11"><h2><xsl:value-of select="/SingleMonthData/CachedBalanceOfMonth" /></h2></td>
			  <td colspan="8"><h2><xsl:value-of select="/SingleMonthData/CachedTitleOfMonth" /></h2></td>
			</tr>
		  </thead>
          <xsl:for-each select="/SingleMonthData/Days/SingleDayData">
			<xsl:choose>
			  <xsl:when test="IsNoWorkDay = 'true'">
			    <tr class="noWorkDay">
				  <td class="noWorkDay dayLabel"><xsl:value-of select="Day" /></td>
				  <td class="balanceOfDay"><xsl:call-template name="balanceOfDay" /></td>
				  <xsl:apply-templates />
				</tr>
			  </xsl:when>
			  <xsl:otherwise>
			    <tr>
			      <td class="dayLabel"><xsl:value-of select="Day" /></td>
				  <td class="balanceOfDay"><xsl:call-template name="balanceOfDay" /></td>
				  <xsl:apply-templates />
			    </tr>
			  </xsl:otherwise>
			</xsl:choose>
          </xsl:for-each>
		  </table>
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="balanceOfDay">
    <xsl:if test="CachedBalanceOfDay != '00:00'">
	  <xsl:value-of select="CachedBalanceOfDay" />
    </xsl:if>
  </xsl:template>
  
  <xsl:template name="startTime">
    <xsl:param name="time" />
	&#160;&#160;&#160; <!-- &nbsp; -->
    <xsl:value-of select="substring(substring-after($time, 'T'), 1,5)" />
    -
  </xsl:template>
  
  <xsl:template name="endTime">
    <xsl:param name="time" />
	<xsl:value-of select="substring(substring-after($time, 'T'), 1,5)" />
	&#160;
  </xsl:template>

  <xsl:template name="startTimeWithCorrections">
    <xsl:param name="correctionTime" />
    <xsl:param name="time" />
	  <xsl:choose>
	    <xsl:when test="$correctionTime != '' and substring-after($correctionTime, 'T') != '00:00:00'">
		  <xsl:call-template name="startTime"><xsl:with-param name="time" select="$correctionTime" /></xsl:call-template>
		</xsl:when>
	    <xsl:otherwise>
		  <xsl:if test="substring-after($time, 'T') != '00:00:00'">
		    <xsl:call-template name="startTime"><xsl:with-param name="time" select="$time" /></xsl:call-template>
		  </xsl:if>
		</xsl:otherwise>
	  </xsl:choose>
  </xsl:template>  
  
  <xsl:template name="endTimeWithCorrections">
    <xsl:param name="correctionTime" />
    <xsl:param name="time" />
	  <xsl:choose>
	    <xsl:when test="$correctionTime != '' and substring-after($correctionTime, 'T') != '00:00:00'">
		  <xsl:call-template name="endTime"><xsl:with-param name="time" select="$correctionTime" /></xsl:call-template>
		</xsl:when>
	    <xsl:otherwise>
		  <xsl:if test="substring-after($time, 'T') != '00:00:00'">
		    <xsl:call-template name="endTime"><xsl:with-param name="time" select="$time" /></xsl:call-template>
		  </xsl:if>
		</xsl:otherwise>
	  </xsl:choose>
  </xsl:template>  

  <xsl:template name="interruptTypeCode">
    <xsl:param name="type" />
	<xsl:choose>
	  <xsl:when test="$type = 'OTHER'">-</xsl:when>
	  <xsl:when test="$type = 'OBED'" >O</xsl:when>
	  <xsl:when test="$type = 'PDOMA'">P</xsl:when>
	  <xsl:when test="$type = 'DOV'"  >D</xsl:when>
	  <xsl:when test="$type = 'LEK'"  >L</xsl:when>
	  <xsl:when test="$type = 'SLUZ'" >S</xsl:when>
	  <xsl:when test="$type = 'ZP'"   >Z</xsl:when>
	  <xsl:when test="$type = 'OCR'"  >OČR</xsl:when>
	  <xsl:when test="$type = 'PN'"   >P</xsl:when>
	</xsl:choose>
  </xsl:template>
  
  <xsl:template name="typeWithCorrection">
    <xsl:param name="time" />
    <xsl:param name="correctionTime" />
    <xsl:param name="type" />
    <xsl:param name="correctedType" />
    <xsl:param name="cachedBalanceOfDay" />
    <xsl:choose>
	  <xsl:when test="($cachedBalanceOfDay != '00:00') and ($correctedType != '') and ($correctedType != 'OTHER') and ($time = '' or substring-after($time, 'T') = '00:00:00') and ($correctionTime = '' or substring-after($correctionTime, 'T') = '00:00:00')">
	    &#160;&#160;&#160;&#160;&#160;&#160;
	    <xsl:call-template name="interruptTypeCode">
		  <xsl:with-param name="type" select="$correctedType" />
		</xsl:call-template>
	  </xsl:when>
	  <xsl:when test="($cachedBalanceOfDay != '00:00') and ($type != 'OTHER') and ($time = '' or substring-after($time, 'T') = '00:00:00') and ($correctionTime = '' or substring-after($correctionTime, 'T') = '00:00:00')">
	    &#160;&#160;&#160;&#160;&#160;&#160;
	    <xsl:call-template name="interruptTypeCode">
		  <xsl:with-param name="type" select="$type" />
		</xsl:call-template>
	  </xsl:when>
	  <xsl:when test="($time = '' or substring-after($time, 'T') = '00:00:00') and ($correctionTime = '' or substring-after($correctionTime, 'T') = '00:00:00')" />
	  <xsl:when test="$correctedType != ''">
	    <xsl:call-template name="interruptTypeCode">
		  <xsl:with-param name="type" select="$correctedType" />
		</xsl:call-template>
  	  </xsl:when>
	  <xsl:otherwise>
	    <xsl:call-template name="interruptTypeCode">
		  <xsl:with-param name="type" select="$type" />
		</xsl:call-template>
	  </xsl:otherwise>
    </xsl:choose>
  </xsl:template>  
  
  <xsl:template match="WorkSpans/WorkSpan">
    <td>
	  <xsl:call-template name="startTimeWithCorrections">
	    <xsl:with-param name="correctionTime" select="CorrectionStartTime" />
	    <xsl:with-param name="time" select="StartTime" />
	  </xsl:call-template>
	  <xsl:call-template name="endTimeWithCorrections">
	    <xsl:with-param name="correctionTime" select="CorrectionEndTime" />
	    <xsl:with-param name="time" select="EndTime" />
	  </xsl:call-template>
	</td>
  </xsl:template>

  <xsl:template match="WorkInterruptions/WorkInterruption">
    <td class="workInterruption">
	  <xsl:call-template name="startTimeWithCorrections">
	    <xsl:with-param name="correctionTime" select="CorrectionStartTime" />
	    <xsl:with-param name="time" select="StartTime" />
	  </xsl:call-template>
	  <xsl:call-template name="endTimeWithCorrections">
	    <xsl:with-param name="correctionTime" select="CorrectionEndTime" />
	    <xsl:with-param name="time" select="EndTime" />
	  </xsl:call-template>
	  <xsl:call-template name="typeWithCorrection">
	    <xsl:with-param name="time" select="StartTime" />
	    <xsl:with-param name="correctionTime" select="CorrectionStartTime" />
	    <xsl:with-param name="type" select="Type" />
	    <xsl:with-param name="correctedType" select="CorrectedType" />
	    <xsl:with-param name="cachedBalanceOfDay" select="../../CachedBalanceOfDay" />
	  </xsl:call-template>
	</td>
  </xsl:template>

  <xsl:template match="SingleDayData/Day" />
  <xsl:template match="SingleDayData/CachedBalanceOfDay" />
  <xsl:template match="SingleDayData/IsNoWorkDay" />
  <xsl:template match="SingleDayData/NotesContent" />

</xsl:stylesheet>
