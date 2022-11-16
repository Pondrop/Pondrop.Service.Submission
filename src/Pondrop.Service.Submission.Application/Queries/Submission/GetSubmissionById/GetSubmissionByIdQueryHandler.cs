using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Submission.Application.Interfaces;
using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Application.Queries.Submission.GetAllSubmissions;
using Pondrop.Service.Submission.Application.Queries.Submission.GetSubmissionById;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Enums.User;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;
using System.Runtime.CompilerServices;

namespace Pondrop.Service.Submission.Application.Queries;

public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, Result<SubmissionViewRecord?>>
{

    private readonly ICheckpointRepository<SubmissionEntity> _submissionCheckpointRepository;
    private readonly ICheckpointRepository<StoreVisitEntity> _storeVisitCheckpointRepository;
    private readonly ICheckpointRepository<SubmissionTemplateEntity> _submissionTemplateCheckpointRepository;
    private readonly ICheckpointRepository<FieldEntity> _fieldCheckpointRepository;
    private readonly IContainerRepository<SubmissionWithStoreViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<GetSubmissionByIdQueryHandler> _logger;
    private readonly IContainerRepository<StoreViewRecord> _storeContainerRepository;
    private readonly IValidator<GetSubmissionByIdQuery> _validator;

    public GetSubmissionByIdQueryHandler(
        ICheckpointRepository<SubmissionEntity> submissionCheckpointRepository,
        ICheckpointRepository<StoreVisitEntity> storeVisitCheckpointRepository,
        ICheckpointRepository<SubmissionTemplateEntity> submissionTemplateCheckpointRepository,
        ICheckpointRepository<FieldEntity> fieldCheckpointRepository,
        IContainerRepository<StoreViewRecord> storeContainerRepository,
        IContainerRepository<SubmissionWithStoreViewRecord> containerRepository,
        IValidator<GetSubmissionByIdQuery> validator,
        IMapper mapper,
        IUserService userService,
        ILogger<GetSubmissionByIdQueryHandler> logger) : base()
    {
        _submissionCheckpointRepository = submissionCheckpointRepository;
        _storeVisitCheckpointRepository = storeVisitCheckpointRepository;
        _submissionTemplateCheckpointRepository = submissionTemplateCheckpointRepository;
        _fieldCheckpointRepository = fieldCheckpointRepository;
        _storeContainerRepository = storeContainerRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<SubmissionViewRecord?>> Handle(GetSubmissionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var result = default(Result<SubmissionViewRecord?>);

        try
        {
            var queryString = $"SELECT * FROM c WHERE c.id = '{query.Id}'";
            //queryString += _userService.CurrentUserType() == UserType.Shopper;
                //? $" AND c.createdBy = '{_userService.CurrentUserId()}'"
                //: string.Empty;

            queryString += " OFFSET 0 LIMIT 1";

            var entities = await _submissionCheckpointRepository.QueryAsync(queryString);

            if (entities != null || entities.Count != 0)
            {
                try
                {
                    var submissionEntity = entities.FirstOrDefault();

                    var submissionTemplateTask =
                        _submissionTemplateCheckpointRepository.GetByIdAsync(submissionEntity.SubmissionTemplateId);
                    var storeVisitTask = _storeVisitCheckpointRepository.GetByIdAsync(submissionEntity.StoreVisitId);

                    await Task.WhenAll(submissionTemplateTask, storeVisitTask);

                    var submissionTemplate = _mapper.Map<SubmissionTemplateRecord>(submissionTemplateTask.Result);
                    var storeVisit = _mapper.Map<StoreVisitRecord>(storeVisitTask.Result);

                    if (submissionTemplate == null || storeVisit == null)
                        return result;

                    var store = await _storeContainerRepository.GetByIdAsync(storeVisit.StoreId);

                    if (store == null)
                        return result;

                    var steps = new List<SubmissionStepWithDetailsViewRecord>();

                    foreach (var step in submissionEntity.Steps)
                    {
                        var fields = new List<SubmissionFieldWithDetailsViewRecord>();

                        var templateStep = submissionTemplate.Steps.FirstOrDefault(s => s.Id == step.TemplateStepId);

                        if (templateStep != null)
                            foreach (var field in step.Fields)
                            {
                                var templateFieldId =
                                    templateStep.FieldIds.FirstOrDefault(s => s == field.TemplateFieldId);

                                var templateField = await _fieldCheckpointRepository.GetByIdAsync(templateFieldId);
                                if (templateField != null)
                                    fields.Add(new SubmissionFieldWithDetailsViewRecord(field.Id, field.TemplateFieldId,
                                        templateField?.Label ?? string.Empty, templateField?.FieldType ?? SubmissionFieldType.unknown,
                                        field.Values));
                            }

                        steps.Add(new SubmissionStepWithDetailsViewRecord(step.Id, step.TemplateStepId,
                            templateStep?.Title ?? string.Empty, fields));
                    }

                    var submissionView = new SubmissionViewRecord(submissionEntity.Id, submissionEntity.StoreVisitId,
                        submissionEntity.SubmissionTemplateId, storeVisit.StoreId, submissionTemplate.Title, submissionEntity.CreatedUtc, store.Name,
                        store.Retailer.Name, submissionEntity.Latitude, submissionEntity.Longitude, steps,
                        submissionEntity.CreatedBy, submissionEntity.UpdatedBy, submissionEntity.CreatedUtc,
                        submissionEntity.UpdatedUtc);


                    result = submissionView is not null
                        ? Result<SubmissionViewRecord?>.Success(submissionView)
                        : Result<SubmissionViewRecord?>.Success(null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    result = Result<SubmissionViewRecord?>.Error(ex);

                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<SubmissionViewRecord?>.Error(ex);
        }

        return result;
    }

}
