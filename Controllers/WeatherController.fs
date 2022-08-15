namespace MyAPI.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging
open MyAPI
open MyAPI.Hubs.MyHub


// https://stackoverflow.com/questions/17395201/call-a-hub-method-from-a-controllers-action
// https://stackoverflow.com/questions/46904678/call-signalr-core-hub-method-from-controller
[<ApiController>]
[<Route("[controller]")>]
type SendController(hubCtx: IHubContext<MyHub>,logger : ILogger<SendController>) =
    inherit ControllerBase()
    
    member this.Get() =
        let user = "Server"
        let message = "Hello from the controller"
        // https://docs.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-6.0
        // sendMessage hubCtx user message |> ignore
        sendMsg hubCtx.Clients user message |> ignore
        // https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/anonymous-records
        {|
            User=user
            Message=message
        |}

// the most weired thing is the convention
// change the name of type/class to <Whatever Name>Controller
// You will get an actual controller
// [<Route("[controller]")>]
// Thank god you can set the end point manually
// [<Route("/abc")>]
[<ApiController>]
[<Route("[controller]")>]
type WeatherController (logger : ILogger<WeatherController>) =
    inherit ControllerBase()

    let summaries =
        [|
            "Freezing"
            "Bracing"
            "Chilly"
            "Cool"
            "Mild"
            "Warm"
            "Balmy"
            "Hot"
            "Sweltering"
            "Scorching"
        |]

    [<HttpGet>]
    member _.Get() =
        let rng = System.Random()
        [|
            for index in 0..5 ->
                { Date = DateTime.Now.AddDays(float index)
                  TemperatureC = rng.Next(-20,55)
                  Summary = summaries.[rng.Next(summaries.Length)] }
        |]
