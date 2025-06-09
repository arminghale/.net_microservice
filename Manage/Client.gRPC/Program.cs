using Client.gRPC;
using Grpc.Net.Client;

// The port number must match the port of the gRPC server.
//using var channel = GrpcChannel.ForAddress("https://localhost:5296");
using var channel = GrpcChannel.ForAddress("https://localhost:7215");
var client = new Users.UsersClient(channel);
var reply = await client.SendUserAsync(
    new UserRequest { Id = 1 });
Console.WriteLine($"Phone: {reply.Phonenumber}, Username: {reply.Username}, Email: {reply.Email}, Validation: {reply.Validation}, LastLogin: {reply.LastLogin}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
