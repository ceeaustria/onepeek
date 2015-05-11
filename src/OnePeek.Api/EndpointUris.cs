﻿using System;
using System.Net;

namespace OnePeek.Api
{
  internal static class EndpointUris
  {
    public const string WINDOWSPHONE_IMAGE_URI = "http://cdn.marketplaceimages.windowsphone.com/v8/images/{0}";

    public const string WINDOWSPHONE_METADATA_URI = "http://marketplaceedgeservice.windowsphone.com/v9/catalog/apps/{0}?os=8.10.14219.0&cc={1}&lang={2}";

    public const string WINDOWSPHONE_REVIEWS_URI = "http://marketplaceedgeservice.windowsphone.com/v9/ratings/product/{0}/reviews?os=8.10.14219.0&cc={1}&lang={2}&dm=RM-1045_1012&chunksize=20&skuId=c1424839-be1e-40eb-8bc3-b2730db30b62&orderBy={3}";

    public const string WINDOWSPHONE_SEARCH_URI = "http://marketplaceedgeservice.windowsphone.com/v9/catalog/apps?os=8.10.14219.0&cc={0}&lang={1}&dm=Virtual&chunkSize=50&q={2}";

    public const string WINDOWSPHONE_SPOTLIGHT_URI = "http://cdn.marketplaceedgeservice.windowsphone.com/v9/catalog/hubs?os=8.10.14219.0&cc={0}&lang={1}&hw=520293381&dm=RM-1045_1012&oemId=NOKIA&moId=HUT-AT&hub={2}&cf=99-1";



    internal static Uri GetWindowsPhoneMetadataUri(string appId, string culture)
    {
      culture = culture.Replace('_', '-');
      string country = culture.Split('-')[1];
      return Uri(WINDOWSPHONE_METADATA_URI, appId, country, culture);
    }



    internal static Uri GetWindowsPhoneImageUri(string urn, string type)
    {
      return Uri(WINDOWSPHONE_IMAGE_URI, urn.Replace("urn:uuid:", "") + (!String.IsNullOrWhiteSpace(type) ? "?imageType=" + type : ""));
    }



    internal static Uri GetWindowsPhoneReviewsUri(string appId, string culture, string orderBy, string prevPageMarkerId = null, string nextPageMarkerId = null)
    {
      culture = culture.Replace('_', '-');
      string country = culture.Split('-')[1];
      string affix = String.Empty;

      if (!String.IsNullOrWhiteSpace(prevPageMarkerId))
      {
        affix = "&beforeMarker=" + prevPageMarkerId;
      }
      else if (!String.IsNullOrWhiteSpace(nextPageMarkerId))
      {
        affix = "&afterMarker=" + nextPageMarkerId;
      }

      return Uri(WINDOWSPHONE_REVIEWS_URI + affix, appId, country, culture, orderBy);
    }



    internal static Uri GetWindowsPhoneSearchUri(string query, string culture)
    {
      culture = culture.Replace('_', '-');
      string country = culture.Split('-')[1];
      return Uri(WINDOWSPHONE_SEARCH_URI, country, culture, WebUtility.HtmlEncode(query));
    }



    internal static Uri GetWindowsPhoneSpotlightUri(string culture, string type)
    {
      // type should be games or apps

      culture = culture.Replace('_', '-');
      string country = culture.Split('-')[1];
      return Uri(WINDOWSPHONE_SPOTLIGHT_URI, country, culture, type);
    }



    private static Uri Uri(string template, params string[] replacements)
    {
      string uriString = String.Format(template, replacements);
      return new Uri(uriString, UriKind.Absolute);
    }
  }
}
