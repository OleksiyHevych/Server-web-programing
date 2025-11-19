using Lab1.Data;
using Lab1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Lab1.Hubs
{
    [Authorize]
    public class MovieChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();

        public MovieChatHub(ApplicationDbContext db) { _db = db; }

        private string GetUserId() => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
        private string GetUserName() => Context.User?.Identity?.Name ?? "unknown";

        public override async Task OnConnectedAsync()
        {
            var uid = GetUserId();
            var uname = GetUserName();
            _onlineUsers[uid] = uname;

            await Clients.Caller.SendAsync("InitializeOnlineUsers", _onlineUsers);
            await Clients.Others.SendAsync("UserStatusChanged", uid, uname, true);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var uid = GetUserId();
            _onlineUsers.TryRemove(uid, out _);
            await Clients.All.SendAsync("UserStatusChanged", uid, "", false);
            await base.OnDisconnectedAsync(exception);
        }

        // --- movie chat ---
        public async Task JoinMovie(string movieId) => await Groups.AddToGroupAsync(Context.ConnectionId, $"movie-{movieId}");
        public async Task LeaveMovie(string movieId) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"movie-{movieId}");

        public async Task SendMessageToMovie(string movieId, string text)
        {
            var uid = GetUserId();
            var uname = GetUserName();

            var msg = new ChatMessage
            {
                MovieId = int.TryParse(movieId, out var m) ? m : (int?)null,
                SenderUserId = uid,
                SenderUserName = uname,
                Text = text,
                IsPrivate = false
            };

            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            await Clients.Group($"movie-{movieId}").SendAsync("ReceiveMessage", new
            {
                id = msg.Id,
                movieId = msg.MovieId,
                senderId = msg.SenderUserId,
                senderName = msg.SenderUserName,
                text = msg.Text,
                fileUrl = msg.FileUrl,
                fileName = msg.FileName,
                createdAt = msg.CreatedAt
            });
        }

        // --- Send file ---
        public async Task SendFileToMovie(string movieId, string fileUrl, string fileName)
        {
            var uid = GetUserId();
            var uname = GetUserName();

            var msg = new ChatMessage
            {
                MovieId = int.TryParse(movieId, out var m) ? m : (int?)null,
                SenderUserId = uid,
                SenderUserName = uname,
                Text = null,
                FileUrl = fileUrl,
                FileName = fileName,
                IsPrivate = false
            };

            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            await Clients.Group($"movie-{movieId}").SendAsync("ReceiveMessage", new
            {
                id = msg.Id,
                movieId = msg.MovieId,
                senderId = msg.SenderUserId,
                senderName = msg.SenderUserName,
                text = msg.Text,
                fileUrl = msg.FileUrl,
                fileName = msg.FileName,
                createdAt = msg.CreatedAt
            });
        }

        // --- private DM ---
        public async Task SendPrivateMessage(string receiverUserId, string text)
        {
            var uid = GetUserId();
            var uname = GetUserName();

            var msg = new ChatMessage
            {
                SenderUserId = uid,
                SenderUserName = uname,
                ReceiverUserId = receiverUserId,
                Text = text,
                IsPrivate = true
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            await Clients.User(receiverUserId).SendAsync("ReceivePrivateMessage", new
            {
                senderId = uid,
                senderName = uname,
                receiverId = receiverUserId,
                text = text,
                createdAt = msg.CreatedAt
            });
            await Clients.Caller.SendAsync("ReceivePrivateMessage", new
            {
                senderId = uid,
                senderName = uname,
                receiverId = receiverUserId,
                text = text,
                createdAt = msg.CreatedAt
            });
        }

        public async Task TypingPrivate(string receiverUserId)
        {
            var uid = GetUserId();
            var uname = GetUserName();
            await Clients.User(receiverUserId).SendAsync("UserTypingPrivate", uid, uname);
        }

        public async Task StopTypingPrivate(string receiverUserId)
        {
            var uid = GetUserId();
            await Clients.User(receiverUserId).SendAsync("UserStopTypingPrivate", uid);
        }
    }
}
