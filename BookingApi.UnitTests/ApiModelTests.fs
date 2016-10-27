namespace Ploeh.Samples.BookingApi.UnitTests

open Swensen.Unquote
open Xunit
open Ploeh.Samples.Rop
open Ploeh.Samples.BookingApi

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