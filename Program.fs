namespace MyAPI

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Reflection
open FSharpPlus
open MyAPI.Hubs.MyHub

// https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/parameters-and-arguments
// https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/active-patterns
module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder =
            WebApplication.CreateBuilder(args)

        builder.Services.AddControllers()
        builder.Services.AddSignalR()
        // https://stackoverflow.com/questions/31942037/how-to-enable-cors-in-asp-net-core
        builder.Services.AddCors (fun action ->
            action.AddPolicy(
                "AllowAll",
                fun builder ->
                    builder
                        .SetIsOriginAllowed(konst (true)) // allow all origins
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                    |> ignore
            )) // return unit (void) (Action is a function that return null)

        let app = builder.Build()

        // app.UseHttpsRedirection()
        //app.UseAuthorization()
        app.UseCors("AllowAll")
        app.MapControllers()
        app.MapHub<MyHub>("/signal")

        app.Run()

        exitCode
