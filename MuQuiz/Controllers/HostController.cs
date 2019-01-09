﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MuQuiz.Models;
using MuQuiz.Models.ViewModels;

namespace MuQuiz.Controllers
{
    public class HostController : Controller
    {
        HostService service;
        SpotifyService spotify;

        public HostController(HostService service, SpotifyService spotify)
        {
            this.service = service;
            this.spotify = spotify;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new HostIndexVM { GameId = service.GenerateGameId() };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Index(string gameId)
        {
            HttpContext.Session.SetString("GameId", gameId);
            return RedirectToAction(nameof(HostGame));
        }

        public IActionResult HostGame(string gameId)
        {
            var vm = new HostGameVM { GameId = HttpContext.Session.GetString("GameId") };
            return View(vm);
        }

        public IActionResult ShowAlternatives(string song)
        {
            // to-do: retrieve alternatives from data storage
            return PartialView("~/Views/Shared/Host/_Alternatives.cshtml");
        }

        public IActionResult ShowResults()
        {
            return PartialView("~/Views/Shared/Host/_Results.cshtml");
        }

        public IActionResult ShowFinalResults()
        {
            return PartialView("~/Views/Shared/Host/_FinalResults.cshtml");
        }
    }
}