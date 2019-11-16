using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using contractWhist2.Data;
using contractWhist2.Models;

namespace contractWhist2.Controllers
{
    public class cardsController : Controller
    {
        private readonly whistContext _context;

        public cardsController(whistContext context)
        {
            _context = context;
        }

        // GET: cards
        public async Task<IActionResult> Index()
        {
            return View(await _context.card.ToListAsync());
        }

        // GET: cards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.card
                .FirstOrDefaultAsync(m => m.id == id);
            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // GET: cards/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: cards/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,name,fullname,valueInSuit,suit")] card card)
        {
            if (ModelState.IsValid)
            {
                _context.Add(card);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(card);
        }

        // GET: cards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.card.FindAsync(id);
            if (card == null)
            {
                return NotFound();
            }
            return View(card);
        }

        // POST: cards/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,name,fullname,valueInSuit,suit")] card card)
        {
            if (id != card.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(card);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!cardExists(card.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(card);
        }

        // GET: cards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.card
                .FirstOrDefaultAsync(m => m.id == id);
            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // POST: cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var card = await _context.card.FindAsync(id);
            _context.card.Remove(card);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool cardExists(int id)
        {
            return _context.card.Any(e => e.id == id);
        }
    }
}
