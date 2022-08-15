module MyAPI.Hubs.MyHub

open System
open FSharpPlus
open Microsoft.AspNetCore.SignalR
open FSharp.Json
open FSharpPlus

let sendMsgTo (user: string) (msg: string) (clients: IClientProxy) =
    clients.SendAsync("ReceiveMessage", user, msg)

let sendMsg<'T when 'T :> IClientProxy>
    (user: string)
    (message: string)
    (exclude: Option<List<string>>)
    (clients: IHubClients<'T>)
    =
    let c =
        match exclude with
        | Some (ex) -> clients.AllExcept(ex)
        | None -> clients.All

    c |> sendMsgTo user message |> Async.AwaitTask


// https://docs.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-6.0
// https://stackoverflow.com/questions/34380571/combining-signalr-2-with-database
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0
type MyHub() =
    inherit Hub()

    let mutable groups =
        Map<string, Set<string>> []

    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr-typescript-webpack?view=aspnetcore-6.0
    // Use `connection.send("sendToAll", user, message)` to send a message to all clients in JavaScript side.
    [<HubMethodName("SendToAll")>]
    member this.SendMessage(user: string, message: string) =
        // I still not sure whether the task block is needed (I guess not since the return type is still correct)
        // Use `connection.on("receiveMessage", (usr: string, msg: string) => {})` (small camel)
        // to handle messages in the client (JavaScript side).
        // I can really use some macro shit
        // Send to all clients except the sender.
        this.Clients.Caller
        |> sendMsgTo "Server" $"You said: {message}"
        |> ignore

        this.Clients.Others |> sendMsgTo user message

    member this.JoinGroup(group: string) =
        let id = this.Context.ConnectionId
        this.Groups.AddToGroupAsync(id, group) |> ignore
        let ids = groups.TryFind group

        groups <-
            match ids with
            | Some (ids) -> groups.Add(group, Set.add id ids)
            | None -> groups.Add(group, Set.ofSeq [ id ])

        this.Clients.Caller
        |> sendMsgTo "Server" $"You joined the group {group}"
        |> ignore

        this.Clients.Others
        |> sendMsgTo "Server" $"{id} has joined the group {group}"

    member this.ListGroup() =
        this.Clients.Caller
        |> sendMsgTo "Server" $"{Json.serialize groups}"
        |> ignore


    // https://github.com/dotnet/fsharp/issues/12448
    // https://www.compositional-it.com/news-blog/task-vs-async/
    // member this.BaseOnConnectedAsync() = base.OnConnectedAsync()
    override this.OnConnectedAsync() =
        this.Clients.Caller
        |> sendMsgTo "Server" $"Welcome to the chat! {this.Context.ConnectionId}"
        |> ignore

        this.Clients.All
        |> sendMsgTo "Server" $"{this.Context.ConnectionId} joined the chat."
        |> ignore

        base.OnConnectedAsync()

    override this.OnDisconnectedAsync(ex: Exception) =
        let id = this.Context.ConnectionId
        groups <- map (Set.remove id) groups

        this.Clients.Others
        |> sendMsgTo "Server" $"{this.Context.ConnectionId} has died."
        |> ignore

        ``base``.OnDisconnectedAsync(ex)
