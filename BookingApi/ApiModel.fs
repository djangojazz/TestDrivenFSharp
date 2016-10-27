namespace Ploeh.Samples.BookingApi

open System.Web.Http

[<CLIMutable>]
type ReservationRendition = {
  Date : string
  Name : string
  Email : string
  Quantity : int}

type ReservationsController(imp) =
  inherit ApiController()
  member this.Post (rendition : ReservationRendition) : IHttpActionResult =
    this.Ok() :> _
