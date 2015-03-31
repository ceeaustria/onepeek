﻿using Nancy;
using OnePeek.Entities;
using System.IO;

namespace OnePeek.WebConsole
{
  public class HomeModule : NancyModule
  {
    public HomeModule()
    {
      var ratingEndpoint = new Api.AppRatingEndpoint();
      var metaEndpoint = new Api.AppMetadataEndpoint();


      Get["/meta/{id?2532ff45-aa3f-4aba-a266-ed7ec71d47bd}", true] = async (ctx, token) =>
      {
        AppMetadata meta = await metaEndpoint.GetMetadata(ctx.id, StoreType.WindowsPhone8, StoreCultureType.EN_US);
        return Response.AsJson(meta);
      };


      Get["/reviews/{id?2532ff45-aa3f-4aba-a266-ed7ec71d47bd}", true] = async (ctx, token) =>
      {      
        AppReviews reviews = await ratingEndpoint.GetReviews(ctx.id, StoreType.WindowsPhone8, StoreCultureType.EN_US, StoreReviewSorting.Latest);
        return Response.AsJson(reviews);
      };


      Get["/image/{id?4274cebb-01c7-4788-97f7-635322fa4877}", true] = async (ctx, token) =>
      {
        Stream imageStream = await metaEndpoint.GetImageAsStream(ctx.id, StoreImageType.None);
        return Response.FromStream(imageStream, "image/jpeg");
      };
    }
  }
}