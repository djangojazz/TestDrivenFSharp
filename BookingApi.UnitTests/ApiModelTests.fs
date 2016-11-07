namespace Ploeh.Samples.BookingApi.UnitTests

open System
open Swensen.Unquote
open FsCheck
open FsCheck.Xunit
open Ploeh.Samples.Rop
open Ploeh.Samples.BookingApi

module ValidateTests = 

  [<Property>]
  let ``Validate.reservationValid returns correct result on valid date`` 
    (rendition : ReservationRendition) 
    (date: DateTimeOffset) =

    let rendition = { rendition with Date = date.ToString "o" }

    let actual = Validate.reservationValid rendition

    let expected = Success {
        Date = date
        Name = rendition.Name
        Email = rendition.Email
        Quantity = rendition.Quantity }
    expected = actual

  [<Property>]
  let ``Validate.reservationValid returns correct result on invalid date``
    (rendition : ReservationRendition)  = 
    not(fst(DateTimeOffset.TryParse rendition.Date)) ==> lazy

    let actual = Validate.reservationValid rendition

    let expected : Result<Reservation, Error> =
      Failure(ValidationError("Invalid date."))
    expected = actual

module CapacityTests = 
  
[<Property>]
let ``Capacity.check returns correct result at no prior reservations`` 
    (reservation: Reservation)
    (excessCapacity: int) =
    (reservation.Quantity >= 0 && excessCapacity >= 0) ==> lazy

      let capacity = reservation.Quantity + excessCapacity
      let getReservedSeats _ = 0

      let actual = Capacity.check capacity getReservedSeats reservation

      let expected : Result<Reservation, Error> = Success reservation

      expected = actual

[<Property>]
let ``Capacity.check returns correct result at too little remaining capacity``
    (reservation: Reservation)
    (capacity: int) 
    (reservedSeats: int) =
    (capacity >= 0 && reservedSeats >= 0 && capacity < reservedSeats + reservation.Quantity) ==> lazy

  let getReservedSeats _ = reservedSeats

  let actual = Capacity.check  capacity getReservedSeats reservation

  let expected : Result<Reservation, Error> =  Failure CapacityExceeded

  expected = actual

module ControllerTests =

open System.Web.Http
open System.Net

[<Property>]
let ``ReservationsController.Post returns correct result on Success`` 
    (rendition : ReservationRendition) =

    let imp _ = Success ()
    use sut = new ReservationsController(imp)

    let result : IHttpActionResult = sut.Post rendition

    result :? Results.OkResult

[<Property>]
let ``ReservationsController.Post returns correct result on Validation Error`` 
    (rendition : ReservationRendition) =

    let imp _ = Failure(ValidationError "Invalid date.")
    use sut = new ReservationsController(imp)

    let result = sut.Post rendition

    result :? Results.BadRequestErrorMessageResult

let convertsTo<'a> candidate = 
    match box candidate with
    | :? 'a as converted -> Some converted
    | _ -> None

[<Property>]
let ``ReservationsController.Post returns correct result on capactiy exceeded`` 
    (rendition : ReservationRendition) =

    let imp _ = Failure CapacityExceeded
    use sut = new ReservationsController(imp)

    let result = sut.Post rendition

    result 
        |> convertsTo<Results.StatusCodeResult> 
        |> Option.map (fun x -> x.StatusCode) 
        |> Option.exists ((=) HttpStatusCode.Forbidden)