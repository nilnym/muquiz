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
    public class PlayerController : Controller
    {
        private readonly SessionStorageService sessionService;
        private readonly QuestionService questionService;

        public PlayerController(SessionStorageService sessionStorageService, QuestionService questionService)
        {
            sessionService = sessionStorageService;
            this.questionService = questionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(PlayerIndexVM vm)
        {
            //to-do: validate whether key/gameId is still valid when the player submits name
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            sessionService.Name = vm.Name;

            return RedirectToAction(nameof(Game));
        }

        public IActionResult Game()
        {
            var vm = new PlayerGameVM
            {
                GameId = sessionService.GameId,
                Name = sessionService.Name,
            };

            return View(vm);
        }

        public IActionResult ShowAlternatives(int song)
        {
            var alternatives = questionService.GetQuestionsForId(song);
            return PartialView("~/Views/Shared/Player/_Alternatives.cshtml", alternatives);
        }

        public IActionResult ShowWaitingScreen()
        {
            return PartialView("~/Views/Shared/Player/_WaitingScreen.cshtml");
        }

        public IActionResult ShowFinalPosition(int position)
        {
            //to-do: instead receive userId and find position in DB?
            return PartialView("~/Views/Shared/Player/_FinalPosition.cshtml", position);
        }
    }
}