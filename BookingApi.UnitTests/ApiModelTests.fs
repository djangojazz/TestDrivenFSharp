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

module ControllerTests =

  open System.Web.Http

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