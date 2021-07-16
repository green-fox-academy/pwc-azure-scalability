<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="Számlák">
    <Invoices>
      <xsl:apply-templates select="@*|node()" />
    </Invoices>
  </xsl:template>

  <xsl:template match="Számla">
    <Invoice>
      <xsl:apply-templates select="@*|node()" />
    </Invoice>
  </xsl:template>

  <xsl:template match="Tételek">
    <LineItems>
      <xsl:apply-templates select="@*|node()" />
    </LineItems>
  </xsl:template>

  <xsl:template match="Tétel">
    <InvoiceLineItem>
      <xsl:apply-templates select="@*|node()" />
    </InvoiceLineItem>
  </xsl:template>

  <xsl:template match="@Megnevezés">
    <xsl:attribute name="Name">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="@Mennyiség">
    <xsl:attribute name="Quantity">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="@Egységár">
    <xsl:attribute name="UnitAmount">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="@Számlaszám">
    <xsl:attribute name="InvoiceNumber">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="@Kibocsátó">
    <xsl:attribute name="ContactName">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="@Kibocsátó">
    <xsl:attribute name="ContactName">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="@Végösszeg">
    <xsl:attribute name="TotalAmount">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

</xsl:stylesheet>
