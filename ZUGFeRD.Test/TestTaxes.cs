﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace s2industries.ZUGFeRD.Test;

[TestClass]
public class TestTaxes
{
    [TestMethod]
    public void SavingThenReadingAppliedTradeTaxesShouldWork()
    {
        InvoiceDescriptor expected = InvoiceDescriptor.CreateInvoice("471102", new DateTime(2013, 6, 5), CurrencyCodes.EUR, "GE2020211-471102");
        expected.BusinessProcess = string.Empty;
        expected.Name = string.Empty;
        expected.ReferenceOrderNo = string.Empty;
        var lineItem = expected.AddTradeLineItem(name: "Something",
            grossUnitPrice: 9.9m,
            netUnitPrice: 9.9m,
            billedQuantity: 20m,
            taxType: TaxTypes.VAT,
            categoryCode: TaxCategoryCodes.S,
            taxPercent: 19m
            );
        lineItem.LineTotalAmount = 198m; // 20 * 9.9
        expected.AddApplicableTradeTax(
            basisAmount: lineItem.LineTotalAmount!.Value,
            percent: 19m,
            taxAmount: 29.82m, // 19% of 198
            typeCode: TaxTypes.VAT,
            categoryCode: TaxCategoryCodes.S,
            allowanceChargeBasisAmount: -5m
            );
        expected.LineTotalAmount = 198m;
        expected.TaxBasisAmount = 198m;
        expected.TaxTotalAmount = 29.82m;
        expected.GrandTotalAmount = 198m + 29.82m;
        expected.DuePayableAmount = expected.GrandTotalAmount;

        using MemoryStream ms = new();
        expected.Save(ms);
        ms.Seek(0, SeekOrigin.Begin);

        InvoiceDescriptor actual = InvoiceDescriptor.Load(ms);

        Assert.AreEqual(expected.Taxes.Count, actual.Taxes.Count);
        Assert.AreEqual(1, actual.Taxes.Count);
        Tax actualTax = actual.Taxes[0];
        Assert.AreEqual(198m, actualTax.BasisAmount);
        Assert.AreEqual(19m, actualTax.Percent);
        Assert.AreEqual(29.82m, actualTax.TaxAmount);
        Assert.AreEqual(TaxTypes.VAT, actualTax.TypeCode);
        Assert.AreEqual(TaxCategoryCodes.S, actualTax.CategoryCode);
        Assert.AreEqual(-5m, actualTax.AllowanceChargeBasisAmount);
    }
}
