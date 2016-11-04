namespace Ploeh.Samples.BookingApi

open System
open System.Web.Http
open System.Web.Http.Dispatcher
open Ploeh.Samples

type CompositionRoot() =
    interface IHttpControllerActivator with
        member this.Create(request, controllerDescriptor, controllerType) =
            if controllerType = typeof<ReservationsController> then
                let imp = 
                    Validate.reservationValid
                    >> Rop.bind (Capacity.check 10 SqlGateway.getReservedSeats)
                    >> Rop.map SqlGateway.saveReservation
                new ReservationsController(imp) :> _
            else invalidArg "controllerType" (sprintf "Unknown Controller type: %O" controllerType)

type HttpRouteDefaults = { Controller : string; Id : obj }

type Global() =
    inherit System.Web.HttpApplication()
    member this.Application_Start (sender : obj) (e : EventArgs) =
        GlobalConfiguration.Configuration.Routes.MapHttpRoute(
            "DefaultAPI",
            "{controller}/{id}",
            { Controller = "Home"; Id = RouteParameter.Optional }) |> ignore

        GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver <-
            Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

        GlobalConfiguration.Configuration.Services.Replace(
            typeof<IHttpControllerActivator>,
            CompositionRoot())

