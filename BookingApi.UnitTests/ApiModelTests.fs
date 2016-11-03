namespace Ploeh.Samples.BookingApi.UnitTests

open System
open Swensen.Unquote
open Xunit
open Ploeh.Samples.Rop
open Ploeh.Samples.BookingApi

module ValidateTests = 

  [<Fact>]
  let ``Validate.reservationValid returns correct result on valid date`` ()=
    let rendition : ReservationRendition = {
      Date = "2015-04-15+2:00"
      Name = "Brett Morin"
      Email = "bmorin@a.com"
      Quantity = 4 }

    let actual = Validate.reservationValid rendition

    let expected = Success {
        Date = 
          DateTimeOffset(
            DateTime(2015, 4, 15), 
            TimeSpan.FromHours 2.)
        Name = "Brett Morin"
        Email = "bmorin@a.com"
        Quantity = 4 }
    expected =? actual

  [<Fact>]
  let ``Validate.reservationValid returns correct result on invalid date`` ()=
    let rendition : ReservationRendition = {
      Date = "Not a valid date"
      Name = "Brett Morin"
      Email = "bmorin@a.com"
      Quantity = 4 }

    let actual = Validate.reservationValid rendition

    let expected : Result<Reservation, Error> =
      Failure(ValidationError("Invalid date."))
    expected =? actual

module CapacityTests = 
  
[<Fact>]
let ``Capacity.check returns correct result at no prior reservations`` ()=
  let capacity = 10
  let getReservedSeats _ = 0
  let reservation = {
      Date = 
        DateTimeOffset(
          DateTime(2015, 4, 15), 
          TimeSpan.FromHours 2.)
      Name = "Brett Morin"
      Email = "bmorin@a.com"
      Quantity = 4 }

  let actual = 
    Capacity.check 
      capacity 
      getReservedSeats 
      reservation

  let expected : Result<Reservation, Error> = 
    Success reservation

  expected =? actual

[<Fact>]
let ``Capacity.check returns correct result at too little remaining capacity`` ()=
  let capacity = 10
  let getReservedSeats _ = 7
  let reservation = {
      Date = 
        DateTimeOffset(
          DateTime(2015, 4, 15), 
          TimeSpan.FromHours 2.)
      Name = "Brett Morin"
      Email = "bmorin@a.com"
      Quantity = 4 }

  let actual = 
    Capacity.check 
      capacity 
      getReservedSeats 
      reservation

  let expected : Result<Reservation, Error> = 
    Failure CapacityExceeded

  expected =? actual


module ControllerTests =

open System.Web.Http
open System.Net

[<Fact>]
let ``ReservationsController.Post returns correct result on Success`` () =
    let imp _ = Success ()
    use sut = new ReservationsController(imp)
    let rendition : ReservationRendition = {
        Date = "2015-04-15+2:00"
        Name = "Brett Morin"
        Email = "bmorin@a.com"
        Quantity = 5 }

    let result : IHttpActionResult = sut.Post rendition

    test <@ result :? Results.OkResult @>

[<Fact>]
let ``ReservationsController.Post returns correct result on Validation Error`` () =
    let imp _ = Failure(ValidationError "Invalid date.")
    use sut = new ReservationsController(imp)
    let rendition : ReservationRendition = {
        Date = "2015-04-15+2:00"
        Name = "Brett Morin"
        Email = "bmorin@a.com"
        Quantity = 5 }

    let result = sut.Post rendition

    test <@ result :? Results.BadRequestErrorMessageResult @>

let convertsTo<'a> candidate = 
    match box candidate with
    | :? 'a as converted -> Some converted
    | _ -> None

[<Fact>]
let ``ReservationsController.Post returns correct result on capactiy exceeded`` () =
    let imp _ = Failure CapacityExceeded
    use sut = new ReservationsController(imp)
    let rendition : ReservationRendition = {
        Date = "2015-04-14+2:00"
        Name = "Brett Morin"
        Email = "bmorin@a.com"
        Quantity = 5 }

    let result = sut.Post rendition

    test <@ result 
        |> convertsTo<Results.StatusCodeResult> 
        |> Option.map (fun x -> x.StatusCode) 
        |> Option.exists ((=) HttpStatusCode.Forbidden) @>
