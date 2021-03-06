﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Instructors
{
    public class Delete : PageModel
    {
        private readonly IMediator _mediator;

        public Delete(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public async Task<ActionResult> OnPostAsync()
        {
            await _mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index));
        }

        public class Query : IRequest<Command>
        {
            public int? Id { get; set; }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(m => m.Id).NotNull();
            }
        }

        public class Command : IRequest
        {
            public int? ID { get; set; }

            public string LastName { get; set; }
            [Display(Name = "First Name")]
            public string FirstMidName { get; set; }

            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
            public DateTime? HireDate { get; set; }

            [Display(Name = "Location")]
            public string OfficeAssignmentLocation { get; set; }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Instructor, Command>();
        }

        public class QueryHandler : AsyncRequestHandler<Query, Command>
        {
            private readonly SchoolContext _db;

            public QueryHandler(SchoolContext db) => _db = db;

            protected override Task<Command> Handle(Query message) => _db
                .Instructors
                .Where(i => i.Id == message.Id)
                .ProjectTo<Command>()
                .SingleOrDefaultAsync();
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly SchoolContext _db;

            public CommandHandler(SchoolContext db) => _db = db;

            protected override async Task Handle(Command message)
            {
                Instructor instructor = await _db.Instructors
                    .Include(i => i.OfficeAssignment)
                    .Where(i => i.Id == message.ID)
                    .SingleAsync();

                instructor.Handle(message);

                _db.Instructors.Remove(instructor);

                var department = await _db.Departments
                    .Where(d => d.InstructorID == message.ID)
                    .SingleOrDefaultAsync();
                if (department != null)
                {
                    department.InstructorID = null;
                }

            }
        }
    }
}