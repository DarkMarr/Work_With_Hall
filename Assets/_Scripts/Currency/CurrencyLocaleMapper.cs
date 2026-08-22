using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Purchasing;

public static class CurrencyLocaleHelper
{
    private static readonly Dictionary<string, string> CurrencyToLocale = new()
    {
        // Major Global Currencies
        { "USD", "en-US" }, // United States Dollar
        { "JPY", "ja-JP" }, // Japanese Yen
        { "EUR", "fr-FR" }, // Euro (picked French locale as a common example)
        { "GBP", "en-GB" }, // British Pound Sterling
        { "CNY", "zh-CN" }, // Chinese Yuan Renminbi (Mainland China)
        { "KRW", "ko-KR" }, // South Korean Won
        { "AUD", "en-AU" }, // Australian Dollar
        { "CAD", "en-CA" }, // Canadian Dollar
        { "CHF", "de-CH" }, // Swiss Franc (picked German Switzerland)
        { "HKD", "zh-HK" }, // Hong Kong Dollar (picked Cantonese/Traditional Chinese)
        { "SGD", "en-SG" }, // Singapore Dollar (English is common)

        // Other Common Currencies / Large Economies
        { "BRL", "pt-BR" }, // Brazilian Real
        { "INR", "en-IN" }, // Indian Rupee (English is common)
        { "RUB", "ru-RU" }, // Russian Ruble
        { "MXN", "es-MX" }, // Mexican Peso
        { "ZAR", "en-ZA" }, // South African Rand (English is common)
        { "TRY", "tr-TR" }, // Turkish Lira
        { "PLN", "pl-PL" }, // Polish Zloty
        { "SEK", "sv-SE" }, // Swedish Krona
        { "NOK", "nb-NO" }, // Norwegian Krone
        { "DKK", "da-DK" }, // Danish Krone
        { "NZD", "en-NZ" }, // New Zealand Dollar
        { "ILS", "he-IL" }, // Israeli New Shekel
        { "SAR", "ar-SA" }, // Saudi Riyal
        { "AED", "ar-AE" }, // UAE Dirham
        { "CLP", "es-CL" }, // Chilean Peso
        { "COP", "es-CO" }, // Colombian Peso
        { "ARS", "es-AR" }, // Argentine Peso
        { "PEN", "es-PE" }, // Peruvian Sol
        { "EGP", "ar-EG" }, // Egyptian Pound
        { "IDR", "id-ID" }, // Indonesian Rupiah
        { "MYR", "ms-MY" }, // Malaysian Ringgit
        { "PHP", "en-PH" }, // Philippine Peso (English is common)
        { "THB", "th-TH" }, // Thai Baht
        { "VND", "vi-VN" }, // Vietnamese Dong

        // Further Additions for broader coverage:
        { "CZK", "cs-CZ" }, // Czech Koruna
        { "HUF", "hu-HU" }, // Hungarian Forint
        { "RON", "ro-RO" }, // Romanian Leu
        { "BDT", "bn-BD" }, // Bangladeshi Taka
        { "PKR", "en-PK" }, // Pakistani Rupee
        { "UAH", "uk-UA" }, // Ukrainian Hryvnia
        { "KZT", "kk-KZ" }, // Kazakhstani Tenge (Kazakhstan)
        { "AZN", "az-Latn-AZ" }, // Azerbaijani Manat
        { "GEL", "ka-GE" }, // Georgian Lari
        { "IQD", "ar-IQ" }, // Iraqi Dinar
        { "JOD", "ar-JO" }, // Jordanian Dinar
        { "KWD", "ar-KW" }, // Kuwaiti Dinar
        { "OMR", "ar-OM" }, // Omani Rial
        { "QAR", "ar-QA" }, // Qatari Riyal
        { "BHD", "ar-BH" }, // Bahraini Dinar
    };

    private static Dictionary<string, CultureInfo> currencyCodeToCultureInfo = new Dictionary<string, CultureInfo>();

    public static CultureInfo GetCultureInfoFromCurrency(string isoCurrencyCode)
    {
        if (currencyCodeToCultureInfo.TryGetValue(isoCurrencyCode, out var cultureInfo))
            return cultureInfo;

        if (CurrencyToLocale.TryGetValue(isoCurrencyCode, out var locale))
        {
            cultureInfo = new CultureInfo(locale);
            currencyCodeToCultureInfo.Add(isoCurrencyCode, cultureInfo);
            return cultureInfo;
        }

        return new CultureInfo("en-US");
    }

    public static string GetLocaleFromCurrency(string isoCurrencyCode)
    {
        if (CurrencyToLocale.TryGetValue(isoCurrencyCode, out var locale))
            return locale;

        return "en-US";
    }

    public static string FormatCurrency(string isoCurrencyCode, decimal amount)
    {
        var cultureInfo = GetCultureInfoFromCurrency(isoCurrencyCode);
        return string.Format(cultureInfo, "{0:C}", amount);
    }

    public static string FormatCurrencyForProduct(Product product)
    {
        var cultureInfo = GetCultureInfoFromCurrency(product.metadata.isoCurrencyCode);
        return string.Format(cultureInfo, "{0:C}", product.metadata.localizedPrice);
    }
}
