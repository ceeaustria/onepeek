﻿using Bender;
using OnePeek.Api.Extensions;
using OnePeek.Entities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace OnePeek.Api
{
  public class OnePeekApi : ApiBase, IOnePeekApi
  {
    /// <summary>
    /// Searches for apps based on a term (app name, keywords, ..)
    /// </summary>
    /// <param name="searchTerm">Search termn, is typically an app name or keyword (as used in the store interface).</param>
    /// <param name="store">The store where the app is published.</param>
    /// <param name="storeCulture">Culture of the query (returns location specific metadata + ratings).</param>
    /// <returns></returns>
    public async Task<StoreSearchResults> Search(string searchTerm, StoreType store, StoreCultureType storeCulture)
    {
      if (storeCulture == StoreCultureType.Unknown || storeCulture == StoreCultureType.All)
      {
        throw new ArgumentException("Please provide a valid store culture");
      }

      if (String.IsNullOrWhiteSpace(searchTerm))
      {
        throw new ArgumentException("Search term has to contain at least a char.");
      }

      Uri uri = EndpointUris.GetWindowsPhoneSearchUri(searchTerm.Trim(), storeCulture.ToString());

      string xml = await ApiHttpClient.Instance.Get(uri);

      IEnumerable<XElement> xel = XDocument.Parse(xml).Elements().First().Descendants();

      // create metadatas
      StoreSearchResults result = new StoreSearchResults();
      result.StoreType = store;
      result.StoreCultureType = storeCulture;
      result.Results = xel.Where(x => x.Name.LocalName == "entry").Select(x =>
      {
        IEnumerable<XElement> childs = x.Descendants();

        float rating = childs.GetFloat("averageUserRating");

        if (Configuration.UseFiveStarSystem)
        {
          rating = rating * 0.5f;
        }

        AppMetadata data = new AppMetadata()
        {
          Id = childs.Get("id").Split(':').Last(),
          Urn = childs.Get("id"),
          Name = childs.Get("title"),
          Rating = new AppRating()
          {
            AverageRating = rating,
            RatingCount = Convert.ToInt32(childs.Get("userRatingCount"))
          },
          Images = new AppMetadataImages()
          {
            Logo = new AppImage()
            {
              Rotation = 0,
              Urn = childs.Get("image")
            }
          }
        };

        return data;
      });
      result.Count = result.Results.Count();

      return result;
    }


    /// <summary>
    /// Get spotlight entries for the current day in the specified culture
    /// </summary>
    /// <param name="spotlightType">Can either be apps or games. Both return approx. 20 new results per day.</param>
    /// <param name="store">The store where the app is published.</param>
    /// <param name="storeCulture">Culture of the query (returns location specific metadata + ratings).</param>
    /// <returns></returns>
    public async Task<StoreSpotlightResults> GetSpotlight(StoreSpotlightType spotlightType, StoreType store, StoreCultureType storeCulture)
    {
      if (storeCulture == StoreCultureType.Unknown || storeCulture == StoreCultureType.All)
      {
        throw new ArgumentException("Please provide a valid store culture");
      }

      Uri uri = EndpointUris.GetWindowsPhoneSpotlightUri(storeCulture.ToString(), spotlightType.GetEnumDisplayName());

      string xml = await ApiHttpClient.Instance.Get(uri);

      IEnumerable<XElement> xel = XDocument.Parse(xml).Elements().First().Descendants();

      StoreSpotlightResults result = new StoreSpotlightResults();
      result.StoreCultureType = storeCulture;
      result.StoreSpotlightType = spotlightType;
      result.StoreType = store;
      result.Results = xel.Where(x => x.Name.LocalName == "application").Select(x =>
      {
        IEnumerable<XElement> childs = x.Descendants();

        float rating = childs.GetFloat("averageUserRating");

        if (Configuration.UseFiveStarSystem)
        {
          rating = rating * 0.5f;
        }

        AppMetadata data = new AppMetadata()
        {
          Id = childs.Get("id").Split(':').Last(),
          Urn = childs.Get("id"),
          Name = childs.Get("title"),
          Rating = new AppRating()
          {
            AverageRating = rating,
            RatingCount = Convert.ToInt32(childs.Get("userRatingCount"))
          },
          Images = new AppMetadataImages()
          {
            Logo = new AppImage()
            {
              Rotation = 0,
              Urn = childs.Get("image")
            }
          }
        };

        return data;
      });
      result.Count = result.Results.Count();

      return result;
    }


    /// <summary>
    /// Get spotlight entries for the current day in the specified culture.
    /// This only returns IDs for fast access.
    /// </summary>
    /// <param name="spotlightType">Can either be apps or games. Both return approx. 20 new results per day.</param>
    /// <param name="store">The store where the app is published.</param>
    /// <param name="storeCulture">Culture of the query (returns location specific metadata + ratings).</param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> GetSpotlightIds(StoreSpotlightType spotlightType, StoreType store, StoreCultureType storeCulture)
    {
      if (storeCulture == StoreCultureType.Unknown || storeCulture == StoreCultureType.All)
      {
        throw new ArgumentException("Please provide a valid store culture");
      }

      Uri uri = EndpointUris.GetWindowsPhoneSpotlightUri(storeCulture.ToString(), spotlightType.GetEnumDisplayName());

      string xml = await ApiHttpClient.Instance.Get(uri);

      IEnumerable<string> ids = XDocument.Parse(xml).Elements().First().Descendants().Where(x => x.Name.LocalName == "application").Select(x => x.Descendants().Get("id").Split(':').Last());
      return ids;
    }


    /// <summary>
    /// Get app description, images, publisher, rating and more for an app in the specified culture.
    /// </summary>
    /// <param name="appId">The ID of the app. Can be found in the dev portal or the store URI.</param>
    /// <param name="store">The store where the app is published.</param>
    /// <param name="storeCulture">Culture of the query (returns location specific metadata + ratings).</param>
    /// <returns></returns>
    public async Task<AppMetadata> GetMetadata(string appId, StoreType store, StoreCultureType storeCulture)
    {
      if (storeCulture == StoreCultureType.Unknown || storeCulture == StoreCultureType.All)
      {
        throw new ArgumentException("Please provide a valid store culture");
      }

      string xml = await ApiHttpClient.Instance.Get(
        EndpointUris.GetWindowsPhoneMetadataUri(appId, storeCulture.ToString())
      );

      IEnumerable<XElement> xel = XDocument.Parse(xml).Elements().First().Descendants();

      // create metadatas
      AppMetadata result = Deserialize.Xml<AppMetadata>(xml);
      result.Id = appId;
      result.StoreType = store;
      result.StoreCultureType = storeCulture;

      // create publisher
      result.Publisher = Deserialize.Xml<AppPublisher>(xml);
      result.Publisher.Id = result.Publisher.Urn.Split(':').LastOrDefault();

      // create rating
      result.Rating = Deserialize.Xml<AppRating>(xml);
      result.Rating.AverageRating = xel.GetFloat("averageUserRating");
      if (Configuration.UseFiveStarSystem)
      {
        result.Rating.AverageRating = (float)(result.Rating.AverageRating * 0.5);
      }

      // create images
      result.Images = Deserialize.Xml<AppMetadataImages>(xml);
      result.Images.Cleanup();
      result.Images.Screenshots = xel.Where(x => x.Name.LocalName == "screenshot").Select(x =>
      {
        IEnumerable<XElement> childs = x.Descendants();
        return new AppImage()
        {
          Urn = childs.Get("id"),
          Rotation = childs.GetShort("orientation")
        };
      });

      return result;
    }



    /// <summary>
    /// Get app rating (count + average) in the specified culture.
    /// </summary>
    /// <param name="appId">The ID of the app. Can be found in the dev portal or the store URI.</param>
    /// <param name="store">The store where the app is published.</param>
    /// <param name="storeCulture">Culture of the query (returns location specific ratings).</param>
    /// <returns></returns>
    public async Task<AppRating> GetRating(string appId, StoreType store, StoreCultureType storeCulture)
    {
      if (storeCulture == StoreCultureType.Unknown || storeCulture == StoreCultureType.All)
      {
        throw new ArgumentException("Please provide a valid store culture");
      }

      AppRating result = new AppRating();
      result.Culture = storeCulture;

      try
      {
        string xml = await ApiHttpClient.Instance.Get(
          EndpointUris.GetWindowsPhoneMetadataUri(appId, storeCulture.ToString())
        );

        IEnumerable<XElement> xel = XDocument.Parse(xml).Elements().First().Descendants();

        // update rating
        result.RatingCount = Convert.ToInt32(xel.Get("userRatingCount"));
        result.AverageRating = xel.GetFloat("averageUserRating");
        if (Configuration.UseFiveStarSystem)
        {
          result.AverageRating = (float)(result.AverageRating * 0.5);
        }

        return result;
      }
      catch
      {
        result.RatingNotAvailable = true;
        return result;
      }
    }



    /// <summary>
    /// Get a list of reviews (up to 20 per request) for the specified app.
    /// </summary>
    /// <param name="appId">The ID of the app. Can be found in the dev portal or the store URI.</param>
    /// <param name="store">The store where the app is published.</param>
    /// <param name="storeCulture">Culture of the query (returns location specific metadata + ratings).</param>
    /// <param name="sorting">Sorting criteria.</param>
    /// <returns></returns>
    public async Task<AppReviews> GetReviews(string appId, StoreType store, StoreCultureType storeCulture, StoreReviewSorting sorting, string prevPageMarkerId = null, string nextPageMarkerId = null)
    {
      if (storeCulture == StoreCultureType.Unknown || storeCulture == StoreCultureType.All)
      {
        throw new ArgumentException("Please provide a valid store culture");
      }

      string xml = await ApiHttpClient.Instance.Get(
        EndpointUris.GetWindowsPhoneReviewsUri(appId, storeCulture.ToString(), sorting.ToString(), prevPageMarkerId, nextPageMarkerId)
      );

      IEnumerable<XElement> xel = XDocument.Parse(xml).Elements().First().Descendants();

      AppReviews result = new AppReviews()
      {
        Id = appId,
        StoreType = store,
        StoreCultureType = storeCulture,
        Sorting = sorting,
        StoreDataModifiedDate = DateTime.Parse(xel.Get("updated"))
      };

      // parse markers
      result.PrevPageMarkerId = Utils.GetQueryPart(xel.FirstOrDefault(x => x.Name.LocalName == "link" && x.Attribute("rel").Value == "prev"), "href", "beforeMarker");
      result.NextPageMarkerId = Utils.GetQueryPart(xel.FirstOrDefault(x => x.Name.LocalName == "link" && x.Attribute("rel").Value == "next"), "href", "afterMarker");

      // append reviews
      result.Reviews = xel.Where(x => x.Name.LocalName == "entry").Select(x =>
      {
        IEnumerable<XElement> childs = x.Descendants();
        byte rating = (byte)childs.GetFloat("userRating");

        return new AppReview()
        {
          Id = childs.Get("reviewId"),
          CreatedDate = DateTime.Parse(childs.Get("updated")),
          Author = childs.FirstOrDefault(c => c.Name.LocalName == "author").Descendants().Get("name"),
          Text = childs.Get("content"),
          Rating = Configuration.UseFiveStarSystem ? (byte)(rating * 0.5) : rating,
          Device = childs.Get("device"),
          AppVersion = childs.Get("productVersion")
        };
      });

      result.IsEmpty = result.Reviews == null || !result.Reviews.Any();

      return result;
    }



    /// <summary>
    /// Creates an URI from an image urn (included in the AppImage POCO).
    /// </summary>
    /// <param name="urn">The urn (is part of the AppImage).</param>
    /// <param name="imageType">Crops the image accordingly.</param>
    /// <returns></returns>
    public Uri GetImageUri(string urn, StoreImageType imageType = StoreImageType.None)
    {
      string type = imageType.GetEnumDisplayName();
      return EndpointUris.GetWindowsPhoneImageUri(urn, type);
    }



    /// <summary>
    /// Returns a stream for a store image.
    /// </summary>
    /// <param name="urn">The urn (is part of the AppImage).</param>
    /// <param name="imageType">Crops the image accordingly.</param>
    /// <returns></returns>
    public async Task<Stream> GetImageAsStream(string urn, StoreImageType imageType = StoreImageType.None)
    {
      string type = imageType.GetEnumDisplayName();
      Uri uri = EndpointUris.GetWindowsPhoneImageUri(urn, type);
      return await ApiHttpClient.Instance.GetStream(uri);
    }



    /// <summary>
    /// Gets metadata for an app for all available cultures
    /// Warning: This method makes a request per culture (100+) which can take a while.
    /// </summary>
    /// <param name="appId">The ID of the app. Can be found in the dev portal or the store URI.</param>
    /// <param name="store">The store where the app is published.</param>
    /// <param name="ct">The canellation token.</param>
    /// <param name="progress">The progress event gets triggered as soon as new data arrives.</param>
    /// <returns></returns>
    public async Task<IEnumerable<AppMetadata>> GetMetadataForAllCultures(string appId, StoreType store, CancellationToken ct, IProgress<DownloadProgressChangedEventArgs> progress)
    {
      IEnumerable<StoreCultureType> cultures = Enum.GetValues(typeof(StoreCultureType))
        .Cast<StoreCultureType>()
        .Where(x => x != StoreCultureType.All && x != StoreCultureType.Unknown);

      IEnumerable<Task<AppMetadata>> tasks = cultures.Select(culture =>
      {
        return GetMetadata(appId, store, culture);
      });

      return await Task.WhenAll(tasks).ConfigureAwait(false);
    }



    /// <summary>
    /// Gets ratings for an app for all available cultures
    /// Warning: This method makes a request per culture (100+) which can take a while.
    /// </summary>
    /// <param name="appId">The ID of the app. Can be found in the dev portal or the store URI.</param>
    /// <param name="store">The store where the app is published.</param>
    /// <param name="ct">The canellation token.</param>
    /// <param name="progress">The progress event gets triggered as soon as new data arrives.</param>
    /// <returns></returns>
    public async Task<IEnumerable<AppRating>> GetRatingsForAllCultures(string appId, StoreType store, CancellationToken ct, IProgress<DownloadProgressChangedEventArgs> progress)
    {
      IEnumerable<StoreCultureType> cultures = Enum.GetValues(typeof(StoreCultureType))
        .Cast<StoreCultureType>()
        .Where(x => x != StoreCultureType.All && x != StoreCultureType.Unknown);

      IEnumerable<Task<AppRating>> tasks = cultures.Select(culture =>
      {
        return GetRating(appId, store, culture);
      });

      return await Task.WhenAll(tasks).ConfigureAwait(false);
    }
  }


  public class DownloadProgressChangedEventArgs : EventArgs { }
}
