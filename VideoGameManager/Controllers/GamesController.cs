using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoGameManager.DataAccess;

namespace VideoGameManager.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly VideoGameDataContext context;

        // Constructor Injection
        public GamesController(VideoGameDataContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IEnumerable<Game> GetAllGames() => context.Games;

        [HttpGet]
        [Route("{id}")] // passes id to the method
        public Game GetGameByID(int id) => context.Games.FirstOrDefault(g => g.ID == id);
        
        [HttpPost]
        public async Task<Game> AddGame([FromBody] Game newGame)
        {
            context.Add(newGame);
            await context.SaveChangesAsync();
                return newGame;
        }
    }
}
