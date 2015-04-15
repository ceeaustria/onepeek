﻿using Nancy;
using OnePeek.Api;
using OnePeek.Entities;
using System.Collections.Generic;
using System.IO;

namespace OnePeek.WebConsole.Modules
{
  public class ApiModule : NancyModule
  {
    public ApiModule() : base("api/")
    {
      AppRatingEndpoint ratingEndpoint = new AppRatingEndpoint();
      AppMetadataEndpoint metaEndpoint = new AppMetadataEndpoint();

      Get["/meta/{id}", true] = async (ctx, token) =>
      {
        AppMetadata meta = await metaEndpoint.GetMetadata(ctx.id, StoreType.WindowsPhone8, StoreCultureType.EN_US);
        return Response.AsJson(meta);
      };


      Get["/reviews/{id}", true] = async (ctx, token) =>
      {      
        AppReviews reviews = await ratingEndpoint.GetReviews(ctx.id, StoreType.WindowsPhone8, StoreCultureType.EN_US, StoreReviewSorting.Latest);
        return Response.AsJson(reviews);
      };


      Get["/ratings/{id}", true] = async (ctx, token) =>
      {
        IEnumerable<AppRating> ratings = await metaEndpoint.GetRatingsForAllCultures(ctx.id, StoreType.WindowsPhone8, new System.Threading.CancellationTokenSource().Token, null);
        return Response.AsJson(ratings);
      };


      Get["/image/{id}.jpg", true] = async (ctx, token) =>
      {
        Stream imageStream = await metaEndpoint.GetImageAsStream(ctx.id, StoreImageType.None);
        return Response.FromStream(imageStream, "image/jpeg");
      };
    }
  }
}