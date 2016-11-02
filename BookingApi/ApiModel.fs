namespace Ploeh.Samples.BookingApi

open System.Web.Http
open System
open Ploeh.Samples.Rop

[<CLIMutable>]
type ReservationRendition = {
  Date : string
  Name : string
  Email : string
  Quantity : int}

type Error =
| ValidationError of string
| CapacityExceeded

type Reservation = {
  Date: DateTimeOffset
  Name : string
  Email : string
  Quantity : int}

module Validate = 
  let reservationValid (rendition : ReservationRendition) =
    match rendition.Date |> DateTimeOffset.TryParse with 
    |  true, date ->
    Success {
        Date = date
        Name = rendition.Name
        Email = rendition.Email
        Quantity = rendition.Quantity }
    | _ -> Failure(ValidationError "Invalid date.")

module Capacity =
  let check capacity getReservedSeats reservation =
    let reservedSeats = getReservedSeats reservation.Date
    if capacity < reservation.Quantity + reservedSeats
    then Failure CapacityExceeded
    else Success reservation

type ReservationsController(imp) =
  inherit ApiController()
  member this.Post (rendition : ReservationRendition) : IHttpActionResult =
    this.Ok() :> _
