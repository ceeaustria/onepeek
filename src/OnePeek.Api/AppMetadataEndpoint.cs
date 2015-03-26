﻿using Bender;
using OnePeek.Entities;
using OnePeek.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bender.Configuration;
using System.IO;

namespace OnePeek.Api
{
  public class AppMetadataEndpoint : ApiBase
  {
    public async Task<AppMetadata> GetMetadata(string appId, StoreType store, StoreCultureType storeCulture)
    {
      if (storeCulture == StoreCultureType.Unknown)
      {
        throw new ArgumentException("Please provide a valid store culture");
      }

      string xml = await ApiHttpClient.Instance.Get(
        EndpointUris.GetWindowsPhoneMetadataUri(appId, storeCulture.ToString())
      );

      try
      {
        AppMetadata result = Deserialize.Xml<AppMetadata>(xml);
        result.Id = appId;
        return result;
      }
      catch (Exception exc)
      {
        Debug.WriteLine(exc.Message);
      }

      return null;
    }


    public Uri GetImageUri(string urn, StoreScreenshotType screenshotType)
    {
      string type = screenshotType.GetEnumDisplayName();
      return EndpointUris.GetWindowsPhoneImageUri(urn, type);
    }


    public async Task<Stream> GetImageAsStream(string urn, StoreScreenshotType screenshotType)
    {
      string type = screenshotType.GetEnumDisplayName();
      Uri uri = EndpointUris.GetWindowsPhoneImageUri(urn, type);
      return await ApiHttpClient.Instance.GetStream(uri);
    }
  }
}
