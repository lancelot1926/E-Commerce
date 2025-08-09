using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Common;

public class Result
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }

    public static Result Ok() => new() { Succeeded = true };
    public static Result Fail(string error) => new() { Succeeded = false, Error = error };
}

public class Result<T> : Result
{
    public T? Data { get; init; }
    public static Result<T> Ok(T data) => new() { Succeeded = true, Data = data };
    public new static Result<T> Fail(string error) => new() { Succeeded = false, Error = error };
}