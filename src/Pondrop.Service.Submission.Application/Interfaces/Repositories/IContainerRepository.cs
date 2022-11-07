﻿using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Interfaces;

public interface IContainerRepository<T>
{
    Task<bool> IsConnectedAsync();

    Task<T?> UpsertAsync(T item);

    Task<List<T>> GetAllAsync();

    Task<T?> GetByIdAsync(Guid id);

    Task<List<T>> QueryAsync(string sqlQueryText, Dictionary<string, string>? parameters = null);
}