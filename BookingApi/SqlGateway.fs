module Ploeh.Samples.BookingApi.SqlGateway

open System
open System.Data.Entity
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open Ploeh.Samples

type Reservation() =
    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    member val Id = Unchecked.defaultof<int> with get, set

    member val Date = Unchecked.defaultof<DateTimeOffset> with get, set

    [<Required; StringLength(50)>]
    member val Name = Unchecked.defaultof<string> with get, set

    [<Required; StringLength(50)>]
    member val Email = Unchecked.defaultof<string> with get, set

    member val Quantity = Unchecked.defaultof<int> with get, set

type ReservationsContext() =
    inherit DbContext("name=ReservationsModel")

    [<DefaultValue>] val mutable reservations : DbSet<Reservation>
    member this.Reservations
        with get() = this.reservations
        and set v = this.reservations <- v


let getReservedSeats (date: DateTimeOffset) =
    let min = DateTimeOffset(date.Date, date.Offset)
    let max = DateTimeOffset(date.Date.AddDays 1., date.Offset)
    
    use ctx = new ReservationsContext()
    query {
        for r in ctx.reservations do
        where (min <= r.Date && r.Date < max)
        select r.Quantity }
    |> Seq.sum

let saveReservation (reservation : BookingApi.Reservation) =
    use ctx = new ReservationsContext()
    ctx.Reservations.Add(
        Reservation(
            Date = reservation.Date,
            Name = reservation.Name,
            Email = reservation.Email,
            Quantity = reservation.Quantity)) |> ignore

    ctx.SaveChanges() |> ignore