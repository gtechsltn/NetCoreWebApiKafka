﻿using MediatR;
using NetCoreWebApiKafka.Application.Exceptions;
using NetCoreWebApiKafka.Application.Interfaces;
using NetCoreWebApiKafka.Application.Interfaces.Repositories;
using NetCoreWebApiKafka.Application.Wrappers;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreWebApiKafka.Application.Features.Positions.Commands.UpdatePosition
{
    public class UpdatePositionCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }
        public string PositionTitle { get; set; }
        public string PositionDescription { get; set; }
        public decimal PositionSalary { get; set; }

        public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, Response<Guid>>
        {
            private readonly IPositionRepositoryAsync _positionRepository;
            private readonly IProducerService _producerService;

            public UpdatePositionCommandHandler(IPositionRepositoryAsync positionRepository, IProducerService producerService)
            {
                _positionRepository = positionRepository;
                _producerService = producerService;
            }

            public async Task<Response<Guid>> Handle(UpdatePositionCommand command, CancellationToken cancellationToken)
            {
                var position = await _positionRepository.GetByIdAsync(command.Id);

                if (position == null)
                {
                    throw new ApiException($"Position Not Found.");
                }
                else
                {
                    position.PositionTitle = command.PositionTitle;
                    position.PositionSalary = command.PositionSalary;
                    position.PositionDescription = command.PositionDescription;
                    await _positionRepository.UpdateAsync(position);

                    var message = JsonSerializer.Serialize(position);

                    await _producerService.ProduceAsync("UpdatedPositions", message);


                    return new Response<Guid>(position.Id);
                }
            }
        }
    }
}