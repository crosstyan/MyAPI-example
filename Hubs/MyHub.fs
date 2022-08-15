module MyAPI.Hubs.MyHub

open Microsoft.AspNetCore.SignalR

// Fuck I wish I have macro
// Or something like code generation
// https://github.com/MoiraeSoftware/myriad
let sendMessage<'T when 'T :> Hub> (ctx: IHubContext<'T>) (user: string) (message: string) =
    ctx.Clients.All.SendAsync("ReceiveMessage", user, message)
    
let sendMsg<'T when 'T :> IClientProxy> (clients: IHubClients<'T>) (user: string) (message: string) =
    clients.All.SendAsync("ReceiveMessage", user, message)
    
// https://docs.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-6.0
// https://stackoverflow.com/questions/34380571/combining-signalr-2-with-database
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0
type MyHub() =
    inherit Hub()

    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr-typescript-webpack?view=aspnetcore-6.0&tabs=visual-studio

    // Use `connection.send("sendToAll", user, message)` to send a message to all clients in JavaScript side.
    [<HubMethodName("SendToAll")>]
    member this.SendMessage(user: string, message: string) =
        // I still not sure whether the task block is needed (I guess not since the return type is still correct)
        // Use `connection.on("receiveMessage", (usr: string, msg: string) => {})` (small camel)
        // to handle messages in the client (JavaScript side).
        // I can really use some macro shit
        // this.Clients.All.SendAsync("ReceiveMessage", user, message)
        sendMsg this.Clients user message

    // https://github.com/dotnet/fsharp/issues/12448
    // https://www.compositional-it.com/news-blog/task-vs-async/
    // member this.BaseOnConnectedAsync() = base.OnConnectedAsync()

    override this.OnConnectedAsync() =
        this.Groups.AddToGroupAsync(this.Context.ConnectionId, "TestGroup") |> ignore
        base.OnConnectedAsync()
        
