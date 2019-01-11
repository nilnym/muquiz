﻿using Microsoft.EntityFrameworkCore;
using MuQuiz.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuQuiz.Models
{
    public class GameService
    {
        private readonly MuquizContext muquizContext;

        public GameService(MuquizContext muquizContext)
        {
            this.muquizContext = muquizContext;
        }

        public async Task InitializeSession(string gameId, string connectionId)
        {
            await muquizContext.GameSession.AddAsync(new GameSession { GameId = gameId, IsPlaying = false, HostConnectionId = connectionId });
            await muquizContext.SaveChangesAsync();
        }

        public async Task SetIsPlaying(string gameId, bool isPlaying)
        {
            muquizContext.GameSession.Single(g => g.GameId == gameId).IsPlaying = isPlaying;
            await muquizContext.SaveChangesAsync();
        }

        public async Task StartPlaying(string gameId)
        {
            await SetIsPlaying(gameId, true);
        }

        public async Task StopPlaying(string gameId)
        {
            await SetIsPlaying(gameId, false);
            muquizContext.Player
                .Where(p => p.GameSession.GameId == gameId)
                .ToList()
                .ForEach(p => p.Score = 0);
            await muquizContext.SaveChangesAsync();
        }

        public async Task<bool> SessionIsActive(string gameId)
        {
            if (await muquizContext.GameSession.AnyAsync(g => g.GameId == gameId))
            {
                var result = await muquizContext.GameSession.SingleAsync(g => g.GameId == gameId);
                return !result.IsPlaying;
            }
            return false;
        }

        public async Task AddPlayer(string connectionId, string name, string gameId)
        {
            await muquizContext.Player.AddAsync(new Player
            {
                Name = name,
                ConnectionId = connectionId,
                Score = 0,
                GameSessionId = muquizContext.GameSession.SingleOrDefault(g => g.GameId == gameId).Id
            });
            await muquizContext.SaveChangesAsync();
        }

        internal async Task<bool> IsPlayer(string connectionId)
        {
            return await muquizContext.Player.CountAsync(p => p.ConnectionId == connectionId) > 0;
        }

        public async Task<Player[]> GetAllPlayers(string gameId)
        {
            return await muquizContext.Player.Where(p => p.GameSession.GameId == gameId).OrderByDescending(p => p.Score).ToArrayAsync();
        }

        public async Task<Player> GetPlayerByConnectionId(string connectionId)
        {
            return await muquizContext.Player.SingleOrDefaultAsync(s => s.ConnectionId == connectionId);
        }

        public async Task<string> GetHostConnectionIdByGameId(string gameId)
        {
            var result = await muquizContext.GameSession.SingleAsync(s => s.GameId == gameId);
            return result.HostConnectionId;
        }

        internal async Task<string> GetGameIdByHostConnectionId(string connectionId)
        {
            var result = await muquizContext.GameSession.SingleOrDefaultAsync(g => g.HostConnectionId == connectionId);
            return result.GameId;
        }

        internal async Task RemovePlayerByConnectionId(string connectionId)
        {
            var playerToRemove = await muquizContext.Player.SingleOrDefaultAsync(p => p.ConnectionId == connectionId);
            muquizContext.Player.Remove(playerToRemove);
            await muquizContext.SaveChangesAsync();
        }

        internal async Task RemoveGameSession(string connectionId)
        {
            var playersToRemove = await muquizContext.Player.Where(p => p.GameSession.HostConnectionId == connectionId).ToListAsync();
            muquizContext.Player.RemoveRange(playersToRemove);

            var gameSessionToRemove = await muquizContext.GameSession.SingleOrDefaultAsync(g => g.HostConnectionId == connectionId);
            muquizContext.GameSession.Remove(gameSessionToRemove);

            await muquizContext.SaveChangesAsync();
        }

        public bool EvaluateAnswer(string connectionId, string answer)
        {
            return muquizContext.Question.Count(q => answer == q.CorrectAnswer) > 0;
        }

        internal async Task UpdateScore(string connectionId, int count)
        {
            if (count > 3)
                count = 4;

            muquizContext.Player
                    .SingleOrDefault(p => p.ConnectionId == connectionId)
                    .Score += 1000 + 100 * (4 - count);
            await muquizContext.SaveChangesAsync();
        }
    }
}
